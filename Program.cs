using AeC.Enderecos.Data;
using AeC.Enderecos.Models;
using AeC.Enderecos.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// REGISTRO DE SERVIÇOS (Injeção de Dependência)
// Banco de dados via Entity Framework Core + SQL Server
// A connection string está em appsettings.json → "DefaultConnection"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpClient tipado para o ViaCepService
// Isso cria um HttpClient gerenciado (evita problemas de socket exhaustion)
builder.Services.AddHttpClient<ViaCepService>();

// Autenticação por cookie
// LoginPath define para onde redirecionar quando não autenticado
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath      = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // sessão expira em 8 horas
    });

builder.Services.AddControllersWithViews();

// BUILD DA APLICAÇÃO

var app = builder.Build();

// SEED: cria um usuário padrão se o banco estiver vazio
// Útil para o primeiro uso após executar as migrations

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Usuarios.Any())
    {
        context.Usuarios.Add(new Usuario
        {
            Nome        = "Administrador",
            NomeUsuario = "admin",
            // BCrypt.HashPassword gera um hash seguro da senha
            // Nunca armazene senhas em texto plano!
            SenhaHash   = BCrypt.Net.BCrypt.HashPassword("123456")
        });

        context.SaveChanges();
    }
}

// PIPELINE DE MIDDLEWARES (ordem importa!)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// UseAuthentication deve vir antes de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Rota padrão: abre direto no Login
// Se já estiver autenticado, o AuthController redireciona para Enderecos/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
