// <copyright file="CaptchaService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    using System;

    /// <summary>
    /// Provides services for generating and validating captchas.
    /// </summary>
    public class CaptchaService : ICaptchaService
    {
        /// <summary>
        /// Validates if the entered captcha matches the generated captcha.
        /// </summary>
        /// <param name="generatedCaptcha">The generated captcha value.</param>
        /// <param name="currentCaptcha">The entered captcha value.</param>
        /// <returns>True if the entered captcha matches the generated captcha, otherwise false.</returns>
        public bool IsEnteredCaptchaValid(string generatedCaptcha, string currentCaptcha)
        {
            return generatedCaptcha == currentCaptcha;
        }

        /// <summary>
        /// Generates a random alphanumeric captcha code.
        /// </summary>
        /// <returns>A randomly generated alphanumeric captcha code.</returns>
        public string GenerateCaptcha()
        {
            var captchaCodeLength = Random.Shared.Next(6, 8);
            var captchaCode = string.Empty;

            while (captchaCode.Length != captchaCodeLength)
            {
                // Generate a character from one of three ranges: 0-9, A-Z, or a-z
                var characterType = Random.Shared.Next(3);
                var currentCharacterAscii = characterType switch
                {
                    0 => Random.Shared.Next(48, 58),  // 0-9 (ASCII 48-57)
                    1 => Random.Shared.Next(65, 91),  // A-Z (ASCII 65-90)
                    _ => Random.Shared.Next(97, 123),  // a-z (ASCII 97-122)
                };
                var currentCharacter = (char)currentCharacterAscii;

                if (!IsAlphaNumeric(currentCharacter))
                {
                    continue;
                }

                captchaCode += currentCharacter;
            }

            return captchaCode;
        }

        /// <summary>
        /// Checks if a character is alphanumeric.
        /// </summary>
        /// <param name="currentCharacter">The character to check.</param>
        /// <returns>True if the character is alphanumeric, otherwise false.</returns>
        private static bool IsAlphaNumeric(char currentCharacter)
        {
            return (currentCharacter >= '0' && currentCharacter <= '9')
                || (currentCharacter >= 'a' && currentCharacter <= 'z')
                || (currentCharacter >= 'A' && currentCharacter <= 'Z');
        }
    }
}
