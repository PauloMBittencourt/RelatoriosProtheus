using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RelatoriosProtheus.Data;
using RelatoriosProtheus.Models.Entities;
using RelatoriosProtheus.Models.ViewModels;

namespace RelatoriosProtheus.Controllers
{
    [Authorize]
    public class GruposController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GruposController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Grupoes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Grupos.ToListAsync());
        }

        // GET: Grupoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupo = await _context.Grupos
                .FirstOrDefaultAsync(m => m.GrupoId == id);
            if (grupo == null)
            {
                return NotFound();
            }

            return View(grupo);
        }

        // GET: Grupoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Grupoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GrupoId,Nome,Cor,GrupoSeparacao")] Grupo grupo)
        {
            if (!ModelState.IsValid)
            {
                // Loga todos os erros de validação
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, x.Value.Errors })
                    .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Errors.Select(e => e.ErrorMessage))}");
                }

                return View(grupo);
            }
            else
            {
                _context.Add(grupo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Grupoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null)
            {
                return NotFound();
            }
            return View(grupo);
        }

        // POST: Grupoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GrupoId,Nome,Cor,GrupoSeparacao")] Grupo grupo)
        {
            if (id != grupo.GrupoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grupo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrupoExists(grupo.GrupoId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(grupo);
        }

        // GET: Grupoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupo = await _context.Grupos
                .FirstOrDefaultAsync(m => m.GrupoId == id);
            if (grupo == null)
            {
                return NotFound();
            }

            return View(grupo);
        }

        // POST: Grupoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo != null)
            {
                _context.Grupos.Remove(grupo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Grupos(int Id)
        {
            var idClaim = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userClaim = _context.Funcionarios.Where(x => x.Id == idClaim).Include(pf => pf.Perfis).ThenInclude(y => y.PerfilFk).FirstOrDefault();

            var usuario = await _context.Funcionarios.FindAsync(Id);

            var gruposUsuarios = _context.FuncionarioGrupos
                .Include(pf => pf.FuncionarioFk)
                .Where(pf => pf.FuncionariosId == Id && pf.GrupoId != 0)
                .Select(pf => pf.GrupoFk.Nome)
                .ToList();

            var isAdm = userClaim.Perfis.Select(pf => pf.PerfilFk.Descricao).Contains("Administrador");

            var todosGrupos = _context.Grupos
                .Select(p => p.Nome)
                .ToList();

            var disponiveis = todosGrupos.Except(gruposUsuarios);

            if (!isAdm)
            {
                disponiveis = disponiveis.Where(p => p != "Administrador");
            }

            var usuarioGrupos = new UsuarioViewModel
            {
                Matricula = usuario.Matricula,
                FuncionarioId = usuario.Id,
                Email = usuario.Email,
                Grupos = gruposUsuarios.AsQueryable()
            };

            ViewBag.Grupos = disponiveis.ToList();

            return View(usuarioGrupos);
        }

        [HttpPost]
        public async Task<bool> Grupos(int funcionarioId, string[] gruposUsuario)
        {
            try
            {
                var listaGruposFuncionario = _context.FuncionarioGrupos
                .Where(pf => pf.FuncionariosId == funcionarioId)
                .ToList();

                _context.FuncionarioGrupos.RemoveRange(listaGruposFuncionario);
                await _context.SaveChangesAsync();

                var grupos = _context.Grupos.ToList();

                for (int i = 0; i < gruposUsuario.Length; i++)
                {
                    var grupoId = grupos
                        .Where(p => p.Nome == gruposUsuario[i])
                        .Select(p => p.GrupoId)
                        .First();

                    var gruposFuncionarios = new FuncionarioGrupo
                    {
                        FuncionariosId = funcionarioId,
                        GrupoId = grupoId,
                    };

                    _context.FuncionarioGrupos.Add(gruposFuncionarios);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeGrupoUsuario(int FuncionarioId, string Perfil, bool active)
        {
            var usuario = await _context.Funcionarios.FindAsync(FuncionarioId);

            if (usuario != null)
            {
                if (active)
                {
                    AddToGrupo(FuncionarioId, Perfil);
                }
                else
                {
                    RemoveFromGrupo(FuncionarioId, Perfil);
                }
            }

            return Json("ok");
        }

        #region Metodos Auxiliares
        private void AddToGrupo(int FuncionarioId, string Grupo)
        {
            try
            {
                var gruposFind = _context.Grupos.Where(p => p.Nome == Grupo).FirstOrDefault();
                if (gruposFind != null)
                {
                    var grupo = new FuncionarioGrupo
                    {
                        FuncionariosId = FuncionarioId,
                        GrupoId = gruposFind.GrupoId
                    };
                    _context.Add(grupo);
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void RemoveFromGrupo(int FuncionarioId, string Grupo)
        {
            try
            {
                var grupoFind = _context.Grupos.Where(p => p.Nome == Grupo).FirstOrDefault();

                if (grupoFind != null)
                {
                    var grupoFuncionario = _context.FuncionarioGrupos.Where(pf => pf.GrupoId == grupoFind.GrupoId
                    && pf.FuncionariosId == FuncionarioId).FirstOrDefault();

                    if (grupoFuncionario != null)
                    {
                        _context.FuncionarioGrupos.Remove(grupoFuncionario);
                    }

                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool GrupoExists(int id)
        {
            return _context.Grupos.Any(e => e.GrupoId == id);
        } 
        #endregion

    }
}
