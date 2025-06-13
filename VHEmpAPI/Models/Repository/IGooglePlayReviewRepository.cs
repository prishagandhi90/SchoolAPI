namespace VHEmpAPI.Models.Repository
{
    public interface IGooglePlayReviewRepository
    {
        Task<List<PlayStoreReviewModel>> GetAllReviewsAsync(int? days = null, int? minRating = null, int? maxRating = null, bool excludeDevComment = true);
        List<PlayStoreReviewModel> FilterReviewsByRating(List<PlayStoreReviewModel> reviews, string ratingFilter);
        Task<bool> ReplyToReviewAsync(string packageName, string reviewId, string replyText);
    }
}
