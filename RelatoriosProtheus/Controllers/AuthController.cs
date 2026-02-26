using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelatoriosProtheus.Data;
using RelatoriosProtheus.Models.Entities;
using RelatoriosProtheus.Models.ViewModels;
using RelatoriosProtheus.Utils;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using FluentEmail.Core;
using System;
using System.Linq;

namespace RelatoriosProtheus.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _toast;
        private readonly IFluentEmail _mail;

        public AuthController(ILogger<AuthController> logger, ApplicationDbContext context, INotyfService toast, IFluentEmail mail)
        {
            _logger = logger;
            _context = context;
            _toast = toast;
            _mail = mail;
        }

        public IActionResult Index()
        {
            var usuarios = _context.Funcionarios
                .ToList();

            List<UsuarioViewModel> usuariosPerfis = new List<UsuarioViewModel>();

            foreach (var item in usuarios)
            {
                var perfisUsuario = _context.Perfis_Funcionarios
                    .Include(pf => pf.PerfilFk)
                    .Where(pf => pf.FuncionarioId == item.Id)
                    .Select(pf => pf.PerfilFk.Descricao);

                usuariosPerfis.Add(new UsuarioViewModel
                {
                    Matricula = item.Matricula,
                    FuncionarioId = item.Id,
                    Email = item.Email,
                    Perfis = perfisUsuario
                });
            }

            ViewBag.Perfis = _context.Perfis
                .Where(p => p.Descricao != "Administrador")
                .ToList();

            var perfis = _context.Perfis.ToList();
            return View(usuariosPerfis);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            var vm = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login([Bind("Empresa,Matricula,Senha")] LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            try
            {
                var empresaEnum = EnumsUtils.retornarEnum(login.Empresa);

                var funcionario = _context.Funcionarios
                    .Include(f => f.Perfis)
                    .ThenInclude(pf => pf.PerfilFk)
                    .FirstOrDefault(f =>
                        f.Empresa == empresaEnum &&
                        f.Matricula == login.Matricula &&
                        f.Senha == login.Senha);

                if (funcionario == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuário não encontrado, por favor tente novamente.");
                    return View(login);
                }

                AutenticarIdentity(funcionario, login.RememberMe);

                if (!string.IsNullOrWhiteSpace(login.ReturnUrl))
                    return LocalRedirect(login.ReturnUrl);

                return RedirectToAction("Index", "Relatorios");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao processar o login.");
                return View(login);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("Login");
        }

        [AllowAnonymous]
        public IActionResult RecuperarSenha()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarSenha([Bind("Email")]EsqueceuSenhaViewModel vw)
        {
            if (ModelState.IsValid)
            {
                var func = _context.Funcionarios
                    .Where(f => f.Email == vw.Email && f.Situacao != 0)
                    .FirstOrDefault();

                if (func != null)
                {
                    var model = new EsqueceuSenhaViewModel
                    {
                        Nome = func.Nome,
                        Email = func.Email,
                        Matricula = func.Matricula,
                        Senha = func.Senha
                    };
                    await EnviarEmail(model);
                }
                else
                {
                    _toast.Error("E-mail não identificado.");
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }


        [HttpPost]
        public IActionResult Create(Funcionarios funcionario)
        {
            if (_context.Funcionarios.Any((f => f.Matricula == funcionario.Matricula && f.Empresa == funcionario.Empresa)))
            {
                _toast.Error("Já existe um usuário cadastrado com essa matrícula.");
                return View(funcionario);
            }

            funcionario.Situacao = 1;

            _context.Funcionarios.Add(funcionario);
            _context.SaveChanges();

            return RedirectToAction("Index", "Relatorios");
        }

        public IActionResult Edit(int id)
        {
            var usuario = _context.Funcionarios.Find(id);
            var func = new FuncionarioEditViewModel()
            {
                Email = usuario.Email,
                Empresa = usuario.Empresa,
                Id = usuario.Id,
                Matricula = usuario.Matricula,
                Nome = usuario.Nome,
                Senha = usuario.Senha,
                Situacao = usuario.Situacao == 1 ? true : false
            };
            return View(func);
        }

        [HttpPost]
        public IActionResult Edit(Funcionarios usuario)
        {
            var usuarioDb = _context.Funcionarios.FirstOrDefault(u => u.Id == usuario.Id);
            if (usuarioDb != null)
            {
                usuarioDb.Matricula = usuario.Matricula;
                usuarioDb.Nome = usuario.Nome;
                usuarioDb.Email = usuario.Email;
                usuarioDb.Senha = usuario.Senha;
                usuarioDb.Situacao = usuario.Situacao;
                usuarioDb.Empresa = usuario.Empresa;
                _context.Funcionarios.Update(usuarioDb);
                _context.SaveChanges();
            }
            return RedirectToAction("Index", "Relatorios");
        }

        public async Task<IActionResult> Perfis(int Id)
        {
            var idClaim = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userClaim = _context.Funcionarios.Where(x => x.Id == idClaim).Include(pf => pf.Perfis).ThenInclude(y => y.PerfilFk).FirstOrDefault();
            
            var usuario = await _context.Funcionarios.FindAsync(Id);

            var perfisUsuario = _context.Perfis_Funcionarios
                .Include(pf => pf.PerfilFk)
                .Where(pf => pf.FuncionarioId == Id && pf.PerfilId != 0)
                .Select(pf => pf.PerfilFk.Descricao)
                .ToList();

            var isAdm = userClaim.Perfis.Select(pf => pf.PerfilFk.Descricao).Contains("Administrador");

            var todosPerfis = _context.Perfis
                .Select(p => p.Descricao)
                .ToList();

            var disponiveis = todosPerfis.Except(perfisUsuario);

            if (!isAdm)
            {
                disponiveis = disponiveis.Where(p => p != "Administrador");
            }

            var usuarioPerfis = new UsuarioViewModel
            {
                Matricula = usuario.Matricula,
                FuncionarioId = usuario.Id,
                Email = usuario.Email,
                Perfis = perfisUsuario.AsQueryable()
            };

            ViewBag.Perfis = disponiveis.ToList();

            return View(usuarioPerfis);
        }

        [HttpPost]
        public async Task<bool> Perfil(int funcionarioId, string[] perfisUsuario)
        {
            try
            {
                var listaPerfisFuncionario = _context.Perfis_Funcionarios
                .Where(pf => pf.FuncionarioId == funcionarioId && pf.PerfilId != 1)
                .ToList();

                _context.Perfis_Funcionarios.RemoveRange(listaPerfisFuncionario);
                await _context.SaveChangesAsync();

                var perfis = _context.Perfis.ToList();

                for (int i = 0; i < perfisUsuario.Length; i++)
                {
                    var perfilId = perfis
                        .Where(p => p.Descricao == perfisUsuario[i])
                        .Select(p => p.Id)
                        .First();

                    Perfis_Funcionarios perfisFuncionarios = new Perfis_Funcionarios
                    {
                        FuncionarioId = funcionarioId,
                        PerfilId = perfilId,
                    };

                    _context.Perfis_Funcionarios.Add(perfisFuncionarios);
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
        public async Task<IActionResult> ChangePerfilUsuario(int FuncionarioId, string Perfil, bool active)
        {
            var usuario = await _context.Funcionarios.FindAsync(FuncionarioId);

            if (usuario != null)
            {
                if (active)
                {
                    AddToPerfil(FuncionarioId, Perfil);
                }
                else
                {
                    RemoveFromPerfil(FuncionarioId, Perfil);
                }
            }

            return Json("ok");
        }

        #region Metódos Auxiliares

        private async void AutenticarIdentity(Funcionarios f, bool RememberMe)
        {
            try
            {
                List<Claim> claims = [
                        new Claim(ClaimTypes.NameIdentifier, f.Id.ToString()),
                        new Claim(ClaimTypes.Name, f.Email)
                    ];

                if (!f.Perfis.IsNullOrEmpty())
                {
                    foreach (var claim in f.Perfis)
                    {
                        claims.Add(new Claim("role", claim.PerfilFk.Descricao));
                    }
                }

                var authScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                var identity = new ClaimsIdentity(claims, authScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(authScheme, principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = RememberMe
                    });
            }
            catch (Exception)
            {

                throw;
            }

        }

        private void AddToPerfil(int FuncionarioId, string Perfil)
        {
            try
            {
                var perfilFind = _context.Perfis.Where(p => p.Descricao == Perfil).FirstOrDefault();
                if (perfilFind != null)
                {
                    var perfil = new Perfis_Funcionarios
                    {
                        FuncionarioId = FuncionarioId,
                        PerfilId = perfilFind.Id
                    };
                    _context.Add(perfil);
                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void RemoveFromPerfil(int FuncionarioId, string Perfil)
        {
            try
            {
                var perfilFind = _context.Perfis.Where(p => p.Descricao == Perfil).FirstOrDefault();

                if (perfilFind != null)
                {
                    var perfilFuncionario = _context.Perfis_Funcionarios.Where(pf => pf.PerfilId == perfilFind.Id
                    && pf.FuncionarioId == FuncionarioId).FirstOrDefault();

                    if (perfilFuncionario != null)
                    {
                        _context.Perfis_Funcionarios.Remove(perfilFuncionario);
                    }

                    _context.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task EnviarEmail(EsqueceuSenhaViewModel model)
        {
            try
            {
                string toSend = $"{model.Email}";

                var sendEmail = _mail
                    .To(toSend)
                    .Subject("Esqueceu a senha - Sistema Relatórios Protheus")
                    .UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}/Templates/EsqueceuSenhaTemplate.cshtml", model);

                await sendEmail.SendAsync();
                _toast.Success("Verifique seu e-mail.");
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
    }
}
