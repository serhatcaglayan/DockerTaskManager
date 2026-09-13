using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controller servislerini ekleme
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Ayarları: Şimdilik geliştirme ortamında olduğumuz için her şeye izin veriyoruz (AllowAnyOrigin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // Herhangi bir adresten (IP/Port) gelen isteklere izin ver
              .AllowAnyHeader()    // Her türlü başlığa (Header) izin ver
              .AllowAnyMethod();   // Bütün HTTP metodlarına (GET, POST, PUT, DELETE) izin ver
    });
});

var app = builder.Build();

// OTOMATİK TABLO OLUŞTURMA (MIGRATION) ---
// Uygulama başlarken veritabanına bağlanıp tabloları oluşturacak
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");


app.MapControllers(); 

app.Run();