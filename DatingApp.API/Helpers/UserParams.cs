namespace DatingApp.API.Helpers
{
    public class UserParams
    {
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
        
    }
}