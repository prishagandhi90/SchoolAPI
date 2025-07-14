namespace VHEmpAPI.Models
{
    public class PlayStoreReviewModel
    {
        public string ReviewId { get; set; }
        public string ReviewerLanguage { get; set; }
        public int StarRating { get; set; }
        public string ReviewText { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class ReviewFilterRequest
    {
        public string RatingFilter { get; set; } = "1-5";  // default
        public string Days { get; set; } = "30";  // default
        public bool ExcludeDevComment { get; set; } = true;  // default
    }
}
