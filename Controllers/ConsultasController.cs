using System.Security.Claims;
using GestaoConsultasUVV.Data;
using GestaoConsultasUVV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoConsultasUVV.Controllers;

// Protege TODAS as acoes: quem nao estiver logado e redirecionado para /Conta/Login.
[Authorize]
public class ConsultasController : Controller
{
    private readonly AppDbContext _context;

    public ConsultasController(AppDbContext context)
    {
        _context = context;
    }

    // Id do dono da sessao atual, lido da claim gravada no login.
    private int UsuarioLogadoId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET: /Consultas
    public async Task<IActionResult> Index()
    {
        var consultas = await _context.Consultas
            .Where(c => c.UsuarioId == UsuarioLogadoId)
            .OrderBy(c => c.DataHora)
            .AsNoTracking()
            .ToListAsync();

        return View(consultas);
    }

    // GET: /Consultas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var consulta = await _context.Consultas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioLogadoId);

        if (consulta is null) return NotFound();

        return View(consulta);
    }

    // GET: /Consultas/Create
    public IActionResult Create()
    {
        return View(new Consulta { DataHora = DateTime.Now.AddDays(1) });
    }

    // POST: /Consultas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Especialidade,DataHora,Descricao")] Consulta consulta)
    {
        if (!ModelState.IsValid)
        {
            return View(consulta);
        }

        // O dono e definido no servidor, nunca vem do formulario.
        consulta.UsuarioId = UsuarioLogadoId;

        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Consulta registrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Consultas/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var consulta = await _context.Consultas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioLogadoId);

        if (consulta is null) return NotFound();

        return View(consulta);
    }

    // POST: /Consultas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Especialidade,DataHora,Descricao")] Consulta consulta)
    {
        if (id != consulta.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(consulta);
        }

        // Recarrega do banco ja filtrando pelo dono: impede editar consulta de outro usuario.
        var existente = await _context.Consultas
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioLogadoId);

        if (existente is null) return NotFound();

        existente.Especialidade = consulta.Especialidade;
        existente.DataHora = consulta.DataHora;
        existente.Descricao = consulta.Descricao;

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Consulta atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Consultas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var consulta = await _context.Consultas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioLogadoId);

        if (consulta is null) return NotFound();

        return View(consulta);
    }

    // POST: /Consultas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var consulta = await _context.Consultas
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioLogadoId);

        if (consulta is null) return NotFound();

        _context.Consultas.Remove(consulta);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Consulta excluída com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
