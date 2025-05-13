using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("Conversations")]
    public class Conversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        // Navigation property
        [ForeignKey("UserId")]
        public User User { get; set; }
        
        // Navigation property for messages in this conversation
        public ICollection<Message> Messages { get; set; }
    }
    
    // DTOs
    public class ConversationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
    }
    
    public class CreateConversationDto
    {
        [Required]
        public int UserId { get; set; }
    }
} 