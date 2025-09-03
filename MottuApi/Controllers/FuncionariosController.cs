using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuApi.Data;
using MottuApi.Models;

namespace MottuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionariosController : ControllerBase
    {
        private readonly MottuDbContext _context;

        public FuncionariosController(MottuDbContext context)
        {
            _context = context;
        }

        // GET: api/Funcionarios?page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios(int page = 1, int pageSize = 10)
        {
            var totalItems = await _context.Funcionarios.CountAsync();
            var funcionarios = await _context.Funcionarios
                .Include(f => f.Patio)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                Data = funcionarios,
                Links = new
                {
                    Self = Url.Action("GetFuncionarios", new { page, pageSize }),
                    Next = page * pageSize < totalItems ? Url.Action("GetFuncionarios", new { page = page + 1, pageSize }) : null,
                    Previous = page > 1 ? Url.Action("GetFuncionarios", new { page = page - 1, pageSize }) : null
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

        // GET: api/Funcionarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Funcionario>> GetFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios
                .Include(f => f.Patio)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcionario == null)
            {
                return NotFound();
            }

            var result = new
            {
                Data = funcionario,
                Links = new
                {
                    Self = Url.Action("GetFuncionario", new { id }),
                    Update = Url.Action("PutFuncionario", new { id }),
                    Delete = Url.Action("DeleteFuncionario", new { id })
                }
            };

            return Ok(result);
        }

        // PUT: api/Funcionarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFuncionario(int id, Funcionario funcionario)
        {
            if (id != funcionario.Id)
            {
                return BadRequest();
            }

            _context.Entry(funcionario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FuncionarioExists(id))
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

        // POST: api/Funcionarios
        [HttpPost]
        public async Task<ActionResult<Funcionario>> PostFuncionario(Funcionario funcionario)
        {
            _context.Funcionarios.Add(funcionario);
            await _context.SaveChangesAsync();

            var result = new
            {
                Data = funcionario,
                Links = new
                {
                    Self = Url.Action("GetFuncionario", new { id = funcionario.Id }),
                    Update = Url.Action("PutFuncionario", new { id = funcionario.Id }),
                    Delete = Url.Action("DeleteFuncionario", new { id = funcionario.Id })
                }
            };

            return CreatedAtAction("GetFuncionario", new { id = funcionario.Id }, result);
        }

        // DELETE: api/Funcionarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null)
            {
                return NotFound();
            }

            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FuncionarioExists(int id)
        {
            return _context.Funcionarios.Any(e => e.Id == id);
        }
    }
}