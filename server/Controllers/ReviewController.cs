using System.Net;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Models.DTOs.Mappers;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        [HttpGet("buyer/{buyerId}")]
        [ProducesResponseType(typeof(List<ReviewDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetReviewsByBuyer(int buyerId)
        {
            try
            {
                var buyer = new User { Id = buyerId };
                var reviews = _reviewRepository.GetAllReviewsByBuyer(buyer);

                foreach (var review in reviews)
                {
                    review.LoadGenericImages();
                }

                var reviewDtos = reviews.Select(ReviewMapper.ToDto).ToList();
                return Ok(reviewDtos);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("seller/{sellerId}")]
        [ProducesResponseType(typeof(List<ReviewDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetReviewsBySeller(int sellerId)
        {
            try
            {
                var seller = new User { Id = sellerId };
                var reviews = _reviewRepository.GetAllReviewsBySeller(seller);

                foreach (var review in reviews)
                {
                    review.LoadGenericImages();
                }

                var reviewDtos = reviews.Select(ReviewMapper.ToDto).ToList();
                return Ok(reviewDtos);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReviewDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateReview([FromBody] ReviewDTO reviewDto)
        {
            if (reviewDto == null)
            {
                return BadRequest("Review cannot be null.");
            }

            try
            {
                var review = ReviewMapper.ToModel(reviewDto);
                review.Id = 0;

                _reviewRepository.CreateReview(review);

                var createdReviewDto = ReviewMapper.ToDto(review);
                return CreatedAtAction(nameof(GetReviewsByBuyer), new { buyerId = review.BuyerId }, createdReviewDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ReviewDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult EditReview([FromBody] ReviewDTO reviewDto)
        {
            if (reviewDto == null)
            {
                return BadRequest("Review cannot be null.");
            }

            try
            {
                var review = ReviewMapper.ToModel(reviewDto);
                
                // Load existing review to ensure it exists
                var existingReview = _reviewRepository.GetAllReviewsByBuyer(new User { Id = review.BuyerId })
                    .FirstOrDefault(r => r.Id == review.Id && r.SellerId == review.SellerId && r.BuyerId == review.BuyerId);

                if (existingReview == null)
                {
                    return NotFound("Review not found.");
                }

                // Update the review properties
                existingReview.Description = review.Description;
                existingReview.Rating = review.Rating;
                existingReview.Images = review.Images;
                existingReview.SyncImagesBeforeSave();

                _reviewRepository.EditReview(existingReview, existingReview.Rating, existingReview.Description);

                var updatedReviewDto = ReviewMapper.ToDto(existingReview);
                return Ok(updatedReviewDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteReview([FromQuery] int reviewId, [FromQuery] int sellerId, [FromQuery] int buyerId)
        {
            if (reviewId <= 0 || sellerId <= 0 || buyerId <= 0)
            {
                return BadRequest("Invalid review, seller, or buyer ID.");
            }

            try
            {
                var review = new Review { Id = reviewId, SellerId = sellerId, BuyerId = buyerId };
                _reviewRepository.DeleteReview(review);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }
    }
}
