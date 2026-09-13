using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Controllers;
using TaskAPI.Data;
using TaskAPI.Models;

namespace TaskAPI.Tests
{
    public class ProjectsControllerTests
    {
        [Fact]
        public async Task CreateProject_AddsProjectToDatabase()
        {
            // 1. ARRANGE (Hazýrlýk): Test için temiz, sanal bir veritabaný ayarlýyoruz
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var newProject = new Project { Name = "Test Projesi", Description = "Birim Test" };

            using (var context = new AppDbContext(options))
            {
                var controller = new ProjectsController(context);

                // 2. ACT (Eylem): Yazdýðýmýz API metodunu tetikliyoruz (Sanki istek gelmiþ gibi)
                var result = await controller.CreateProject(newProject);

                // 3. ASSERT (Doðrulama): Sonuç beklediðimiz gibi mi?
                Assert.IsType<CreatedAtActionResult>(result); // 201 Created döndü mü?
                Assert.Equal(1, context.Projects.Count()); // Veritabanýna 1 kayýt eklendi mi?
                Assert.Equal("Test Projesi", context.Projects.First().Name); // Eklenen kaydýn adý doðru mu?
            }
        }
    }
}