using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioApi.Models;

[Table("usuarios")]
public class Usuario
{
    [Column("id")] public int Id { get; set; }
    [Column("nombre_usuario")] public string NombreUsuario { get; set; } = string.Empty;
    [Column("password_hash")] public string PasswordHash { get; set; } = string.Empty;
    [Column("rol")] public string Rol { get; set; } = "usuario";
    [Column("creado_en")] public DateTime CreadoEn { get; set; }
}
