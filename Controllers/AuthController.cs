using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
        public IActionResult Register(UserForRegistrationDto userForRegistrationDto)
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

            byte[] passwordSalt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistrationDto.Password, passwordSalt);

            string sqlAddAuth = "INSERT INTO TutorialAppSchema.Auth (Email, PasswordHash, PasswordSalt) VALUES (@Email, @PasswordHash, @PasswordSalt)";
            var parameters = new
            {
                Email = userForRegistrationDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            var insertSuccess = _dapper.ExecuteSql(sqlAddAuth, parameters);

            if (!insertSuccess)
            {
                return StatusCode(500, "Internal server error");
            }

            string sqlAddUser = @"
                INSERT INTO TutorialAppSchema.Users (
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active]
                ) VALUES (
                    '" + userForRegistrationDto.FirstName + @"',
                    '" + userForRegistrationDto.LastName + @"',
                    '" + userForRegistrationDto.Email + @"',
                    '" + userForRegistrationDto.Gender + @"',
                    " + 1 + ")";
            if (!_dapper.ExecuteSql(sqlAddUser))
            {
                return StatusCode(500, "Failed to add user");
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLoginDto)
        {
            string sqlForHashAndSalt = "SELECT PasswordHash, PasswordSalt FROM TutorialAppSchema.Auth WHERE Email = @Email";
            UserForLoginConfirmationDto? userForConfirmationDto = _dapper
                .LoadDataSingleWithParams<UserForLoginConfirmationDto>(sqlForHashAndSalt, new { userForLoginDto.Email });

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
            int userId = _dapper.LoadDataSingleWithParams<int>(sqlForUserId, new { userForLoginDto.Email });



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