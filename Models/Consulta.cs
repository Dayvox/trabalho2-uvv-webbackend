using System.ComponentModel.DataAnnotations;

namespace GestaoConsultasUVV.Models;

public class Consulta
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A especialidade é obrigatória.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "A especialidade deve ter entre 3 e 80 caracteres.")]
    [Display(Name = "Especialidade")]
    public string Especialidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data e hora são obrigatórias.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Data e hora")]
    public DateTime DataHora { get; set; }

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    // Lado "muitos": chave estrangeira para o dono da consulta.
    [Display(Name = "Usuário")]
    public int UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }
}
