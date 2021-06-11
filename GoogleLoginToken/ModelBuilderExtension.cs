using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Utilitats;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using Gabriel.Cat.S.Blazor;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoogleLoginToken
{
    public static class ModelBuilderExtension
    {
        const string ID = "Id";
        const string RelationNN = "#";
        static SortedList<string, TwoKeysList<string, string, bool>> DicRelations { get; set; } = new SortedList<string, TwoKeysList<string, string, bool>>();
        /// <summary>
        /// Lee las propiedades con el atributo [Key] y luego las añade al metodo HasKey
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="modelBuilder"></param>
        public static void ConfigureKeys<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, new()
        {

            const string KEY = nameof(KeyAttribute);
            IList<PropiedadTipo> propiedades = typeof(TEntity).GetPropiedadesTipos();
            List<string> keys = new List<string>();
            for (int i = 0; i < propiedades.Count; i++)
            {
                if (propiedades[i].Atributos.Any(attr => attr.GetType().Name == KEY))
                    keys.Add(propiedades[i].Nombre);
            }
            if (keys.Count > 1)
                modelBuilder.Entity<TEntity>().HasKey(keys.ToArray());

        }

        /// <summary>
        /// Configura las relaciones 1-1,1-n,n-1,n-n de cada propiedad
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="modelBuilder"></param>
        public static void ConfigureRelations<TEntity>(this ModelBuilder modelBuilder, string assemblyQualifiedName) where TEntity : class, new()
        {
            bool encontrado;
            Type arrayType;
            string nombrePropiedad, nombrePropiedadAux;
            IList<PropiedadTipo> propiedadesProperty;
            IList<PropiedadTipo> propiedades = typeof(TEntity).GetPropiedadesTipos();

            Type tipoEntity = typeof(TEntity);
            if (!DicRelations.ContainsKey(assemblyQualifiedName))
                DicRelations.Add(assemblyQualifiedName, new TwoKeysList<string, string, bool>());
            for (int i = 0; i < propiedades.Count; i++)
            {
                try
                {
                    if (propiedades[i].Uso.HasFlag(UsoPropiedad.Set))
                    {

                        if (propiedades[i].Tipo.ImplementInterficie(typeof(ICollection<>)))
                        {
                            arrayType = propiedades[i].Tipo.GetGenericArguments()[0];
                            if (arrayType.IsClass && !arrayType.AssemblyQualifiedName.Contains(nameof(System)))
                            {

                                propiedadesProperty = arrayType.GetPropiedadesTipos();
                                //n-1
                                //n-n -> siempre será n-1 ya que se usa un tipo por medio
                                encontrado = false;
                                for (int j = 0; j < propiedadesProperty.Count && !encontrado; j++)
                                {

                                    encontrado = propiedadesProperty[j].Tipo.Equals(tipoEntity);
                                    if (encontrado)
                                    {
                                        nombrePropiedad = propiedades[i].GetNombrePropiedadDestino();
                                        if (String.IsNullOrEmpty(nombrePropiedad))
                                        {
                                            nombrePropiedad = propiedadesProperty[j].Nombre;
                                        }
                                        if (!nombrePropiedad.Contains(RelationNN))
                                        {
                                            if (NotContainsKey($"{tipoEntity.Name}.{propiedades[i].Nombre}", $"{propiedades[i].Tipo.Name}.{nombrePropiedad}", assemblyQualifiedName))
                                            {
                                                modelBuilder.Entity<TEntity>().HasMany(propiedades[i].Nombre).WithOne(nombrePropiedad);
                                                DicRelations[assemblyQualifiedName].Add($"{tipoEntity.Name}.{propiedades[i].Nombre}", $"{propiedades[i].Tipo.Name}.{nombrePropiedad}", true);
                                            }
                                        }
                                        else
                                        {
                                            nombrePropiedad = nombrePropiedad.Substring(0, nombrePropiedad.Length - RelationNN.Length);
                                            nombrePropiedadAux = propiedades[i].GetNombrePropiedadDestino(false);
                                            if (!NotContainsKey(nombrePropiedad, nombrePropiedadAux, assemblyQualifiedName))
                                            {
                                                modelBuilder.Entity<TEntity>().HasMany(nombrePropiedad).WithMany(nombrePropiedadAux);
                                                DicRelations[assemblyQualifiedName].Add(nombrePropiedad, nombrePropiedadAux, true);

                                            }

                                        }

                                    }
                                }

                            }

                        }
                        else if (propiedades[i].Tipo.IsClass && !propiedades[i].Tipo.AssemblyQualifiedName.Contains(nameof(System)))
                        {

                            propiedadesProperty = propiedades[i].Tipo.GetPropiedadesTipos();
                            //1-n
                            //1-1
                            encontrado = false;
                            for (int j = 0; j < propiedadesProperty.Count && !encontrado; j++)
                            {
                                if ((!propiedadesProperty[j].Tipo.ImplementInterficie(typeof(ICollection<>)) && !propiedadesProperty[j].Tipo.AssemblyQualifiedName.Contains(nameof(System))) || (propiedadesProperty[j].Tipo.ImplementInterficie(typeof(ICollection<>)) && !propiedadesProperty[j].Tipo.GetGenericArguments()[0].AssemblyQualifiedName.Contains(nameof(System))))
                                {
                                    nombrePropiedad = propiedades[i].GetNombrePropiedadDestino();
                                    if (String.IsNullOrEmpty(nombrePropiedad))
                                    {
                                        nombrePropiedad = propiedadesProperty[j].Nombre;
                                    }
                                    if (nombrePropiedad.Contains(RelationNN))
                                    {
                                        encontrado = true;
                                        if (NotContainsKey($"{tipoEntity.Name}.{propiedades[i].Nombre}", $"{propiedadesProperty[j].Tipo.Name}.{nombrePropiedad}", assemblyQualifiedName))
                                        {
                                            modelBuilder.Entity<TEntity>().HasOne(propiedades[i].Nombre).WithOne( propiedadesProperty[j].Nombre).HasForeignKey(propiedadesProperty[j].Tipo, GetForeingKeyName(propiedadesProperty[j]));
                                            DicRelations[assemblyQualifiedName].Add($"{tipoEntity.Name}.{propiedades[i].Nombre}", $"{propiedadesProperty[j].Tipo.Name}.{nombrePropiedad}", true);
                                        }
                                    }
                                    else if (!Equals(nombrePropiedad, ID))
                                    {
                                        if (propiedadesProperty[j].Tipo.IsGenericType)
                                        {

                                            //1-n
                                            encontrado = propiedadesProperty[j].Tipo.GetGenericArguments()[0].Equals(tipoEntity);
                                            if (encontrado)
                                            {
                                                if (NotContainsKey($"{tipoEntity.Name}.{propiedades[i].Nombre}", $"{propiedadesProperty[j].Tipo.Name}.{nombrePropiedad}", assemblyQualifiedName))
                                                {
                                                    modelBuilder.Entity<TEntity>().HasOne(propiedades[i].Nombre).WithMany(nombrePropiedad);
                                                    DicRelations[assemblyQualifiedName].Add($"{tipoEntity.Name}.{propiedades[i].Nombre}", $"{propiedadesProperty[j].Tipo.Name}.{nombrePropiedad}", true);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //1-1
                                            encontrado = propiedadesProperty[j].Tipo.Equals(tipoEntity);
                                            if (encontrado)
                                            {
                                                if (NotContainsKey($"{tipoEntity.Name}.{propiedades[i].Nombre}", $"{propiedadesProperty[j].Tipo.Name}.{nombrePropiedad}", assemblyQualifiedName))
                                                {
                                                    modelBuilder.Entity<TEntity>().HasOne(propiedades[i].Nombre).WithOne(nombrePropiedad);
                                                    DicRelations[assemblyQualifiedName].Add($"{tipoEntity.Name}.{propiedades[i].Nombre}", $"{propiedadesProperty[j].Tipo.Name}.{nombrePropiedad}", true);
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        encontrado = true;

                                        modelBuilder.Entity<TEntity>().HasOne(propiedades[i].Nombre).WithMany().HasForeignKey(propiedades[i].Nombre + nombrePropiedad);
                                    }



                                }
                            }

                        }
                    }
                }
                catch { }

            }
        }

        private static string[] GetForeingKeyName(PropiedadTipo propiedadTipo)
        {
            string name;
            ForeignKeyAttribute attrForingKey = propiedadTipo.Atributos.Where(a => a is ForeignKeyAttribute).FirstOrDefault() as ForeignKeyAttribute;
            if (Equals(attrForingKey, default(ForeignKeyAttribute)))
                name = propiedadTipo.Nombre + ID;
            else name = attrForingKey.Name;

            return new string[]{ name};
        }

        static bool NotContainsKey(string key1, string key2, string bdName)
        {
            return !DicRelations[bdName].ContainsKey(new TwoKeys<string, string>(key1, key2)) &&
                !DicRelations[bdName].ContainsKey(new TwoKeys<string, string>(key2, key1));
        }

        static string GetNombrePropiedadDestino(this PropiedadTipo propiedadTipo, bool cambiarNN = true)
        {
            string name = String.Empty;
            Type tipo = propiedadTipo.Tipo.GetTipo();
            IList<PropiedadTipo> propiedadesTipo = tipo.GetPropiedadesTipos();
            Attribute attributeForeingKey = propiedadTipo.Atributos.Where(p => p is ForeignKeyAttribute).FirstOrDefault();
            if (propiedadTipo.Nombre == nameof(UserPermiso.Granted) && false)
                System.Diagnostics.Debugger.Break();
            if (!Equals(attributeForeingKey, default(Attribute)))
            {
                name = ((ForeignKeyAttribute)attributeForeingKey).Name;
            }
            //si contiene el nombre algo del tipo es 1-1 o 1-n
            else if (!tipo.Name.Contains(propiedadTipo.Nombre))
            {   //sino puede ser que use el nombre de la propiedad en la clase //tener en cuenta un atributo para ponerle nombre 
                name = propiedadesTipo.Where(
                    p =>
                    {

                        bool valid = !p.Tipo.GetTipo().FullName.Contains(nameof(System)) && !p.Nombre.Equals(propiedadTipo.Nombre);

                        if (valid)
                            valid = p.Tipo.IsGenericType ? p.Nombre.Contains(propiedadTipo.Nombre) : propiedadTipo.Nombre.Contains(p.Nombre);
                        return valid;
                    })
                    .Select(p => p.Nombre).FirstOrDefault();

            }
            else
            {
                name = ID;
            }
            try
            {
                if (!string.IsNullOrEmpty(name) && tipo.Name.Contains(name) && cambiarNN)
                {
                    //n-n
                    name = propiedadesTipo.Where(p => !p.Tipo.GetTipo().FullName.Contains(nameof(System)) && tipo.Name.Contains(name) && p.Tipo.Name != name).Select(p => p.Nombre).FirstOrDefault();
                    if (string.IsNullOrEmpty(name))
                    {
                        name = ID;
                    }

                    name += RelationNN;

                }
            }
            catch
            {
                System.Diagnostics.Debugger.Break();
            }

            return name;
        }
        public static Type GetTipo(this Type tipo)
        {
            return tipo.IsGenericType ? tipo.GetGenericArguments()[0] : tipo;
        }
        /// <summary>
        /// Configura las Keys y las Relaciones
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="modelBuilder"></param>
        static void IConfigureEntity<TEntity>(this ModelBuilder modelBuilder, string bdName) where TEntity : class, new()
        {
            modelBuilder.ConfigureKeys<TEntity>();
            modelBuilder.ConfigureRelations<TEntity>(bdName);
        }

        public static void ConfigureEntity(this ModelBuilder modelBuilder, Type tipoEntity, string assemblyQualifiedName)
        {
            MethodInfo method;
            MethodInfo generic;

            method = typeof(ModelBuilderExtension).GetMethod(nameof(IConfigureEntity), BindingFlags.Static | BindingFlags.NonPublic);
            generic = method.MakeGenericMethod(tipoEntity);
            generic.Invoke(null, new object[] { modelBuilder, assemblyQualifiedName });



        }
        public static void ConfigureEntities(this ModelBuilder modelBuilder, string assemblyQualifiedName, params Type[] tiposDbContext)
        {
            for (int i = 0; i < tiposDbContext.Length; i++)
                ConfigureEntity(modelBuilder, tiposDbContext[i], assemblyQualifiedName);
            DicRelations.Remove(assemblyQualifiedName);
        }

        public static void ConfigureEntities<T>(this T context, ModelBuilder modelBuilder) where T : DbContext
        {
            string assemblyQualifiedName = context.GetType().AssemblyQualifiedName;

            modelBuilder.ConfigureEntities(assemblyQualifiedName, context.GetType().GetPropiedadesTipos().Where(p => p.Tipo.IsGenericType && p.Tipo.GetGenericTypeDefinition().Equals(typeof(DbSet<>))).Select(p => p.Tipo.GetGenericArguments()[0]).ToArray());
        }

    }
}
