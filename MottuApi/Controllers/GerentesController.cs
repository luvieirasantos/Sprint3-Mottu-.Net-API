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

        /// <summary>
        /// Obtém uma lista paginada de gerentes.
        /// </summary>
        /// <param name="page">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10).</param>
        /// <returns>Lista paginada de gerentes com links HATEOAS.</returns>
        /// <response code="200">Retorna a lista de gerentes.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gerente>>> GetGerentes(int page = 1, int pageSize = 10)
        {
            var totalItems = await _context.Gerentes.CountAsync();
            var gerentes = await _context.Gerentes
                .Include(g => g.Funcionario)
                .Include(g => g.Patio)
                .OrderBy(g => g.Id)
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

        /// <summary>
        /// Obtém um gerente específico pelo ID.
        /// </summary>
        /// <param name="id">ID do gerente.</param>
        /// <returns>O gerente solicitado com links HATEOAS.</returns>
        /// <response code="200">Retorna o gerente.</response>
        /// <response code="404">Gerente não encontrado.</response>
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

        /// <summary>
        /// Atualiza um gerente existente.
        /// </summary>
        /// <param name="id">ID do gerente.</param>
        /// <param name="gerente">Dados atualizados do gerente.</param>
        /// <response code="204">Gerente atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="404">Gerente não encontrado.</response>
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

        /// <summary>
        /// Cria um novo gerente.
        /// </summary>
        /// <param name="gerente">Dados do gerente a ser criado.</param>
        /// <returns>O gerente criado com links HATEOAS.</returns>
        /// <response code="201">Gerente criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
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

        /// <summary>
        /// Exclui um gerente pelo ID.
        /// </summary>
        /// <param name="id">ID do gerente.</param>
        /// <response code="204">Gerente excluído com sucesso.</response>
        /// <response code="404">Gerente não encontrado.</response>
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