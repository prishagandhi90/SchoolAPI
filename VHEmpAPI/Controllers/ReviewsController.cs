using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using VHEmpAPI.Interfaces;
using VHEmpAPI.Models;
using VHEmpAPI.Models.Repository;
using VHMobileAPI.Models;
using static VHEmpAPI.Shared.CommonProcOutputFields;

namespace VHEmpAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IGooglePlayReviewRepository _reviewRepository;

        public ReviewsController(IGooglePlayReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        [HttpPost("all")]
        public async Task<IActionResult> GetAllReviews([FromBody] ReviewFilterRequest request)
        {
            var range = request.RatingFilter.Split('-');
            int minRating = 1, maxRating = 5, days = String.IsNullOrEmpty(request.Days) ? 30 : Convert.ToInt16(request.Days);

            if (range.Length == 2 &&
                int.TryParse(range[0], out int from) &&
                int.TryParse(range[1], out int to))
            {
                minRating = from;
                maxRating = to;
            }

            var filteredReviews = await _reviewRepository.GetAllReviewsAsync(days: days, minRating: minRating, maxRating: maxRating, excludeDevComment: request.ExcludeDevComment);
            //var filteredReviews = _reviewRepository.FilterReviewsByRating(reviews, request.RatingFilter);
            if (filteredReviews.Count == 0)
                return Ok(new { StatusCode = Ok(filteredReviews).StatusCode, IsSuccess = "false", Message = "No Data found!", Data = new { } });

            return Ok(new { StatusCode = Ok(filteredReviews).StatusCode, IsSuccess = "true", Message = "Successful!", Data = filteredReviews });
        }

        [HttpPut("reply")]
        public async Task<IActionResult> ReplyToReview([FromBody] ReviewReplyRequest request)
        {
            if (string.IsNullOrEmpty(request.PackageName) || string.IsNullOrEmpty(request.ReviewId) || string.IsNullOrEmpty(request.ReplyText))
            {
                return BadRequest("PackageName, ReviewId, and ReplyText are required.");
            }

            var success = await _reviewRepository.ReplyToReviewAsync(request.PackageName, request.ReviewId, request.ReplyText);
            if (success)
                return Ok("Reply posted successfully.");
            else
                return StatusCode(500, "Failed to post reply.");
        }
    }

    public class ReviewReplyRequest
    {
        public string PackageName { get; set; }
        public string ReviewId { get; set; }
        public string ReplyText { get; set; }
    }
}
