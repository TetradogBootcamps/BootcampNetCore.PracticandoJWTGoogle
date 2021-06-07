using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleLoginToken
{
    public class LoginContext:DbContext
    {
        public LoginContext(DbContextOptions<LoginContext> options) : base(options) { }

        public DbSet<UserInfo> Users { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<UserPermiso> PermisosUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserPermiso>().HasKey(nameof(UserPermiso.IdUser), nameof(UserPermiso.IdPermiso));
            modelBuilder.Entity<UserInfo>().HasMany(u => u.Permisos).WithOne(p => p.User);
            modelBuilder.Entity<Permiso>().HasMany(u => u.Usuarios).WithOne(p => p.Permiso);
        }

    }
}
