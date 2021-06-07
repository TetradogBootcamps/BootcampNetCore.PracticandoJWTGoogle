using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleLoginToken.Controllers
{
    [AllowAnonymous, Route("api/[controller]"),ApiController]
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
        [Route("google-login")]
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
                if (!Context.Users.Any(user => user.Equals(userInfoAux)))
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

    }
}
