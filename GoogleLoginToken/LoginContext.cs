using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleLoginToken
{
    public class LoginContext : DbContext
    {
        public LoginContext(DbContextOptions<LoginContext> options) : base(options) { }

        public DbSet<UserInfo> Users { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<UserPermiso> PermisosUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserInfo>().HasMany(u => u.Permisos).WithOne(p => p.User);
            modelBuilder.Entity<Permiso>().HasMany(u => u.Usuarios).WithOne(p => p.Permiso);

            modelBuilder.Entity<UserPermiso>().HasKey(nameof(UserPermiso.UserId), nameof(UserPermiso.PermisoId));
            modelBuilder.Entity<UserPermiso>().HasOne(u => u.User).WithMany(u => u.Permisos);
            modelBuilder.Entity<UserPermiso>().HasOne(u => u.Permiso).WithMany(u => u.Usuarios);


        }

        public bool ExistUser(UserInfo userInfoAux)
        {
            string email = userInfoAux.Email.ToLower();
            return Users.Select(u => u.Email).Any(emailUser => emailUser.ToLower().Equals(email));
        }

        public UserInfo GetUserWithEmailOrDefault(string email)
        {
            return Users.Where(u => Equals(u.Email, email)).FirstOrDefault();
        }
        public UserInfo GetUserFromHttpContext(HttpContext httpContext)
        {
            return GetUserWithEmailOrDefault(UserInfo.GetEmailFromHttpContext(httpContext));
        }
        public UserPermiso GetRolOrDefault(UserInfo user, string rol)
        {
            IList<Permiso> permisos = Permisos.ToList();//si no lo pongo me falla p.Permiso...porque es null...
            return PermisosUsuarios.ToList().Where(p => p.IsActive && p.UserId == user.Id && p.Permiso.Name == rol).FirstOrDefault();
        }
        public bool IsAdmin(UserInfo user) => !Equals(GetRolOrDefault(user, Permiso.ADMIN), default(UserPermiso));

        public IList<Permiso> GetPermisos(UserInfo user)
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
        public bool CanValidate(UserInfo user) => GetPermisos(user).Select(p => p.Name).Intersect(Permiso.CanValidate).Count() > 0;
        IList<UserInfo> GetUsers(Permiso permiso)
        {
            int[] ids = PermisosUsuarios.ToList().Where(p => p.IsActive && p.PermisoId == permiso.Id).Select(p => p.UserId).ToArray();
            return Users.Where(p => ids.Contains(p.Id)).ToArray();
        }

        public bool CanSetPermiso(UserInfo user)
        {
           return GetPermisos(user).Select(p => p.Name).Intersect(Permiso.CanSetPermiso).Count() > 0;
        }
    }
}
