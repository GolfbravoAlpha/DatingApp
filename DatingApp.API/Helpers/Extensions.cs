using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddAppllicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        //sending information down to the client for the pagination
        public static void AddPagination(this HttpResponse response, 
            int currentPage, int itemsPerPage, int totalItems, int totalPages)
            {
                var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

                //serializeObject() converts it to camelCase
                var camelCaseformatter = new JsonSerializerSettings();
                camelCaseformatter.ContractResolver = new CamelCasePropertyNamesContractResolver();

                //convert 'paginationHeader' from objet to string using jsonConvert
                response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseformatter));
                //expose the headers so that we dont get a cors error
                response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
                //request a specific page number and page size
            }
        
         public static int CalculateAge(this DateTime theDateTime)
        {
            var age = DateTime.Today.Year - theDateTime.Year;
            if(theDateTime.AddYears(age) > DateTime.Today)
                age--;

            return age;
        }
    }   
}