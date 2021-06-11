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
using Gabriel.Cat.S.Blazor;

namespace GoogleLoginToken.Controllers
{
    [AllowAnonymous, Route("[controller]"), ApiController]
    public class AccountController : Controller
    {
        IConfiguration Configuration { get; set; }
        LoginContext Context { get; set; }

        public AccountController(IConfiguration config, LoginContext context)
        {
            Configuration = config;
            Context = context;

        }
        [HttpGet]
        [Route("")]
        [Authorize]
        public IActionResult GetUser()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserWithEmailOrDefault(GoogleLoginToken.User.GetEmailFromHttpContext(HttpContext));
                result = Ok(new UserInfoDto(user, Context.GetPermisos(user)));//aqui leo el usuario admin
            }
            else result = Unauthorized();
            return result;
        }

        [HttpGet]
        [Route("login")]
        public IActionResult GoogleLogin()
        {
            AuthenticationProperties properties;
            Log.WriteLines("Se hace Login");

            properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GetToken)) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [Route("token")]
        public async Task<IActionResult> GetToken()
        {
            IActionResult result;
            User userInfoAux;
            JwtSecurityToken token;
            AuthenticateResult googleResult;

            Log.WriteLines("Se obtiene el Token");

            googleResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (googleResult.Succeeded)
            {
                userInfoAux = new User(googleResult.Principal);
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
                result = Unauthorized();
            }
            return result;


        }

        [Authorize]
        [HttpPost]
        [Route("permisos/{idUsuario:int}/{nombrePermiso}")]

        public async Task<IActionResult> SetPermiso(int idUsuario, string nombrePermiso)
        {
            User grantedBy;
            UserPermiso userPermiso;
            IActionResult result;
            Permiso permiso;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = await Context.FindAsync<User>(idUsuario);

                if (!Equals(user, default(User)))
                {
                    nombrePermiso = nombrePermiso.ToLower();
                    permiso = Context.Permisos.Where(p => Equals(p.Name.ToLower(), nombrePermiso)).FirstOrDefault();
                    if (!Equals(permiso, default(Permiso)))
                    {
                        if (Context.CanAddUsuario(permiso))
                        {
                            grantedBy = Context.GetUserFromHttpContext(HttpContext);//aqui leo el usuario admin
                            if (!Equals(grantedBy, default(User)) && permiso.OnlyAdminCanSet && Context.IsAdmin(grantedBy) || Context.CanSetPermiso(grantedBy))
                            {
                                userPermiso = Context.GetRolOrDefault(user, nombrePermiso);
                                if (Equals(userPermiso, default(UserPermiso)))
                                {
                                    userPermiso = new UserPermiso(user, permiso, grantedBy);
                                    Context.Add(userPermiso);

                                }
                                else
                                {
                                    userPermiso.SetGranted(grantedBy);
                                }
                                await Context.SaveChangesAsync();
                                result = Ok(new UserInfoDto(user, Context.GetPermisos(user)));
                            }
                            else
                            {
                                result = Forbid();
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
        [Authorize]
        public async Task<IActionResult> UnsetPermiso(int idUsuario, string nombrePermiso)
        {
            User revokedBy;
            UserPermiso userPermiso;
            IActionResult result;
            Permiso permiso;
            User user;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = await Context.FindAsync<User>(idUsuario);

                if (!Equals(user, default(User)))
                {
                    nombrePermiso = nombrePermiso.ToLower();
                    permiso = Context.Permisos.Where(p => Equals(p.Name, nombrePermiso)).FirstOrDefault();
                    if (!Equals(permiso, default(Permiso)))
                    {
                        if (Context.CanRemoveUsuario(permiso))
                        {
                            revokedBy = Context.GetUserFromHttpContext(HttpContext);//aqui leo el usuario admin
                            if (!Equals(revokedBy, default(User)) && permiso.OnlyAdminCanSet && Context.IsAdmin(revokedBy) || Context.CanSetPermiso(revokedBy))
                            {
                                userPermiso = Context.GetRolOrDefault(user, nombrePermiso);
                                if (!Equals(userPermiso, default(UserPermiso)) && userPermiso.IsActive)
                                {

                                    userPermiso.SetRevoked(revokedBy);
                                    await Context.SaveChangesAsync();
                                }
                                result = Ok(new UserInfoDto(user, Context.GetPermisos(user)));
                            }
                            else
                            {
                                result = Forbid();
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

        [HttpPost]
        [Route("validate/{idUsuario:int}/")]
        [Authorize]
        public async Task<IActionResult> ValidateUser(int idUsuario)
        {
            User validator;
            IActionResult result;
            User user;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = await Context.FindAsync<User>(idUsuario);

                if (!Equals(user, default(User)))
                {
                    //aqui leo el usuario admin
                    validator = Context.GetUserFromHttpContext(HttpContext);
                    if (!Equals(validator, default(User)) && Context.CanValidate(validator))
                    {
                        //if (!user.IsValidated)
                        //{
                        //    //user.IdValidador = validator.Id;
                        //    await Context.SaveChangesAsync();
                        //}

                        result = Ok(new UserInfoDto(user, Context.GetPermisos(user)));
                    }
                    else
                    {
                        result = Forbid();
                    }
                }
                else
                {
                    result = StatusCode(StatusCodes.Status416RangeNotSatisfiable);
                }

            }
            else result = Unauthorized();

            return result;
        }



        [HttpGet("testAdmin")]
        //[Authorize(Policy =AdminRequirement.POLICITY)]//no funciona...no pasa por la clase  AdminAuthorizationHandler
        public IActionResult TestAdmin()
        {
            IActionResult result;
            User user;

            user = Context.GetUserFromHttpContext(HttpContext);
            if (Equals(user, default(User)) || !Context.IsAdmin(user))
            {
                result = Forbid();
            }
            else result = Ok();

            return result;
        }

    }
}
