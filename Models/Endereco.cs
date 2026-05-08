using System.ComponentModel.DataAnnotations;

namespace AeC.Enderecos.Models;

public class Endereco
{
    public int Id { get; set; }

    [Required]
    [StringLength(9)]
    public string Cep { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Logradouro { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Complemento { get; set; }

    [Required]
    [StringLength(100)]
    public string Bairro { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    [StringLength(2)]
    public string Uf { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    public int UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }
}