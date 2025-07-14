using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace VHEmpAPI.Models.Repository
{
    public class GooglePlayReviewRepository : IGooglePlayReviewRepository
    {
        private readonly AndroidPublisherService _androidPublisherService;
        private const string PackageName = "com.venus_hospital.emp_app";

        public GooglePlayReviewRepository()
        {
            // Service account json path, apne environment ke hisab se set karna
            //var credential = GoogleCredential.FromFile("path/to/service-account.json")
            //                                .CreateScoped(AndroidPublisherService.Scope.Androidpublisher);
            //var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "GoogleReviewReplyCredentials", "service-account.json");
            var credentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GoogleReviewReplyCredentials", "vhempapp-3591e-e01f3da39c69.json");

            var credential = GoogleCredential.FromFile(credentialPath)
                .CreateScoped(AndroidPublisherService.Scope.Androidpublisher);

            _androidPublisherService = new AndroidPublisherService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Venus Workspace"
            });
        }

        #region commented old working code

        //public async Task<List<PlayStoreReviewModel>> GetAllReviewsAsync()
        //{
        //    var request = _androidPublisherService.Reviews.List(PackageName);
        //    request.MaxResults = 100;

        //    var response = await request.ExecuteAsync();
        //    var reviews = new List<PlayStoreReviewModel>();

        //    if (response.Reviews != null)
        //    {
        //        foreach (var review in response.Reviews)
        //        {
        //            var entry = review.Comments?.FirstOrDefault()?.UserComment;
        //            if (entry != null)
        //            {
        //                reviews.Add(new PlayStoreReviewModel
        //                {
        //                    ReviewId = review.ReviewId,
        //                    ReviewerLanguage = entry.ReviewerLanguage ?? "N/A",
        //                    StarRating = entry.StarRating ?? 0,
        //                    ReviewText = entry.Text,
        //                    LastModified = entry.LastModified?.Seconds != null
        //                        ? DateTimeOffset.FromUnixTimeSeconds((long)entry.LastModified.Seconds).DateTime
        //                        : DateTime.MinValue
        //                });
        //            }
        //        }
        //    }

        //    return reviews.OrderByDescending(x => x.LastModified).ToList();
        //}

        #endregion

        public async Task<List<PlayStoreReviewModel>> GetAllReviewsAsync(int? days = null, int? minRating = null, int? maxRating = null, bool excludeDevComment = true)
        {
            var request = _androidPublisherService.Reviews.List(PackageName);
            request.MaxResults = 10000; // Max allowed

            var response = await request.ExecuteAsync();
            var reviews = new List<PlayStoreReviewModel>();

            if (response.Reviews != null)
            {
                foreach (var review in response.Reviews)
                {
                    var entry = review.Comments?.FirstOrDefault()?.UserComment;
                    var reply = review.Comments?.FirstOrDefault(c => c.DeveloperComment != null)?.DeveloperComment;

                    if (entry == null) continue;

                    if (excludeDevComment && reply != null)
                        continue;

                    var reviewDate = entry.LastModified?.Seconds != null
                        ? DateTimeOffset.FromUnixTimeSeconds((long)entry.LastModified.Seconds).UtcDateTime
                        : DateTime.MinValue;

                    var starRating = entry.StarRating ?? 0;

                    // 🧠 Filter logic starts here
                    bool isWithinDateRange = !days.HasValue || (reviewDate >= DateTime.UtcNow.AddDays(-days.Value));
                    bool isWithinRatingRange = (!minRating.HasValue || starRating >= minRating.Value) &&
                                               (!maxRating.HasValue || starRating <= maxRating.Value);

                    if (isWithinDateRange && isWithinRatingRange)
                    {
                        reviews.Add(new PlayStoreReviewModel
                        {
                            ReviewId = review.ReviewId,
                            ReviewerLanguage = entry.ReviewerLanguage ?? "N/A",
                            StarRating = starRating,
                            ReviewText = entry.Text,
                            LastModified = reviewDate
                        });
                    }
                }
            }

            return reviews.OrderByDescending(x => x.LastModified).ToList();
        }


        public List<PlayStoreReviewModel> FilterReviewsByRating(List<PlayStoreReviewModel> reviews, string ratingFilter)
        {
            var range = ratingFilter.Split('-');

            if (range.Length == 2 &&
                int.TryParse(range[0], out int from) &&
                int.TryParse(range[1], out int to))
            {
                return reviews
                    .Where(x => x.StarRating >= from && x.StarRating <= to)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();
            }
            else if (range.Length == 1 && int.TryParse(range[0], out int exact))
            {
                return reviews
                    .Where(x => x.StarRating == exact)
                    .OrderByDescending(x => x.LastModified)
                    .ToList();
            }

            // default fallback: return all
            return reviews
                .OrderByDescending(x => x.LastModified)
                .ToList();
        }


        public async Task<bool> ReplyToReviewAsync(string packageName, string reviewId, string replyText)
        {
            //var reviewReply = new ReviewReply
            var reviewReply = new ReviewsReplyRequest
            {
                ReplyText = replyText
            };

            try
            {
                var request = _androidPublisherService.Reviews.Reply(reviewReply, packageName, reviewId);
                var response = await request.ExecuteAsync();
                return true;  // Reply successful
            }
            catch (Exception ex)
            {
                // Logging etc.
                Console.WriteLine("Error replying to review: " + ex.Message);
                return false;
            }
        }
    }
}
