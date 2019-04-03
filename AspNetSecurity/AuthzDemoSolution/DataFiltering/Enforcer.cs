using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ModelLibrary;

namespace DataFiltering
{
    public static class Enforce
    {
        public static readonly MethodInfo _enforceAgeAndOwner =
            typeof(Enforce).GetMethod("EnforceAgeAndOwner");

        //public static IQueryable<T> EnforceAgeAndOwner<T>(
        //    this IQueryable<T> source,
        //    Maturity maturity)
        //    where T : class
        //{
        //    //return source.Annotate(Expression.Constant(notes));
        //    var method = _enforceAgeAndOwner.MakeGenericMethod(typeof(T));

        //    return source
        //        //.IgnoreQueryFilters() // should you have GlobalFilters
        //        .Provider.CreateQuery<T>(
        //        Expression.Call(
        //            null,
        //            method,
        //            new Expression[] { source.Expression,
        //                Expression.Constant(maturity) }
        //            ));
        //}


        // this overload takes a DbSet<T> so that it will
        // be the first call found from the visitor
        public static IQueryable<T> EnforceAgeAndOwner<T>(
            this IQueryable<T> source,
            string username, Maturity usernameMaturity)
            where T : class
        {
            if (username == null)
            {
                return source;
            }

            var method = _enforceAgeAndOwner.MakeGenericMethod(typeof(T));

            return source
                //.IgnoreQueryFilters() // should you have GlobalFilters
                .Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    method,
                    new Expression[]
                    {
                        source.Expression,
                        Expression.Constant(username),
                        Expression.Constant(usernameMaturity)
                    })
                );
        }

    }
}
