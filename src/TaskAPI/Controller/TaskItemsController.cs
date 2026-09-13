using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Models;
using RabbitMQ.Client;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using TaskAPI.Hubs

namespace TaskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext; 

        public TaskItemsController(AppDbContext context)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/taskitems/project/5 (Belirli bir projeye ait görevleri getirir)
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(int projectId)
        {
            var tasks = await _context.TaskItems
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            if (!tasks.Any())
                return NotFound("Bu projeye ait görev bulunamadı.");

            return Ok(tasks);
        }

        // POST: api/taskitems
        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskItem taskItem)
        {
            // Görevin ekleneceği proje gerçekten veritabanında var mı?
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == taskItem.ProjectId);
            if (!projectExists)
                return BadRequest("Geçersiz Proje ID'si. Önce projeyi oluşturmalısınız.");

            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            // --- RABBITMQ MESAJ GÖNDERME BAŞLANGICI ---
            try
            {
                // Docker içindeki RabbitMQ servisimizin adı: c_rabbitmq
                var factory = new ConnectionFactory() { HostName = "c_rabbitmq" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // 'task_queue' adında bir kuyruk oluştur (eğer yoksa)
                    channel.QueueDeclare(queue: "task_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    // Gönderilecek mesajı hazırla
                    string message = $"Yeni Görev Eklendi: {taskItem.Title}";
                    var body = Encoding.UTF8.GetBytes(message);

                    // Mesajı kuyruğa fırlat
                    channel.BasicPublish(exchange: "", routingKey: "task_queue", basicProperties: null, body: body);
                }
            }
            catch (Exception ex)
            {
                // RabbitMQ çökerse ana API hata vermesin, sadece loglasın diye Try-Catch içine aldık
                Console.WriteLine($"RabbitMQ Hatası: {ex.Message}");
            }
            // --- RABBITMQ MESAJ GÖNDERME BİTİŞİ ---

            // Tüm bağlı tarayıcılara "ReceiveNotification" adıyla canlı bir olay fırlat
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Yeni Görev Sistemde: {taskItem.Title}");

            return Ok(taskItem);
        }
        // PUT: api/taskitems/id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id)
                return BadRequest("ID uyuşmazlığı.");

            // Proje ID'si geçerli mi kontrolü
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == taskItem.ProjectId);
            if (!projectExists)
                return BadRequest("Bağlanmaya çalışılan Proje ID'si geçersiz.");

            _context.Entry(taskItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TaskItems.Any(e => e.Id == id))
                    return NotFound("Güncellenmek istenen görev bulunamadı.");
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/taskitems/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
                return NotFound("Silinecek görev bulunamadı.");

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}