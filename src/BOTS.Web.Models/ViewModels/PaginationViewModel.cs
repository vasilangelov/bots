namespace BOTS.Web.Models.ViewModels
{
    public class PaginationViewModel<T>
    {
        public int CurrentPage { get; set; }

        public int PageCount { get; set; }

        public int DisplayCount { get; set; } = 5;

        public int ItemsPerPage { get; set; }

        public IEnumerable<T> Items { get; set; }
            = Enumerable.Empty<T>();
    }
}
