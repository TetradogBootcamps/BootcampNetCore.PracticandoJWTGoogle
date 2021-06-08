using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleLoginToken.GestionPermisos;

namespace GoogleLoginToken
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        }

        public IConfiguration Configuration { get; }
        public string MyAllowSpecificOrigins { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    builder.AllowAnyOrigin()
                    //.WithOrigins("http://localhost:44322"

                    //                    )
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                });
            });
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GoogleLoginToken", Version = "v1" });
            });
            services.AddDbContextPool<LoginContext>(o => o.UseMySql(
                     Configuration.GetConnectionString("Default"), new MariaDbServerVersion(new Version(10, 5, 10)))
                    );
            services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                   .AddCookie(options =>
                   {
                       options.LoginPath = "/account/google-login"; // Must be lowercase
                   })
                   .AddGoogle(options =>
                   {
                       options.ClientId = Configuration["Google:ClientId"];
                       options.ClientSecret = Configuration["Google:ClientSecret"];
                   });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience=true,
                    ValidAudience=Configuration["Jwt:Audience"],
                    ValidIssuer=Configuration["Jwt:Issuer"],
                    IssuerSigningKey=new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });
            services.AddHttpContextAccessor();
            //permisos
            // Add custom authorization handlers
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AdminRequirement.POLICITY, policy => policy.Requirements.Add(new AdminRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GoogleLoginToken v1"));
                app.UseCors(MyAllowSpecificOrigins);
            }

     
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
