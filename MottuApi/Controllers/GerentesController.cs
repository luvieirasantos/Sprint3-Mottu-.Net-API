using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuApi.Data;
using MottuApi.Models;

namespace MottuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GerentesController : ControllerBase
    {
        private readonly MottuDbContext _context;

        public GerentesController(MottuDbContext context)
        {
            _context = context;
        }

        // GET: api/Gerentes?page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gerente>>> GetGerentes(int page = 1, int pageSize = 10)
        {
            var totalItems = await _context.Gerentes.CountAsync();
            var gerentes = await _context.Gerentes
                .Include(g => g.Funcionario)
                .Include(g => g.Patio)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                Data = gerentes,
                Links = new
                {
                    Self = Url.Action("GetGerentes", new { page, pageSize }),
                    Next = page * pageSize < totalItems ? Url.Action("GetGerentes", new { page = page + 1, pageSize }) : null,
                    Previous = page > 1 ? Url.Action("GetGerentes", new { page = page - 1, pageSize }) : null
                },
                Pagination = new
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                }
            };

            return Ok(result);
        }

        // GET: api/Gerentes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Gerente>> GetGerente(int id)
        {
            var gerente = await _context.Gerentes
                .Include(g => g.Funcionario)
                .Include(g => g.Patio)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gerente == null)
            {
                return NotFound();
            }

            var result = new
            {
                Data = gerente,
                Links = new
                {
                    Self = Url.Action("GetGerente", new { id }),
                    Update = Url.Action("PutGerente", new { id }),
                    Delete = Url.Action("DeleteGerente", new { id })
                }
            };

            return Ok(result);
        }

        // PUT: api/Gerentes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGerente(int id, Gerente gerente)
        {
            if (id != gerente.Id)
            {
                return BadRequest();
            }

            _context.Entry(gerente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GerenteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Gerentes
        [HttpPost]
        public async Task<ActionResult<Gerente>> PostGerente(Gerente gerente)
        {
            _context.Gerentes.Add(gerente);
            await _context.SaveChangesAsync();

            var result = new
            {
                Data = gerente,
                Links = new
                {
                    Self = Url.Action("GetGerente", new { id = gerente.Id }),
                    Update = Url.Action("PutGerente", new { id = gerente.Id }),
                    Delete = Url.Action("DeleteGerente", new { id = gerente.Id })
                }
            };

            return CreatedAtAction("GetGerente", new { id = gerente.Id }, result);
        }

        // DELETE: api/Gerentes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGerente(int id)
        {
            var gerente = await _context.Gerentes.FindAsync(id);
            if (gerente == null)
            {
                return NotFound();
            }

            _context.Gerentes.Remove(gerente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GerenteExists(int id)
        {
            return _context.Gerentes.Any(e => e.Id == id);
        }
    }
}