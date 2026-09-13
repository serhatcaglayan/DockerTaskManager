using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Models;

namespace TaskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/projects
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {

            var result = await _context.Projects
                .Select(project => new
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    TaskItems = _context.TaskItems.Where(task => task.ProjectId == project.Id).ToList()
                })
                .ToListAsync();

            return Ok(result);
        }

        // POST: api/projects
        [HttpPost]
        public async Task<IActionResult> CreateProject(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjects), new { id = project.Id }, project);
        }
        // PUT: api/projects/id
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, Project project)
        {
            // URL'deki ID ile gönderilen nesnenin ID'si eşleşmeli
            if (id != project.Id)
                return BadRequest("ID uyuşmazlığı. Lütfen doğru projeyi güncellediğinizden emin olun.");

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Projects.Any(e => e.Id == id))
                    return NotFound("Güncellenmek istenen proje bulunamadı.");
                else
                    throw;
            }

            return NoContent(); // 204: Başarılı ama geri dönecek veri yok
        }

        // DELETE: api/projects/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound("Silinecek proje bulunamadı.");

            // ÖNEMLİ: Projeyi silmeden önce, bu projeye bağlı olan görevleri siliyoruz.
            var relatedTasks = await _context.TaskItems.Where(t => t.ProjectId == id).ToListAsync();
            _context.TaskItems.RemoveRange(relatedTasks);

            // Şimdi projeyi güvenle silebiliriz
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            return Ok("TaskAPI v2 - CD Otomasyonu Harika Çalışıyor! 🚀");
        }
    }
}