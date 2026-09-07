using System.Security.Claims;
using GestaoConsultasUVV.Data;
using GestaoConsultasUVV.Models;
using GestaoConsultasUVV.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoConsultasUVV.Controllers;

public class ContaController : Controller
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    // Ambas as dependencias chegam pelo container de DI configurado no Program.cs.
    public ContaController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // GET: /Conta/Registrar
    [HttpGet]
    public IActionResult Registrar() => View();

    // POST: /Conta/Registrar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistroViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();

        if (await _context.Usuarios.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Já existe uma conta cadastrada com este e-mail.");
            return View(model);
        }

        var usuario = new Usuario
        {
            Nome = model.Nome.Trim(),
            Email = email,
            DataCadastro = DateTime.Now
        };

        // A senha digitada vira hash e somente o hash e gravado no banco.
        usuario.SenhaHash = _passwordHasher.HashPassword(usuario, model.Senha);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        await AutenticarAsync(usuario, lembrarMe: false);

        TempData["Sucesso"] = "Cadastro realizado com sucesso. Bem-vindo(a)!";
        return RedirectToAction("Index", "Consultas");
    }

    // GET: /Conta/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: /Conta/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        // Mensagem generica de proposito: nao revela se o e-mail existe no sistema.
        if (usuario is null ||
            _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, model.Senha)
                == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(model);
        }

        await AutenticarAsync(usuario, model.LembrarMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Consultas");
    }

    // POST: /Conta/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // Cria o cookie de autenticacao com as claims do usuario.
    private async Task AutenticarAsync(Usuario usuario, bool lembrarMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = lembrarMe });
    }
}
