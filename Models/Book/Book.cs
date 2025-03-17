namespace poplensMediaApi.Models.Book {
    public class Book {
        public string Key { get; set; }
        public string Title { get; set; }
        public List<string> Subjects { get; set; }
        public List<int> PublishDate { get; set; }
        public int? CoverId { get; set; }
        public string Description { get; set; }
        public List<string> Authors { get; set; }
    }
}
