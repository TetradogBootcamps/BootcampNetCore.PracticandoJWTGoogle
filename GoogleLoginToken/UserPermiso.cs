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

            PermisoId = permiso.Id;
            UserId = user.Id;

            SetGranted(grantedBy);
        }

        public int PermisoId { get; set; }

        public Permiso Permiso { get; set; }

        public int UserId { get; set; }

        public UserInfo User { get; set; }


        public int GrantedById { get; set; }

        public UserInfo GrantedBy { get; set; }
        public DateTime FechaGranted { get; set; }


        public int? RevokedById { get; set; }

        public UserInfo RevokedBy { get; set; }
        public DateTime? FechaRevoked { get; set; }

        public bool IsActive =>  FechaGranted >= FechaRevoked.GetValueOrDefault();//si son iguales es que es default(DateTime)

        public void SetRevoked(UserInfo revokedBy)
        {
            if (IsActive)
            {
                RevokedBy = revokedBy;
                RevokedById = revokedBy.Id;
                FechaRevoked = DateTime.UtcNow;
            }
        }
        public void SetGranted(UserInfo grantedBy)
        {
        
                if (!IsActive)
                {
                    GrantedBy = grantedBy;
                    GrantedById = grantedBy.Id;
                    FechaGranted = DateTime.UtcNow;
                }
        }
        public override string ToString()
        {
            return $"{IsActive} IdUser {UserId} IdPermiso {PermisoId}";
        }
    }
    public class UserPermisoDto
    {
        public UserPermisoDto() { }
        public UserPermisoDto(UserPermiso permiso) => Name = permiso.Permiso.Name;
        public string Name { get; set; }
    }
}