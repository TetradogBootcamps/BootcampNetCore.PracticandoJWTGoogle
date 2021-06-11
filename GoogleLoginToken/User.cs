using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GoogleLoginToken
{
    [Index(nameof(Email), IsUnique = true, Name = nameof(Email) + "_uniqueContraint")]
    public class User
    {
        public static DateTime DefaultExpireTokenDate { get; set; } = DateTime.UtcNow.AddDays(1);
        public User() { }
        public User([NotNull] ClaimsPrincipal principal):this() {
        
           Claim[] claims=principal.Identities.FirstOrDefault().Claims.ToArray();

            IdExterno = claims[0].Value;
            Email = claims[^1].Value;
            FirstName = claims[2].Value;
            LastName = claims[3].Value;
        }


        public int Id { get; set; }

        [ForeignKey(nameof(GoogleLoginToken.UserDetails.User))]
        public UserDetails UserDetails { get; set; }

        [Required]
        public string IdExterno { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        public int? ValidadorId { get; set; }
        public User Validador { get; set; }
        public IList<User> ValidadorList { get; set; }
        public bool IsValidated => ValidadorId.HasValue;
        //[JsonIgnore]
        //public ICollection<UserPermiso> PermisoList { get; set; }
        //[JsonIgnore]
        //public ICollection<UserPermiso> GrantedList { get; set; }
        //[JsonIgnore]
        //public ICollection<UserPermiso> RevokedList { get; set; }

        //public string[] PermisosName => PermisoList.Where(p => !Equals(p.Permiso, default(Permiso))).Select(p => p.Permiso.Name).ToArray();


        //public bool IsAdmin => PermisoList.Where(p => p.Permiso != default(Permiso)).Any(p => Equals(p.Permiso.Name, Permiso.ADMIN));
        //public bool CanValidate=>IsAdmin || PermisosName.Intersect(Permiso.CanValidate).Count() > 0;
        //public bool CanSetPermiso=>IsAdmin || PermisosName.Intersect(Permiso.CanSetPermiso).Count() > 0;
        public JwtSecurityToken GetToken(IConfiguration configuration,DateTime expiraToken=default(DateTime))
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            SigningCredentials signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            Claim[] claims=new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()),
                new Claim(nameof(FirstName),FirstName),
                new Claim(nameof(LastName),LastName),
                new Claim(nameof(Email),Email),
                //new Claim(nameof(IsValidated),IsValidated.ToString()),//lo quito porque quizás se tenga que hacer diferente
                //new Claim(nameof(PermisoList),System.Text.Json.JsonSerializer.Serialize(PermisosName))
            };
            return new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"],
                                        claims, expires: Equals(expiraToken, default(DateTime)) ? DefaultExpireTokenDate : expiraToken,
                                        signingCredentials: signIn);
        }
        public override string ToString()
        {
            return Email;
        }
        public static string GetEmailFromHttpContext(HttpContext context)
        {
            const int EMAIL = 5;//si cambio el orden de los claim en nameof(GetToken) tengo que mirar donde queda el Email de nuevo!!
            Claim[] claims = context.User.Identities.FirstOrDefault().Claims.ToArray();

            return claims[EMAIL].Value;
        }

    }


    public class UserInfoDto
    {
        public UserInfoDto() { }
        public UserInfoDto(User user,IList<Permiso> permisos)
        {
            Name = user.FirstName;
            Email = user.Email;
            IsValidated = user.IsValidated;
            Permisos = permisos.Select(p=>p.Name).ToArray();
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public string[] Permisos { get; set; }
        public bool IsValidated { get; set; }

    }
}
