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
        public UserPermiso([NotNull] User user, [NotNull] Permiso permiso, [NotNull] User grantedBy)
        {
            User = user;
            Permiso = permiso;

            PermisoId = permiso.Id;
            UserId = user.Id;

            SetGranted(grantedBy);
        }
        [Key]
        public int PermisoId { get; set; }

        public Permiso Permiso { get; set; }
        [Key]
        public int UserId { get; set; }

        public User User { get; set; }


        public int GrantedId { get; set; }

        public User Granted { get; set; }
        public DateTime FechaGranted { get; set; }


        public int? RevokedId { get; set; }

        public User Revoked { get; set; }
        public DateTime? FechaRevoked { get; set; }

        public bool IsActive =>  FechaGranted >= FechaRevoked.GetValueOrDefault();//si son iguales es que es default(DateTime)

        public void SetRevoked(User revokedBy)
        {
            if (IsActive)
            {
                Revoked = revokedBy;
                RevokedId = revokedBy.Id;
                FechaRevoked = DateTime.UtcNow;
            }
        }
        public void SetGranted(User grantedBy)
        {
        
                if (!IsActive)
                {
                    Granted = grantedBy;
                    GrantedId = grantedBy.Id;
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