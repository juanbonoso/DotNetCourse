using System.Security.Cryptography;
using System.Text;
using DotNetApi.Data;
using DotNetApi.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotNetApi.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
            _dapper = new DataContextDapper(_config);
        }

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

            byte[] passwordHash = GetPasswordHash(userForRegistrationDto.Password, passwordSalt);

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

            return Ok();
        }

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

            byte[] passwordHash = GetPasswordHash(userForLoginDto.Password, userForConfirmationDto.PasswordSalt);

            if (!passwordHash.SequenceEqual(userForConfirmationDto.PasswordHash))
            {
                return StatusCode(500, "Incorrect Password");
            }

            return Ok("Login successful");
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                        Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            );

        }

    }
}