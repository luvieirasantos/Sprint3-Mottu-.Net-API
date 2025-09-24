using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MottuApi.Data;
using MottuApi.Models;
using MottuApi.Services;

namespace MottuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionariosController : ControllerBase
    {
        private readonly MottuDbContext _context;
        private readonly AuthService _authService;

        public FuncionariosController(MottuDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Obtém uma lista paginada de funcionários.
        /// </summary>
        /// <param name="page">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Tamanho da página (padrão: 10).</param>
        /// <returns>Lista paginada de funcionários com links HATEOAS.</returns>
        /// <response code="200">Retorna a lista de funcionários.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncionarioResponseDto>>> GetFuncionarios(int page = 1, int pageSize = 10)
        {
            var totalItems = await _context.Funcionarios.CountAsync();
            var funcionarios = await _context.Funcionarios
                .Include(f => f.Patio)
                .OrderBy(f => f.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FuncionarioResponseDto
                {
                    Id = f.Id,
                    Nome = f.Nome,
                    Email = f.Email,
                    PatioId = f.PatioId,
                    Patio = f.Patio
                })
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

        /// <summary>
        /// Obtém um funcionário específico pelo ID.
        /// </summary>
        /// <param name="id">ID do funcionário.</param>
        /// <returns>O funcionário solicitado com links HATEOAS.</returns>
        /// <response code="200">Retorna o funcionário.</response>
        /// <response code="404">Funcionário não encontrado.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<FuncionarioResponseDto>> GetFuncionario(int id)
        {
            var funcionario = await _context.Funcionarios
                .Include(f => f.Patio)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcionario == null)
            {
                return NotFound();
            }

            var funcionarioResponse = new FuncionarioResponseDto
            {
                Id = funcionario.Id,
                Nome = funcionario.Nome,
                Email = funcionario.Email,
                PatioId = funcionario.PatioId,
                Patio = funcionario.Patio
            };

            var result = new
            {
                Data = funcionarioResponse,
                Links = new
                {
                    Self = Url.Action("GetFuncionario", new { id }),
                    Update = Url.Action("PutFuncionario", new { id }),
                    Delete = Url.Action("DeleteFuncionario", new { id })
                }
            };

            return Ok(result);
        }

        /// <summary>
        /// Atualiza um funcionário existente.
        /// </summary>
        /// <param name="id">ID do funcionário.</param>
        /// <param name="funcionario">Dados atualizados do funcionário.</param>
        /// <response code="204">Funcionário atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="404">Funcionário não encontrado.</response>
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

        /// <summary>
        /// Cria um novo funcionário.
        /// </summary>
        /// <param name="funcionarioDto">Dados do funcionário a ser criado.</param>
        /// <returns>O funcionário criado com links HATEOAS.</returns>
        /// <response code="201">Funcionário criado com sucesso.</response>
        /// <response code="400">Dados inválidos ou email já cadastrado.</response>
        [HttpPost]
        public async Task<ActionResult<FuncionarioResponseDto>> PostFuncionario(FuncionarioCreateDto funcionarioDto)
        {
            // Verificar se o pátio existe
            var patioExists = await _context.Patios.AnyAsync(p => p.Id == funcionarioDto.PatioId);
            if (!patioExists)
            {
                return BadRequest("Pátio não encontrado");
            }

            // Verificar se o email já existe
            var emailExists = await _context.Funcionarios.AnyAsync(f => f.Email == funcionarioDto.Email);
            if (emailExists)
            {
                return BadRequest("Email já cadastrado");
            }

            var funcionario = new Funcionario
            {
                Nome = funcionarioDto.Nome,
                Email = funcionarioDto.Email,
                Senha = _authService.HashPassword(funcionarioDto.Senha),
                PatioId = funcionarioDto.PatioId
            };
            
            _context.Funcionarios.Add(funcionario);
            await _context.SaveChangesAsync();

            var funcionarioResponse = new FuncionarioResponseDto
            {
                Id = funcionario.Id,
                Nome = funcionario.Nome,
                Email = funcionario.Email,
                PatioId = funcionario.PatioId,
                Patio = funcionario.Patio
            };

            var result = new
            {
                Data = funcionarioResponse,
                Links = new
                {
                    Self = Url.Action("GetFuncionario", new { id = funcionario.Id }),
                    Update = Url.Action("PutFuncionario", new { id = funcionario.Id }),
                    Delete = Url.Action("DeleteFuncionario", new { id = funcionario.Id })
                }
            };

            return CreatedAtAction("GetFuncionario", new { id = funcionario.Id }, result);
        }

        /// <summary>
        /// Exclui um funcionário pelo ID.
        /// </summary>
        /// <param name="id">ID do funcionário.</param>
        /// <response code="204">Funcionário excluído com sucesso.</response>
        /// <response code="404">Funcionário não encontrado.</response>
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