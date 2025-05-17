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
                review.SyncImagesBeforeSave();

                _reviewRepository.EditReview(review, review.Rating, review.Description);

                var updatedReviewDto = ReviewMapper.ToDto(review);
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
        public IActionResult DeleteReview([FromBody] ReviewDTO reviewDto)
        {
            if (reviewDto == null)
            {
                return BadRequest("Review cannot be null.");
            }

            try
            {
                var review = ReviewMapper.ToModel(reviewDto);
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
