using System.ComponentModel.DataAnnotations;

namespace WebMarketplace.Models
{
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        [Required]
        public string Password { get; set; }

        ///<summary>
        /// Captcha input
        /// </summary>
        [Required]
        public string CaptchaInput { get; set; }
    }
}