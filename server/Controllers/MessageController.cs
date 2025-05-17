using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using System.Net;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository messageRepository;

        public MessageController(IMessageRepository messageRepository)
        {
            this.messageRepository = messageRepository;
        }

        [HttpPost]
        [ProducesResponseType(typeof(MessageDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageDto createMessageDto)
        {
            if (createMessageDto == null)
            {
                return BadRequest("Request body cannot be null");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var message = await messageRepository.CreateMessageAsync(createMessageDto);

                var messageDto = new MessageDto
                {
                    Id = message.Id,
                    ConversationId = message.ConversationId,
                    UserId = message.UserId,
                    Content = message.Content
                };

                return CreatedAtAction(nameof(GetMessagesByConversation), new { conversationId = messageDto.ConversationId }, messageDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet("conversation/{conversationId}")]
        [ProducesResponseType(typeof(List<MessageDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessagesByConversation(int conversationId)
        {
            try
            {
                var messages = await messageRepository.GetMessagesByConversationIdAsync(conversationId);

                var messageDtos = messages.Select(message => new MessageDto
                {
                    Id = message.Id,
                    ConversationId = message.ConversationId,
                    UserId = message.UserId,
                    Content = message.Content
                }).ToList();

                return Ok(messageDtos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
