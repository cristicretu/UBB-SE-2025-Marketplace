// <copyright file="UserApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing user data.
    /// </summary>
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly IUserRepository userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserApiController"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository dependency.</param>
        public UserApiController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>An ActionResult containing the user if found, otherwise appropriate error status.</returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User?>> GetUserById(int userId)
        {
            if (userId <= 0)
            {
                return this.BadRequest("User ID must be a positive integer.");
            }

            try
            {
                User? user = await this.userRepository.GetUserById(userId);

                return this.Ok(user);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting user by ID.");
            }
        }

        /// <summary>
        /// Gets a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>An ActionResult containing the user if found, otherwise appropriate error status.</returns>
        [AllowAnonymous]
        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User?>> GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return this.BadRequest("Email address is required.");
            }

            try
            {
                User? user = await this.userRepository.GetUserByEmail(email);

                return this.Ok(user);
            }
            catch (Exception)
            {
                // Return a 500 Internal Server Error
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting user by email.");
            }
        }

        /// <summary>
        /// Gets a user by their username.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>An ActionResult containing the user if found, otherwise appropriate error status.</returns>
        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User?>> GetUserByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return this.BadRequest("Username is required.");
            }

            try
            {
                User? user = await this.userRepository.GetUserByUsername(username);

                return this.Ok(user);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting user by username.");
            }
        }

        /// <summary>
        /// Gets the phone number and email for a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>An ActionResult containing the user with phone number and email loaded, otherwise appropriate error status.</returns>
        [HttpGet("phone-email/{userId}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User?>> GetUserPhoneNumberAndEmailById(int userId)
        {
            try
            {
                User user = new User { UserId = userId };
                await this.userRepository.LoadUserPhoneNumberAndEmailById(user);

                return this.Ok(user);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting phone number and email.");
            }
        }

        /// <summary>
        /// Adds a new user.
        /// </summary>
        /// <param name="user">The user object to add.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddUser([FromBody] User user)
        {
            if (user == null)
            {
                return this.BadRequest("User is required.");
            }

            try
            {
                await this.userRepository.AddUser(user);
                return this.Created();
            }
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding user.");
            }
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="user">The user object with updated information.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [AllowAnonymous]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateUser([FromBody] User user)
        {
            if (user == null)
            {
                return this.BadRequest("User is required.");
            }

            try
            {
                await this.userRepository.UpdateUser(user);
                return this.NoContent();
            }
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating user.");
            }
        }

        /// <summary>
        /// Checks if an email address already exists.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>An ActionResult containing true if the email exists, false otherwise, or an error status.</returns>
        [AllowAnonymous]
        [HttpGet("email-exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> EmailExists([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return this.BadRequest("Email address is required.");
            }

            try
            {
                bool exists = await this.userRepository.EmailExists(email);
                return this.Ok(exists);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while checking email existence.");
            }
        }

        /// <summary>
        /// Checks if a username already exists.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>An ActionResult containing true if the username exists, false otherwise, or an error status.</returns>
        [AllowAnonymous]
        [HttpGet("username-exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> UsernameExists([FromQuery] string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return this.BadRequest("Username is required.");
            }

            try
            {
                bool exists = await this.userRepository.UsernameExists(username);
                return this.Ok(exists);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while checking username existence.");
            }
        }

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>An ActionResult containing a list of all users, or an error status.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            try
            {
                var users = await this.userRepository.GetAllUsers();
                return this.Ok(users);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting all users.");
            }
        }

        /// <summary>
        /// Gets the failed login count for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>An ActionResult containing the failed login count, or an error status.</returns>
        [AllowAnonymous]
        [HttpGet("failed-logins-count/{userId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetFailedLoginsCountByUserId(int userId)
        {
            try
            {
                var failedLoginsCount = await this.userRepository.GetFailedLoginsCountByUserId(userId);
                return this.Ok(failedLoginsCount);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting users failed login count.");
            }
        }

        /// <summary>
        /// Updates the failed login count for a user.
        /// </summary>
        /// <param name="failedLoginsCount">The new failed login count.</param>
        /// <param name="user">The user object (only UserId is typically needed).</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [AllowAnonymous]
        [HttpPut("update-failed-logins/{failedLoginsCount}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateFailedLoginsCount(int failedLoginsCount, [FromBody] User user)
        {
            if (user == null)
            {
                return this.BadRequest("User is required.");
            }

            try
            {
                await this.userRepository.UpdateUserFailedLoginsCount(user, failedLoginsCount);
                return this.NoContent();
            }
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating failed logins count.");
            }
        }

        /// <summary>
        /// Updates the phone number for a user.
        /// </summary>
        /// <param name="user">The user object containing the UserId and the new PhoneNumber.</param>
        /// <returns>An ActionResult indicating success or failure.</returns>
        [HttpPut("update-phone-number")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdatePhoneNumber([FromBody] User user)
        {
            if (user == null)
            {
                return this.BadRequest("User is required.");
            }

            try
            {
                await this.userRepository.UpdateUserPhoneNumber(user);
                return this.NoContent();
            }
            catch
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating user phone number.");
            }
        }

        /// <summary>
        /// Gets the total number of users.
        /// </summary>
        /// <returns>An ActionResult containing the total count of users, or an error status.</returns>
        [HttpGet("count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetTotalNumberOfUsers()
        {
            try
            {
                var usersCount = await this.userRepository.GetTotalNumberOfUsers();
                return this.Ok(usersCount);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting total number of users.");
            }
        }
    }
}
