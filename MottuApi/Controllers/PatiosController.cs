using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuApi.Data;
using MottuApi.Models;

namespace MottuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatiosController : ControllerBase
    {
        private readonly MottuDbContext _context;

        public PatiosController(MottuDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém uma lista paginada de pátios.
        /// </summary>
        /// <param name="page">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10).</param>
        /// <returns>Lista paginada de pátios com links HATEOAS.</returns>
        /// <response code="200">Retorna a lista de pátios.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patio>>> GetPatios(int page = 1, int pageSize = 10)
        {
            var totalItems = await _context.Patios.CountAsync();
            var patios = await _context.Patios
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                Data = patios,
                Links = new
                {
                    Self = Url.Action("GetPatios", new { page, pageSize }),
                    Next = page * pageSize < totalItems ? Url.Action("GetPatios", new { page = page + 1, pageSize }) : null,
                    Previous = page > 1 ? Url.Action("GetPatios", new { page = page - 1, pageSize }) : null
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

        /// <summary>
        /// Obtém um pátio específico pelo ID.
        /// </summary>
        /// <param name="id">ID do pátio.</param>
        /// <returns>O pátio solicitado com links HATEOAS.</returns>
        /// <response code="200">Retorna o pátio.</response>
        /// <response code="404">Pátio não encontrado.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Patio>> GetPatio(int id)
        {
            var patio = await _context.Patios.FindAsync(id);

            if (patio == null)
            {
                return NotFound();
            }

            var result = new
            {
                Data = patio,
                Links = new
                {
                    Self = Url.Action("GetPatio", new { id }),
                    Update = Url.Action("PutPatio", new { id }),
                    Delete = Url.Action("DeletePatio", new { id })
                }
            };

            return Ok(result);
        }

        // PUT: api/Patios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPatio(int id, Patio patio)
        {
            if (id != patio.Id)
            {
                return BadRequest();
            }

            _context.Entry(patio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatioExists(id))
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

        /// <summary>
        /// Cria um novo pátio.
        /// </summary>
        /// <param name="patio">Dados do pátio a ser criado.</param>
        /// <returns>O pátio criado com links HATEOAS.</returns>
        /// <response code="201">Pátio criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        public async Task<ActionResult<Patio>> PostPatio(Patio patio)
        {
            _context.Patios.Add(patio);
            await _context.SaveChangesAsync();

            var result = new
            {
                Data = patio,
                Links = new
                {
                    Self = Url.Action("GetPatio", new { id = patio.Id }),
                    Update = Url.Action("PutPatio", new { id = patio.Id }),
                    Delete = Url.Action("DeletePatio", new { id = patio.Id })
                }
            };

            return CreatedAtAction("GetPatio", new { id = patio.Id }, result);
        }

        // DELETE: api/Patios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatio(int id)
        {
            var patio = await _context.Patios.FindAsync(id);
            if (patio == null)
            {
                return NotFound();
            }

            _context.Patios.Remove(patio);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PatioExists(int id)
        {
            return _context.Patios.Any(e => e.Id == id);
        }
    }
}