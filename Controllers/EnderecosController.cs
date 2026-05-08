using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AeC.Enderecos.Data;
using AeC.Enderecos.Models;
using AeC.Enderecos.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AeC.Enderecos.Controllers
{
    // [Authorize] garante que todas as actions exigem login
    [Authorize]
    public class EnderecosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ViaCepService _viaCepService;

        // Injeção de dependência: o ASP.NET cuida de criar AppDbContext e ViaCepService
        // Não sei se é necessário injetar o ViaCepService aqui, já que ele é usado apenas no endpoint BuscarCep.
        // Mas deixei para exemplificar a injeção de serviços personalizados

        public EnderecosController(AppDbContext context, ViaCepService viaCepService)
        {
            _context = context;
            _viaCepService = viaCepService;
        }

        // HELPER: pega o ID do usuário logado a partir do cookie de autenticação
        
        private int GetUsuarioId()
            => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // INDEX — lista todos os endereços do usuário logado

        public async Task<IActionResult> Index()
        {
            var enderecos = await _context.Enderecos
                .Where(e => e.UsuarioId == GetUsuarioId())
                .OrderBy(e => e.Cidade)
                .ThenBy(e => e.Logradouro)
                .ToListAsync();

            return View(enderecos);
        }

        // DETAILS — exibe detalhes de um endereço específico

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Filtra por ID e por UsuarioId para evitar que um usuário
            // acesse endereços de outro usuário

            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == GetUsuarioId());

            if (endereco == null) return NotFound();

            return View(endereco);
        }

        // CREATE GET — exibe o formulário em branco
 
        public IActionResult Create() => View();

        // CREATE POST — salva o novo endereço no banco
        
        [HttpPost]
        [ValidateAntiForgeryToken] // proteção contra CSRF
        public async Task<IActionResult> Create(
            [Bind("Cep,Logradouro,Complemento,Bairro,Cidade,Uf,Numero")] Endereco endereco)
        {
            // Associa o endereço ao usuário logado antes de validar/salvar

            endereco.UsuarioId = GetUsuarioId();

            if (ModelState.IsValid)
            {
                _context.Add(endereco);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(endereco);
        }

        // EDIT GET — carrega o endereço existente no formulário

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == GetUsuarioId());

            if (endereco == null) return NotFound();

            return View(endereco);
        }

        // EDIT POST — aplica as alterações no banco

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Cep,Logradouro,Complemento,Bairro,Cidade,Uf,Numero")] Endereco endereco)
        {
            if (id != endereco.Id) return NotFound();

            endereco.UsuarioId = GetUsuarioId();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(endereco);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Se o registro foi excluído por outra sessão enquanto editávamos
                    if (!EnderecoExiste(endereco.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(endereco);
        }

        // DELETE GET — exibe a tela de confirmação de exclusão

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == GetUsuarioId());

            if (endereco == null) return NotFound();

            return View(endereco);
        }

        // DELETE POST — executa a exclusão após confirmação do usuário

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var endereco = await _context.Enderecos
                .FirstOrDefaultAsync(e => e.Id == id && e.UsuarioId == GetUsuarioId());

            if (endereco != null)
            {
                _context.Enderecos.Remove(endereco);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // BUSCAR CEP — endpoint chamado via JavaScript (AJAX) na tela de cadastro
        // Retorna JSON com os dados do endereço ou 404 se não encontrado

        [HttpGet]
        public async Task<IActionResult> BuscarCep(string cep)
        {
            var resultado = await _viaCepService.BuscarEnderecoPorCepAsync(cep);

            if (resultado == null)
                return NotFound(new { mensagem = "CEP não encontrado." });

            return Json(resultado);
        }

        // EXPORTAR CSV — gera e faz download de um arquivo CSV com os
        // endereços do usuário logado

        public async Task<IActionResult> ExportarCsv()
        {
            var enderecos = await _context.Enderecos
                .Where(e => e.UsuarioId == GetUsuarioId())
                .OrderBy(e => e.Cidade)
                .ThenBy(e => e.Logradouro)
                .ToListAsync();

            // Constrói o conteúdo CSV linha a linha
            var csv = new StringBuilder();

            // Cabeçalho das colunas
            csv.AppendLine("CEP;Logradouro;Complemento;Bairro;Cidade;UF;Numero");

            // Uma linha por endereço
            // EscapeCsv protege campos que possam conter ponto-e-vírgula ou aspas

            foreach (var e in enderecos)
            {
                csv.AppendLine(string.Join(";",
                    EscapeCsv(e.Cep),
                    EscapeCsv(e.Logradouro),
                    EscapeCsv(e.Complemento ?? ""),
                    EscapeCsv(e.Bairro),
                    EscapeCsv(e.Cidade),
                    EscapeCsv(e.Uf),
                    EscapeCsv(e.Numero)
                ));
            }

            // Retorna o arquivo para download com o nome baseado na data atual
            var nomeArquivo = $"enderecos_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());

            // "attachment" força o download; "text/csv" define o tipo MIME correto
            return File(bytes, "text/csv; charset=utf-8", nomeArquivo);
        }

        // HELPERS PRIVADOS

        // Verifica se um endereço existe (usado no tratamento de concorrência no Edit)
        private bool EnderecoExiste(int id)
            => _context.Enderecos.Any(e => e.Id == id && e.UsuarioId == GetUsuarioId());

        // Escapa um campo para CSV: se contiver ; ou " envolve em aspas
        private static string EscapeCsv(string valor)
        {
            if (valor.Contains(';') || valor.Contains('"') || valor.Contains('\n'))
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            return valor;
        }
    }
}
