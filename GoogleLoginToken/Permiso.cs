using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GoogleLoginToken
{
    public class Permiso
    {
        public const string ADMIN = "admin";
        public const string MOD = "mod";
        public static readonly string[] CanValidate = { ADMIN, MOD };
        /// <summary>
        /// Aqui solo estan los que aparte del Admin pueden tocar los permisos que nameof(OnlyAdminCanSet)=false
        /// </summary>
        public static readonly string[] CanSetPermiso = { MOD };
        public Permiso() => Usuarios = new List<UserPermiso>();
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public bool OnlyAdminCanSet { get; set; }

        public int Minimum { get; set; }

        public int Maximum { get; set; }
        public IList<UserPermiso> Usuarios { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}