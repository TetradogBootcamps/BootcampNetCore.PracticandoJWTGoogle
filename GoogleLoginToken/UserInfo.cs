using Microsoft.Azure.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GoogleLoginToken
{
    [Index(nameof(Email), IsUnique = true, Name = nameof(Email) + "_uniqueContraint")]
    public class UserInfo
    {
        public static DateTime DefaultExpireTokenDate { get; set; } = DateTime.UtcNow.AddDays(1);
        public UserInfo() { Permisos = new List<UserPermiso>(); }
        public UserInfo([NotNull] ClaimsPrincipal principal):this() {
        
           Claim[] claims=principal.Identities.FirstOrDefault().Claims.ToArray();
            IdExterno = claims[0].Value;
            Email = claims[^1].Value;
            FirstName = claims[2].Value;
            LastName = claims[3].Value;
        }


        public int Id { get; set; }
        [Required]
        public string IdExterno { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        public int? IdValidador { get; set; }

        public IList<UserPermiso> Permisos { get; set; }

        public bool IsValidated => IdValidador.HasValue;
        public bool IsAdmin => Permisos.Any(p => Equals(p.Permiso.Name, Permiso.ADMIN));

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
                new Claim(nameof(IsValidated),IsValidated.ToString()),
                new Claim(nameof(Permisos),System.Text.Json.JsonSerializer.Serialize(Permisos.Select(p=>new UserPermisoDto(p))))
            };
            return new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"],
                                        claims, expires: Equals(expiraToken, default(DateTime)) ? DefaultExpireTokenDate : expiraToken,
                                        signingCredentials: signIn);
        }

    }

    //de momento no se si lo usaré
    public class UserInfoDto
    {
        public UserInfoDto() { }
        public UserInfoDto(UserInfo user)
        {
            Name = user.FirstName;
            Email = user.Email;
            IsValidated = user.IsValidated;
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsValidated { get; set; }

    }
}
