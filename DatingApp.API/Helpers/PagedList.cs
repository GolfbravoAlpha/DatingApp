using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers
{
    //using generics to allow us to send users or messages model
    //<T> can be swapped for users, members or messages
    //it will return back the data with the page count 
    public class PagedList<T> : List<T> 
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }  
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count; //number of users
            PageSize = pageSize; //the amount of users that can fit on one page
            CurrentPage = pageNumber;
            //total pages = totalcount\pagesize, this gives us the number of pages
            //ceiling returns the number rouded up
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        //this method will return a new instance of a paged list 
        //IQueryable allows us to defer the execution of the request to get a bunch of users
        //IQueryable<T> will allow users and message as it is generic 
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, 
            int pageNumber, int pageSize)
        {
            //working out the total count of the items, countAsync() is required to do this
            var count = await source.CountAsync();
            /* get the items from the source of items that are passed in here
            *  we use skip and take operators, when getting the next page we need to skip the ones we have already seen
            *  Once you have already seen page 1, you need to skip those 5 users and then take the next 5
            *  e.g. if the pagenumber the user is on is 2, 2 -1 = 1,  1 * 5 = 5, therefore we skip 5 and take the rest. 
            */
            var items = await source.Skip((pageNumber -1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}