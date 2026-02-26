using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PerfilsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PerfilsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Perfils
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Perfis.ToListAsync());
        }

        // GET: Perfils/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfis
                .FirstOrDefaultAsync(m => m.Id == id);
            if (perfil == null)
            {
                return NotFound();
            }

            return View(perfil);
        }

        // GET: Perfils/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Perfils/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Descricao")] PerfilCreateViewModel perfilVm)
        {
            if (!ModelState.IsValid)
                return View(perfilVm);

            var perfil = new Perfil
            {
                Descricao = perfilVm.Descricao
            };
            _context.Add(perfil);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Perfils/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfis.FindAsync(id);
            if (perfil == null)
                return NotFound();

            var perfilVm = new PerfilEditViewModel()
            {
                Id = perfil.Id,
                Descricao = perfil.Descricao
            };
            return View(perfilVm);
        }

        // POST: Perfils/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PerfilEditViewModel vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var perfil = new Perfil
                {
                    Id = vm.Id,
                    Descricao = vm.Descricao
                };

                _context.Update(perfil);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PerfilExists(vm.Id))
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

        // GET: Perfils/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfis
                .FirstOrDefaultAsync(m => m.Id == id);
            if (perfil == null)
            {
                return NotFound();
            }

            return View(perfil);
        }

        // POST: Perfils/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var perfil = await _context.Perfis.FindAsync(id);
            if (perfil != null)
            {
                _context.Perfis.Remove(perfil);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PerfilExists(int id)
        {
            return _context.Perfis.Any(e => e.Id == id);
        }
    }
}
