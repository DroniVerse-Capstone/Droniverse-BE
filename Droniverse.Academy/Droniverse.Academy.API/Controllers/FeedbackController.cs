using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers
{
    [Route("academy/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly ILogger<FeedbackController> _logger;
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(ILogger<FeedbackController> logger, IFeedbackService feedbackService)
        {
            _logger = logger;
            _feedbackService = feedbackService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacks();
            return Ok(feedbacks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(Guid id)
        {
            var feedback = await _feedbackService.GetFeedbackById(id);
            return Ok(feedback);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackCreateDTO feedbackCreateDto)
        {
            var createdFeedback = await _feedbackService.CreateFeedback(feedbackCreateDto);
            return CreatedAtAction(nameof(GetFeedbackById), new { id = createdFeedback.FeedbackID }, createdFeedback);
        }
    }
}
