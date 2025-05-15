// <copyright file="ICaptchaService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Provides methods for generating and validating captchas.
    /// </summary>
    public interface ICaptchaService
    {
        /// <summary>
        /// Validates if the entered captcha matches the generated captcha.
        /// </summary>
        /// <param name="generatedCaptcha">The generated captcha value.</param>
        /// <param name="currentCaptcha">The entered captcha value.</param>
        /// <returns>True if the entered captcha matches the generated captcha, otherwise false.</returns>
        bool IsEnteredCaptchaValid(string generatedCaptcha, string currentCaptcha);

        /// <summary>
        /// Generates a random alphanumeric captcha code.
        /// </summary>
        /// <returns>A randomly generated alphanumeric captcha code.</returns>
        string GenerateCaptcha();
    }
}
