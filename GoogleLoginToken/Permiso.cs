using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GoogleLoginToken
{
    public class Permiso
    {
        public const string ADMIN = "Admin";

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public bool OnlyAdminCanSet { get; set; }

        public int Minimum { get; set; }

        public int Maximum { get; set; }
        public IList<UserPermiso> Usuarios { get; set; }
        public bool CanAdd =>Equals(Usuarios,default(IList<UserPermiso>)) || Usuarios.Count < Maximum;
        public bool CanRemove=> Equals(Usuarios, default(IList<UserPermiso>)) || Usuarios.Count > Minimum;
    }
}