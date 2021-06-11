using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Utilitats;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleLoginToken
{
    public static class DbSetExtension
    {
        /// <summary>
        /// Así no hay propiedades con valores nulos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static IQueryable<T> IncludeAll<T>(this DbSet<T> dbSet,bool recursive=true) where T:class
        {
            IList<PropiedadTipo> propiedadTipos;
            IQueryable<T> lastQuery= dbSet;
            Type tipoDbSet = typeof(T);


            if (!recursive)
            {
                propiedadTipos = tipoDbSet.GetPropiedadesTipos();

                for (int i = 0; i < propiedadTipos.Count; i++)
                {
                    if (!propiedadTipos[i].Tipo.FullName.Contains(nameof(System)))
                    {
                        lastQuery = lastQuery.Include(propiedadTipos[i].Nombre);
                    }
                }
            }
            else
            {
                lastQuery = IIncludeAll(lastQuery, tipoDbSet,string.Empty, new SortedList<string, string>());
            }
            return lastQuery;

        }
        static IQueryable<T> IIncludeAll<T>(IQueryable<T> query,Type tipo,string nombrePropiedad,SortedList<string,string> dicTiposCargados) where T:class
        {
            IList<PropiedadTipo> propiedadTipos;
            if (!dicTiposCargados.ContainsKey(tipo.FullName))
            {
                dicTiposCargados.Add(tipo.FullName, tipo.FullName);
                if(!string.IsNullOrEmpty(nombrePropiedad))
                   query = query.Include(nombrePropiedad);
                propiedadTipos = tipo.GetPropiedadesTipos();

                for (int i = 0; i < propiedadTipos.Count; i++)
                {
                    if (!propiedadTipos[i].Tipo.FullName.Contains(nameof(System)) && !dicTiposCargados.ContainsKey(propiedadTipos[i].Tipo.FullName))
                    {
                        dicTiposCargados.Add(propiedadTipos[i].Tipo.FullName, propiedadTipos[i].Tipo.FullName);
                        query = IIncludeAll(query, propiedadTipos[i].Tipo,propiedadTipos[i].Nombre, dicTiposCargados);
                    }
                }

            }
            return query;
        }
    }
}
