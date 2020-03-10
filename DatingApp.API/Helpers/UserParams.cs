namespace DatingApp.API.Helpers
{
    public class UserParams
    {
        //to set the defalt when the client is searching the user pagination
        private const int MaxPageSize = 50;
        public int Pagenumber { get; set; } = 1;
        private int pageSize = 10;
        public int PageSize
        {
            get { return pageSize; }
            //stops client from asking a very high number of pages size returns
            //if its above 50, the turnery operator sets it back to 50
            set { pageSize = (value > MaxPageSize) ? MaxPageSize: value; }
        }

        //filter out current user
        public int UserId { get; set; }

        //filter based on gender
        public string Gender { get; set; }
        //filter based on age
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 99;
        public string OrderBy { get; set; }
        
    }
}