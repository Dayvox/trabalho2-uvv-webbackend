using System.ComponentModel.DataAnnotations;

namespace GestaoConsultasUVV.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    // Guarda SOMENTE o hash da senha, nunca o texto puro.
    [Required]
    [StringLength(500)]
    public string SenhaHash { get; set; } = string.Empty;

    [Display(Name = "Data de cadastro")]
    [DataType(DataType.DateTime)]
    public DateTime DataCadastro { get; set; } = DateTime.Now;

    // Lado "um" do relacionamento 1:N com Consulta.
    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
}
