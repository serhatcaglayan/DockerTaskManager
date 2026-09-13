using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Models;

namespace TaskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskItemsController(AppDbContext context)
        {
            _context = context;
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