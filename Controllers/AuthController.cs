using AeC.Enderecos.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeC.Enderecos.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // GET /Auth/Login — exibe a tela de login 
    // Se já estiver autenticado, redireciona direto para os endereços

    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Enderecos");

        return View();
    }

    // POST /Auth/Login — valida credenciais e cria a sessão (cookie)

    [HttpPost]
    public async Task<IActionResult> Login(string nomeUsuario, string senha)
    {
        // Busca o usuário pelo nome de usuário (não pela senha — a senha é verificada com BCrypt)
        var usuario = _context.Usuarios
            .FirstOrDefault(u => u.NomeUsuario == nomeUsuario);

        // BCrypt.Verify compara a senha em texto plano com o hash armazenado no banco
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash))
        {
            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View();
        }

        // Cria os "claims" do usuário — informações que ficam no cookie de autenticação
        // Esses claims são usados no _Layout para mostrar o nome, e no controller para
        // filtrar os endereços pelo ID do usuário logado
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), // usado para filtrar endereços
            new Claim(ClaimTypes.Name, usuario.Nome),                    // usado para exibir na sidebar
            new Claim("NomeUsuario", usuario.NomeUsuario)
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal  = new ClaimsPrincipal(identidade);

        // Assina o cookie de autenticação — a partir daqui o usuário está "logado"
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // Redireciona direto para a lista de endereços após login bem-sucedido
        return RedirectToAction("Index", "Enderecos");
    }

    // GET /Auth/Logout — encerra a sessão e volta para o login

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
