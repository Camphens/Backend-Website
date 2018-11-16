namespace restserver.Paginator
{
    public class Page<T>
    {
        //Index of the current selected page
        public int Index {get;set;}
        //Amount of entities per page
        public T[] Items {get;set;}
        //Total pages
        public int TotalPages {get;set;}
    }
}

namespace ExtensionMethods
{
    using System;
    using System.Linq;
    using restserver.Paginator;
    public static class MyExtensions
    {
        public static Page<T> GetPage<T>(this Microsoft.EntityFrameworkCore.DbSet<T> list, int page_index, int page_size, Func<T, object> order_by_selector) where T:class
        {
            var res = list.OrderBy(order_by_selector)
            .Skip(page_index * page_size)
            .Take(page_size)
            .ToArray();
            //If there are no items, return null
            if(res==null || res.Length==0)
            return null;

            //Calcute total amount of items
            var tot_items = list.Count();

            //Calculate the total amount of pages
            var tot_pages = tot_items / page_size;
            if(tot_items < page_size) tot_pages = 1;

            //Group data and sned it to the caller(controller)
            return new Page<T>()
            {
                Index = page_index+1, Items = res, TotalPages = tot_pages
            };
        }
    }
}