using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace GoogleLoginToken
{
    public class UserPermiso
    {
        public UserPermiso() { }
        public UserPermiso([NotNull] UserInfo user, [NotNull] Permiso permiso, [NotNull] UserInfo grantedBy)
        {
            User = user;
            Permiso = permiso;
            IdPermiso = permiso.Id;
            IdUser = user.Id;
            SetGranted(grantedBy);
        }

        public int IdPermiso { get; set; }

        public Permiso Permiso { get; set; }

        public int IdUser { get; set; }

        public UserInfo User { get; set; }

        [Column("GrantedById")]
        public int? IdGrantedBy { get; set; }
        [ForeignKey(nameof(IdGrantedBy))]
        public UserInfo GrantedBy { get; set; }
        public DateTime? FechaGranted { get; set; }

        [Column("RevokedById")]
        public int? IdRevokedBy { get; set; }
        [ForeignKey(nameof(IdRevokedBy))]
        public UserInfo RevokedBy { get; set; }
        public DateTime? FechaRevoked { get; set; }

        public bool IsActive =>  FechaGranted.GetValueOrDefault() >= FechaRevoked.GetValueOrDefault();//si son iguales es que es default(DateTime)

        public void SetRevoked(UserInfo revokedBy)
        {
            if (IsActive)
            {
                RevokedBy = revokedBy;
                IdRevokedBy = revokedBy.Id;
                FechaRevoked = DateTime.UtcNow;
            }
        }
        public void SetGranted(UserInfo grantedBy)
        {
        
                if (!IsActive)
                {
                    GrantedBy = grantedBy;
                    IdGrantedBy = grantedBy.Id;
                    FechaGranted = DateTime.UtcNow;
                }
        }
        public override string ToString()
        {
            return $"{IsActive} IdUser {IdUser} IdPermiso {IdPermiso}";
        }
    }
    public class UserPermisoDto
    {
        public UserPermisoDto() { }
        public UserPermisoDto(UserPermiso permiso) => Name = permiso.Permiso.Name;
        public string Name { get; set; }
    }
}