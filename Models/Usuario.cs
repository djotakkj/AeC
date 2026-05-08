using System.ComponentModel.DataAnnotations;

namespace AeC.Enderecos.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string NomeUsuario { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    public List<Endereco> Enderecos { get; set; } = new();
}