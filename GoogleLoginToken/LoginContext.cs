﻿using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Utilitats;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;


namespace GoogleLoginToken
{
    public class LoginContext : DbContextAuto
    {
        public LoginContext(DbContextOptions<LoginContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<UserPermiso> PermisosUsuarios { get; set; }



        public bool ExistUser(User userInfoAux)
        {
            string email = userInfoAux.Email.ToLower();
            return Users.Select(u => u.Email).Any(emailUser => emailUser.ToLower().Equals(email));
        }

        public User GetUserWithEmailOrDefault(string email)
        {
            return Users.Where(u => Equals(u.Email, email)).FirstOrDefault();
        }
        public User GetUserFromHttpContext(HttpContext httpContext)
        {
            return GetUserWithEmailOrDefault(User.GetEmailFromHttpContext(httpContext));
        }
        public UserPermiso GetRolOrDefault(User user, string rol)
        {
            IList<Permiso> permisos = Permisos.ToList();//si no lo pongo me falla p.Permiso...porque es null...
            return PermisosUsuarios.ToList().Where(p => p.IsActive && p.UserId == user.Id && p.Permiso.Name == rol).FirstOrDefault();
        }
        public bool IsAdmin(User user) => !Equals(GetRolOrDefault(user, Permiso.ADMIN), default(UserPermiso));

        public IList<Permiso> GetPermisos(User user)
        {
            int[] ids = PermisosUsuarios.ToList().Where(p => p.IsActive && p.UserId == user.Id ).Select(p => p.PermisoId).ToArray();
            return Permisos.Where(p => ids.Contains(p.Id)).ToArray();
        }

        public bool CanAddUsuario(Permiso permiso)
        {
            return  GetUsers(permiso).Count< permiso.Maximum;
        }

        public bool CanRemoveUsuario(Permiso permiso)
        {
            return  GetUsers(permiso).Count > permiso.Minimum;
        }
        public bool CanValidate(User user) => GetPermisos(user).Select(p => p.Name).Intersect(Permiso.CanValidate).Count() > 0;
        IList<User> GetUsers(Permiso permiso)
        {
            int[] ids = PermisosUsuarios.ToList().Where(p => p.IsActive && p.PermisoId == permiso.Id).Select(p => p.UserId).ToArray();
            return Users.Where(p => ids.Contains(p.Id)).ToArray();
        }

        public bool CanSetPermiso(User user)
        {
           return GetPermisos(user).Select(p => p.Name).Intersect(Permiso.CanSetPermiso).Count() > 0;
        }
    }

    public abstract class DbContextAuto:DbContext
    {
        public DbContextAuto(DbContextOptions options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.ConfigureEntities(modelBuilder);
        }
    }
}
