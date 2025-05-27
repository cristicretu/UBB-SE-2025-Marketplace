using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int ConversationId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Content { get; set; }
        [ForeignKey("ConversationId")]
        public Conversation Conversation { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
    }

    public class CreateMessageDto
    {
        [Required]
        public int ConversationId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Content { get; set; }
    }
}