using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RelatoriosProtheus.Data;
using RelatoriosProtheus.Models.Entities;
using RelatoriosProtheus.Models.ViewModels;

namespace RelatoriosProtheus.Controllers
{
    [Authorize]
    public class RelatoriosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotyfService _toast;

        public RelatoriosController(ApplicationDbContext context, INotyfService toast)
        {
            _context = context;
            _toast = toast;
        }



        // GET: Relatorios
        public async Task<IActionResult> Index(RelatorioFiltroViewModel filtroRelatorio)
        {
            var claim = string.Join(",", ((ClaimsIdentity)User.Identity).Claims.ToList());
            try
            {
                var usuarioEmail = User.Identity.Name;
                var funcionario = await _context.Funcionarios
                    .Include(f => f.FuncionarioGrupos)
                    .FirstOrDefaultAsync(f => f.Email == usuarioEmail);

                if (funcionario == null)
                {
                    return Unauthorized();
                }

                var gruposFuncionarioIds = funcionario.FuncionarioGrupos.Select(fg => fg.GrupoId).ToList();

                var grupos = await _context.Grupos.ToListAsync();

                var relatorios = _context.Relatorios
                    .Where(r => r.RelatorioGrupos.Any(rg => gruposFuncionarioIds.Contains(rg.GrupoId)))
                    .AsQueryable();                
                
                if( claim.Contains("Administrador"))
                    relatorios = _context.Relatorios
                        .AsQueryable();

                if (filtroRelatorio.GrupoSelecionadoId.HasValue)
                    relatorios = relatorios.Where(r =>
                        r.RelatorioGrupos.Any(rg => rg.GrupoId == filtroRelatorio.GrupoSelecionadoId.Value));

                if (!string.IsNullOrEmpty(filtroRelatorio.DescricaoRelatorio) && filtroRelatorio.DescricaoRelatorio.Length > 0)
                    relatorios = relatorios.Where(r => r.Descricao.Contains(filtroRelatorio.DescricaoRelatorio));

                var viewModel = new RelatorioFiltroViewModel
                {
                    GrupoSelecionadoId = filtroRelatorio.GrupoSelecionadoId,
                    DescricaoRelatorio = filtroRelatorio.DescricaoRelatorio,
                    GruposDisponiveis = grupos,
                    Relatorios = await relatorios
                        .Include(r => r.RelatorioGrupos)
                        .ThenInclude(rg => rg.GrupoFk)
                        .ToListAsync()
                };

                ViewBag.Grupos = grupos;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao carregar Index: " + ex.Message, ex);
            }
        }

        // GET: Relatorios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var relatorios = await _context.Relatorios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (relatorios == null)
            {
                return NotFound();
            }

            return View(relatorios);
        }

        // GET: Relatorios/Create
        public IActionResult Create()
        {
            var grupos = _context.Grupos.ToList();
            ViewBag.Grupos = new SelectList(grupos, "GrupoId", "Nome");

            return PartialView("_RelatorioCreateModal");
        }

        // POST: Relatorios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRelatoriosViewModel relatorioVm, List<int> SelectedGrupoIds)
        {
            if (!ModelState.IsValid)
            {
                var erros = ModelState
                        .Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                var listaHtml = "<ul>"
                    + string.Join("", erros.Select(msg => $"<li>{msg}</li>"))
                    + "</ul>";

                TempData["ErrorMessage"] = $@"
                <div class=""alert alert-danger"">
                    <strong>Não foi possível criar o relatório:</strong>
                    {listaHtml}
                </div>";
                return PartialView("_RelatorioCreateModal", relatorioVm);
            }


            var relEntity = new Relatorios
            {
                Nome = relatorioVm.Nome.Trim(),
                Descricao = relatorioVm.Descricao
            };

            bool existe = await _context.Relatorios
                .AnyAsync(r => r.Nome == relEntity.Nome && r.Descricao == relEntity.Descricao && r.RelatorioGrupos.Any(rg => rg.GrupoId == SelectedGrupoIds.First()));

            if (existe)
            {
                ModelState.AddModelError("", "O relatório já existe.");
                return View("Index", relatorioVm);
            }

            _context.Relatorios.Add(relEntity);
            await _context.SaveChangesAsync();

            foreach (var itm in relatorioVm.Items)
            {
                string itemCorrigido = Regex.Unescape(itm.Exemplo ?? "");
                itemCorrigido = itemCorrigido.Trim('"');

                _context.Parametros.Add(new Parametros
                {
                    RelatorioId = relEntity.Id,
                    Param = itm.Param,
                    DescricaoEmTela = itm.DescricaoEmTela,
                    Exemplo = itemCorrigido,
                    Tipo = itm.Tipo,
                    Obrigatorio = itm.Obrigatorio
                });
            }

            await _context.SaveChangesAsync();

            if (SelectedGrupoIds?.Any() == true)
            {
                foreach (var gid in SelectedGrupoIds)
                    _context.RelatorioGrupos.Add(new RelatorioGrupo
                    {
                        RelatoriosId = relEntity.Id,
                        GrupoId = gid
                    });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: Relatorios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var grupos = await _context.Grupos.ToListAsync();

            var rel = await _context.Relatorios
                    .Include(r => r.RelatorioGrupos)
                    .ThenInclude(rg => rg.GrupoFk)
                    .Include(p => p.Parametros)
                    .FirstOrDefaultAsync(r => r.Id == id);
            if (rel == null) return NotFound();

            int? grupoAtualId = rel.RelatorioGrupos.FirstOrDefault()?.GrupoFk.GrupoId;

            var vm = new RelatorioEditViewModel
            {
                Id = rel.Id,
                Nome = rel.Nome,
                Descricao = rel.Descricao,
                Parametros = rel.Parametros.ToList(),
                RelatorioGrupos = rel.RelatorioGrupos,
                SelectedGrupoId = grupoAtualId
            };

            ViewBag.GruposEdit = new SelectList(grupos, "GrupoId", "Nome", grupoAtualId);
            return View(vm);
        }

        // POST: Relatorios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,RelatorioEditViewModel relatorios,int? SelectedGrupoId)
        {
            if (id != relatorios.Id)
                return BadRequest();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RelatorioEditViewModel, Relatorios>()
                    .ForMember(dest => dest.RelatorioGrupos, opt => opt.Ignore())
                    .ForMember(dest => dest.Parametros, opt => opt.Ignore());
            });

            var relParaAtualizar = config.CreateMapper().Map<Relatorios>(relatorios);

            var existingRelatorio = await _context.Relatorios
                   .Include(r => r.RelatorioGrupos)
                   .Include(p => p.Parametros)
                   .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRelatorio == null)
                return NotFound();

            _context.Entry(existingRelatorio).CurrentValues.SetValues(relParaAtualizar);

            existingRelatorio.RelatorioGrupos.Clear();

            #region Lógica para salvar os grupos
            if (SelectedGrupoId.HasValue)
            {
                existingRelatorio.RelatorioGrupos.Add(new RelatorioGrupo
                {
                    RelatoriosId = existingRelatorio.Id,
                    GrupoId = SelectedGrupoId.Value
                });
            }
            #endregion

            #region Lógica para salvar os parametros
            var vmParamIds = relatorios.Parametros.Select(p => p.Id).ToList();
            var parametrosParaRemover = existingRelatorio.Parametros
                                         .Where(ent => !vmParamIds.Contains(ent.Id))
                                         .ToList();

            foreach (var pRem in parametrosParaRemover)
            {
                existingRelatorio.Parametros.Remove(pRem);
            }

            foreach (var pVm in relatorios.Parametros)
            {
                if (pVm.Id > 0)
                {
                    var pEntity = existingRelatorio.Parametros
                                       .FirstOrDefault(x => x.Id == pVm.Id);
                    if (pEntity != null)
                    {
                        pEntity.Param = pVm.Param;
                        pEntity.DescricaoEmTela = pVm.DescricaoEmTela;
                        pEntity.Exemplo = pVm.Exemplo;
                        pEntity.Tipo = pVm.Tipo;
                        pEntity.Obrigatorio = pVm.Obrigatorio;
                    }
                }
                else
                {
                    existingRelatorio.Parametros.Add(new Parametros
                    {
                        Param = pVm.Param,
                        DescricaoEmTela = pVm.DescricaoEmTela,
                        Exemplo = pVm.Exemplo,
                        Tipo = pVm.Tipo,
                        RelatorioId = existingRelatorio.Id,
                    });
                }
            } 
            #endregion

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RelatoriosExists(relatorios.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Relatorios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var relatorios = await _context.Relatorios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (relatorios == null)
            {
                return NotFound();
            }

            return View(relatorios);
        }

        // POST: Relatorios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var relacoes = _context.RelatorioGrupos
                    .Where(rg => rg.RelatoriosId == id);

            _context.RelatorioGrupos.RemoveRange(relacoes);

            await _context.SaveChangesAsync();

            var relatorio = await _context.Relatorios.FindAsync(id);
            if (relatorio != null)
            {
                _context.Relatorios.Remove(relatorio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Parametros(int id, string acao, int? grupoSelecionadoId, RelatorioFiltroViewModel relatorio)
        {
            var grupos = await _context.Grupos.ToListAsync();
            var rel = await _context.Relatorios
                                     .Include(r => r.RelatorioGrupos)
                                     .ThenInclude(rg => rg.GrupoFk)
                                     .Include(p => p.Parametros)
                                     .FirstOrDefaultAsync(r => r.Id == id);

            if (rel == null) return NotFound();

            var vm = new RelatorioComParametrosViewModel
            {
                RelatorioId = rel.Id,
                Nome = rel.Nome,
                Descricao = rel.Descricao,
                Acao = acao,
                TipoArquivo = relatorio.TipoArquivo,
                Parametros = rel.Parametros
                    .Select(p => new ParametrosViewModel
                    {
                        Id = p.Id,
                        Param = p.Param,
                        DescricaoEmTela = p.DescricaoEmTela,
                        Exemplo = p.Exemplo,
                        Tipo = p.Tipo,
                        Valor = "",
                        Obrigatorio = p.Obrigatorio
                    })
                    .ToList()
            };

            return PartialView("_RelatorioParametros", vm);
        }

        [HttpGet]
        public IActionResult RelatoriosGrupos()
        {
            var rel = _context.Relatorios
                .Include(r => r.RelatorioGrupos)
                .ThenInclude(rg => rg.GrupoFk)
                .ToList();

            var grupos = _context.Grupos.ToList();

            var viewModel = new RelatorioGruposViewModel()
            {
                Relatorios = rel,
                Grupos = grupos
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Executar(RelatorioComParametrosViewModel relatorio)
        {
            var relEntity = await _context.Relatorios
                .Include(r => r.RelatorioGrupos)
                .ThenInclude(rg => rg.GrupoFk)
                .FirstOrDefaultAsync(r => r.Id == relatorio.RelatorioId);

            if (relEntity == null)
                return View(relatorio);
            

            var areaId = relEntity.RelatorioGrupos.FirstOrDefault()?.GrupoFk?.Nome;
            if (string.IsNullOrEmpty(areaId))
            {
                _toast.Error("Grupo do relatório não encontrado.");
                return RedirectToAction("Index");
            }

            using var client = new HttpClient();
            var token = await LoginAuth(client);

            if (string.IsNullOrEmpty(token))
            {
                _toast.Error("Falha na autenticação. Verifique suas credenciais.");
                return RedirectToAction("Index");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new Dictionary<string, string>();

            foreach (var param in relatorio.Parametros)
            {
                if (param.Obrigatorio != 0 && string.IsNullOrWhiteSpace(param.Valor))
                {
                    _toast.Error("Falha nos parâmetros. Favor tentar novamente ou contatar o administradores.");
                    return RedirectToAction("Index");
                }

                var key = param.Param.Trim().Trim('"');

                if (param.Valor.IsNullOrEmpty())
                    param.Valor = " ";

                payload[key] = param.Valor;
            }

            var json = JsonConvert.SerializeObject(payload);
            using var body = new StringContent(json, Encoding.UTF8, "application/json");

            var grupo = relEntity.RelatorioGrupos.FirstOrDefault()?.GrupoFk.GrupoSeparacao == 1 ? "ProtheusBackoffice" : "ProtheusRH";

            var tipo = "pdf";

            switch (relatorio.TipoArquivo)
            {
                case 1:
                    tipo = "xls";
                    break;
                case 2:
                    tipo = "doc";
                    break;
            }

            //var tipo = relatorio.TipoArquivo != 1 ? "pdf" : "xls";

            var url = $"http://alecrim/ReportsProtheusAPI/api/Reports/{grupo}/?report={Uri.EscapeDataString(relEntity.Nome)}&area={Uri.EscapeDataString(areaId)}&arquivo={tipo}";
            
            var response = await client.PostAsync(url, body);

            if (!response.IsSuccessStatusCode)
            {
                _toast.Error("Não foi possível gerar o relatório. Verifique os parâmetros e tente novamente.");
                return RedirectToAction("Index");
            }

            var fileName = $"{relEntity.Nome}_{DateTime.Now:yyyyMMdd}.{tipo}";
            var pdfBytes = await response.Content.ReadAsByteArrayAsync();


            if (relatorio.Acao != "download")
            {
                var base64Pdf = Convert.ToBase64String(pdfBytes);
                return PartialView("_PdfIframe", base64Pdf);
            }

            Response.Cookies.Append("DownloadStatus", "success", new CookieOptions
            {
                Expires = DateTime.Now.AddSeconds(5),
                Path = "/"
            });

            return File(pdfBytes, "application/pdf", fileName);
        }

        #region Métodos Auxiliares
        private async Task<string> LoginAuth(HttpClient client)
        {
            var loginUrl = "http://alecrim/ReportsProtheusAPI/token";

            var tokenRequest = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", "reportsprotheusapi@fies.org.br"),
                    new KeyValuePair<string, string>("password", "S3n@i@123")
                });

            var loginResponse = await client.PostAsync(loginUrl, tokenRequest);

            var loginResult = await loginResponse.Content.ReadAsStringAsync();

            var tokenObj = JsonConvert.DeserializeObject<JObject>(loginResult);
            var token = tokenObj["access_token"]?.ToString();

            return token;
        }

        private bool RelatoriosExists(int id)
        {
            return _context.Relatorios.Any(e => e.Id == id);
        }
        #endregion
    }
}
