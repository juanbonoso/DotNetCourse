using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotNetApi.Data;
using DotNetApi.Dtos;
using DotNetApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotNetApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;
        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration config)
        {
            _config = config;
            _dapper = new DataContextDapper(_config);
            _authHelper = new AuthHelper(_config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserForRegistrationDto userForRegistrationDto)
        {
            if (userForRegistrationDto.Password != userForRegistrationDto.PasswordConfirm)
            {
                return BadRequest("Passwords do not match");
            }

            string sqlCheckUserExists = "SELECT COUNT(1) FROM TutorialAppSchema.Auth WHERE Email = @Email";
            var userExists = _dapper.LoadDataSingleWithParams<int>(sqlCheckUserExists, new { userForRegistrationDto.Email });
            if (userExists > 0)
            {
                return BadRequest("User already exists with this email");
            }

            UserForLoginDto userForLoginDto = new()
            {
                Email = userForRegistrationDto.Email,
                Password = userForRegistrationDto.Password
            };
            bool insertSuccess = await _authHelper.SetPassword(userForLoginDto);

            if (!insertSuccess)
            {
                return StatusCode(500, "Internal server error");
            }

            string sqlAddUser = "TutorialAppSchema.spUser_Upsert @FirstName, @LastName, @Email, @Gender, @JobTitle, @Department, @Salary, @Active";
            var insertParameters = new
            {
                userForRegistrationDto.FirstName,
                userForRegistrationDto.LastName,
                userForRegistrationDto.Email,
                userForRegistrationDto.Gender,
                userForRegistrationDto.JobTitle,
                userForRegistrationDto.Department,
                userForRegistrationDto.Salary,
                Active = 1,
            };
            if (!await _dapper.ExecuteSqlAsync(sqlAddUser, insertParameters))
            {
                return StatusCode(500, "Failed to add user");
            }

            return Ok();
        }

        [HttpPut("ResetPassword")]
        public async Task<IActionResult> ResetPassword(UserForLoginDto userForLoginDto)
        {
            bool updateSuccess = await _authHelper.SetPassword(userForLoginDto);

            if (!updateSuccess)
            {
                return StatusCode(500, "Failed to update password");
            }

            return Ok("Password updated successfully");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            string sqlForHashAndSalt = "EXEC TutorialAppSchema.spLoginConfirmation_Get @Email=@EmailParam";
            DynamicParameters parameters = new();
            parameters.Add("EmailParam", userForLoginDto.Email, System.Data.DbType.String);
            UserForLoginConfirmationDto? userForConfirmationDto = await _dapper
                .LoadDataSingleWithParamsAsync<UserForLoginConfirmationDto>(sqlForHashAndSalt, parameters);

            if (userForConfirmationDto == null)
            {
                return Unauthorized();
            }

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLoginDto.Password, userForConfirmationDto.PasswordSalt);

            if (!passwordHash.SequenceEqual(userForConfirmationDto.PasswordHash))
            {
                return StatusCode(500, "Incorrect Password");
            }

            string sqlForUserId = "SELECT UserId FROM TutorialAppSchema.Users WHERE Email = @Email";
            int userId = await _dapper.LoadDataSingleWithParamsAsync<int>(sqlForUserId, new { userForLoginDto.Email });

            return Ok(new Dictionary<string, string>
            {
                { "ok", "true" },
                {"message", "User logged in successfully"},
                { "token", _authHelper.CreateToken(userId) }
            });
        }


        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            string userId = User.FindFirst("userId")?.Value + "";
            string userIdSQL = "SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = @UserId";
            int userIdFromDB = _dapper.LoadDataSingleWithParams<int>(userIdSQL, new { userId });

            return Ok(new Dictionary<string, string>
            {
                { "ok", "true" },
                {"message", "Token refreshed successfully"},
                { "token", _authHelper.CreateToken(userIdFromDB) }
            });
        }
    }
}