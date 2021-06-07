using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GoogleLoginToken
{
    public class UserPermiso
    {

        public int IdPermiso { get; set; }
        
        public Permiso Permiso { get; set; }

        public int IdUser { get; set; }

        public UserInfo User { get; set; }

        [Column("GrantedById")]
        public int IdGrantedBy { get; set; }
        [ForeignKey(nameof(IdGrantedBy))]
        public UserInfo GrantedBy { get; set; }
    }
    public class UserPermisoDto
    {
        public UserPermisoDto(UserPermiso permiso) => Name = permiso.Permiso.Name;
        public string Name { get; set; }
    }
}