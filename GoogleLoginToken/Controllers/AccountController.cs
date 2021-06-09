using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using GoogleLoginToken.GestionPermisos;
using Microsoft.AspNetCore.Http;

namespace GoogleLoginToken.Controllers
{
    [AllowAnonymous, Route("[controller]"),ApiController]
    public class AccountController : Controller
    {
        IConfiguration Configuration { get; set; }
        LoginContext Context { get; set; }

        public AccountController(IConfiguration config,LoginContext context)
        {
            Configuration = config;
            Context = context;
           
        }

        [HttpGet]
        [Route("login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GetToken)) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [Route("token")]
        public async Task<IActionResult> GetToken()
        {
            IActionResult result;
            UserInfo userInfoAux;
            JwtSecurityToken token;
            AuthenticateResult googleResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            if (!Equals(googleResult.Principal, default))
            {
                userInfoAux = new UserInfo(googleResult.Principal);
                if (!Context.ExistUser(userInfoAux))
                { 
                    //no existe pues lo añado
                    try
                    {
                        Context.Users.Add(userInfoAux);
                        await Context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw;//de momento lo dejo así
                    }
                }

                token = userInfoAux.GetToken(Configuration);
                //tengo que guardar el token no?
                //envio al usuario el token
                result = Ok(token.WriteToken());
            }
            else
            {
                result = BadRequest();
            }
            return result;

         
        }


        [HttpPost]
        [Route("permisos/{idUsuario:int}/{nombrePermiso}")]
        [Authorize(Policy = AdminRequirement.POLICITY)]
        public async Task<IActionResult> SetPermiso(int idUsuario,string nombrePermiso)
        {
            UserInfo grantedBy;
            UserPermiso userPermiso;
            IActionResult result;
            Permiso permiso;
            UserInfo user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = await Context.FindAsync<UserInfo>(idUsuario);

                if (!Equals(user, default(UserInfo)))
                {
                    nombrePermiso = nombrePermiso.ToLower();
                    permiso = Context.Permisos.Where(p => Equals(p.Name, nombrePermiso)).FirstOrDefault();
                    if (!Equals(permiso, default(Permiso)))
                    {
                        if (permiso.CanAdd)
                        {
                            grantedBy = Context.GetUserWithEmailOrDefault(new UserInfo(HttpContext.User).Email);//aqui leo el usuario admin
                            if (!Equals(grantedBy, default(UserInfo)) && grantedBy.IsAdmin)
                            {
                                userPermiso = user.Permisos.Where(p => Equals(p.Permiso.Name, nombrePermiso)).FirstOrDefault();
                                if (Equals(user, default(UserPermiso)))
                                {
                                    userPermiso = new UserPermiso(user, permiso, grantedBy);
                                    Context.Add(userPermiso);
                                    user.Permisos.Add(userPermiso);

                                }
                                else
                                {
                                    userPermiso.SetGranted(grantedBy);
                                }
                                await Context.SaveChangesAsync();
                                result = Ok(user);
                            }
                            else
                            {
                                result = StatusCode(StatusCodes.Status403Forbidden);
                            }
                        }
                        else
                        {
                            result = StatusCode(StatusCodes.Status416RangeNotSatisfiable);
                        }
                    }
                    else
                    {
                        result = NotFound();
                    }
                }
                else
                {
                    result = NotFound();
                }
            }
            else result = Unauthorized();
            return result;
        }


        [HttpDelete]
        [Route("permisos/{idUsuario:int}/{nombrePermiso}")]
        [Authorize(Policy = AdminRequirement.POLICITY)]
        public async Task<IActionResult> UnsetPermiso(int idUsuario, string nombrePermiso)
        {
            UserInfo revokedBy;
            UserPermiso userPermiso;
            IActionResult result;
            Permiso permiso;
            UserInfo user;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = await Context.FindAsync<UserInfo>(idUsuario);

                if (!Equals(user, default(UserInfo)))
                {
                    nombrePermiso = nombrePermiso.ToLower();
                    permiso = Context.Permisos.Where(p => Equals(p.Name, nombrePermiso)).FirstOrDefault();
                    if (!Equals(permiso, default(Permiso)))
                    {
                        if (permiso.CanRemove)
                        {
                            revokedBy = Context.GetUserWithEmailOrDefault(new UserInfo(HttpContext.User).Email);//aqui leo el usuario admin
                            if (!Equals(revokedBy, default(UserInfo)))
                            {
                                userPermiso = user.Permisos.Where(p => Equals(p.Permiso.Name, nombrePermiso)).FirstOrDefault();
                                if (!Equals(userPermiso, default(UserPermiso)) && userPermiso.IsActive)
                                {

                                    userPermiso.SetRevoked(revokedBy);
                                    await Context.SaveChangesAsync();
                                }
                                result = Ok(user);
                            }
                            else
                            {
                                result = StatusCode(StatusCodes.Status403Forbidden);
                            }
                        }
                        else
                        {
                            result = StatusCode(StatusCodes.Status416RangeNotSatisfiable);
                        }
                    }
                    else
                    {
                        result = NotFound();
                    }
                }
                else
                {
                    result = NotFound();
                }
            }
            else result = Unauthorized();

            return result;
        }

    }
}
