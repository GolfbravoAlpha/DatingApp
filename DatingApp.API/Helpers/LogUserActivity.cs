using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using DatingApp.API.Data;
using System;

namespace DatingApp.API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        //this is to allow us to see when the user last logged in
        //it's going to use the usersController to see when was the last time the api was accessed by that user
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            /*'await' means wait until the action has completed 
            * allows for multithreading, context refers to the action being exectued  
            * 'next' refers to when the action has been exected
            * the below only uses 'next'
            */           

            //takes on the 'next' action 
            var resultContext = await next();
            //get user ID from the token. 
            var userID = int.Parse(resultContext.HttpContext.User
                .FindFirst(ClaimTypes.NameIdentifier).Value);    
            //get an instnace of the dating repository
            //this will allow us to manipulate the database         
            var repo = resultContext.HttpContext.RequestServices.GetService<IDatingRepository>();
            //get user object with photo from the database. This will allow us to have access to the last active property
            var user = await repo.GetUser(userID);
            //set last active on the user 
            user.LastActive = DateTime.Now;
            //saving last active back to the users table in the database
            await repo.SaveAll();
                
        }
    }
}