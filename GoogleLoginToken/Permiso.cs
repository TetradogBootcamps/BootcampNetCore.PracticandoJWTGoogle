using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GoogleLoginToken
{
    public class Permiso
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public bool OnlyAdminCanSet { get; set; }

        public int Minimum { get; set; }

        public int Maximum { get; set; }
        public IList<UserPermiso> Usuarios { get; set; }
    }
}