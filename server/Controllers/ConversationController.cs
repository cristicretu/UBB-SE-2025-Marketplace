using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using System.Net;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationRepository conversationRepository;

        public ConversationController(IConversationRepository conversationRepository)
        {
            this.conversationRepository = conversationRepository;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ConversationDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto createConversationDto)
        {
            try
            {
                if (createConversationDto == null)
                {
                    return BadRequest("Request body cannot be null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var conversation = new Conversation
                {
                    UserId = createConversationDto.UserId
                };

                var createdConversation = await conversationRepository.CreateConversationAsync(conversation);
                var conversationDto = MapToConversationDto(createdConversation);

                return CreatedAtAction(nameof(GetConversation), new { id = conversationDto.Id }, conversationDto);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, exception.Message);
            }
        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ConversationDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetConversation(int id)
        {
            try
            {
                var conversation = await conversationRepository.GetConversationByIdAsync(id);

                if (conversation == null)
                {
                    return NotFound($"Conversation with id {id} not found.");
                }

                var conversationDto = MapToConversationDto(conversation);

                return Ok(conversationDto);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(List<ConversationDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserConversations(int userId)
        {
            try
            {
                var conversations = await conversationRepository.GetConversationsByUserIdAsync(userId);

                var conversationDtos = conversations.Select(MapToConversationDto).ToList();

                return Ok(conversationDtos);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        private ConversationDto MapToConversationDto(Conversation conversation)
        {
            return new ConversationDto
            {
                Id = conversation.Id,
                UserId = conversation.UserId
            };
        }
    }
}
