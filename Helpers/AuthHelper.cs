using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotNetApi.Data;
using DotNetApi.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotNetApi.Helpers;

public class AuthHelper(IConfiguration config)
{

    private readonly IConfiguration _config = config;
    private readonly DataContextDapper _dapper = new(config);

    public byte[] GetPasswordHash(string password, byte[] passwordSalt)
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

    public string CreateToken(int userId)
    {
        Claim[] claims =
        [
            new Claim("userId", userId.ToString())
        ];

        string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;
        SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(tokenKeyString ?? ""));

        SigningCredentials credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512Signature);
        SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = credentials
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(descriptor);

        return tokenHandler.WriteToken(token);
    }

    public async Task<bool> SetPassword(UserForLoginDto userForLoginDto)
    {
        byte[] passwordSalt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetNonZeroBytes(passwordSalt);
        }

        byte[] passwordHash = GetPasswordHash(userForLoginDto.Password, passwordSalt);

        string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert @Email=@EmailParam, @PasswordHash=@PasswordHashParam, @PasswordSalt=@PasswordSaltParam";
        DynamicParameters parameters = new();
        parameters.Add("EmailParam", userForLoginDto.Email, System.Data.DbType.String);
        parameters.Add("PasswordHashParam", passwordHash, System.Data.DbType.Binary);
        parameters.Add("PasswordSaltParam", passwordSalt, System.Data.DbType.Binary);

        return await _dapper.ExecuteSqlWithParamsAsync(sqlAddAuth, parameters);
        // List<SqlParameter> parameters =
        // [
        //     new SqlParameter("@EmailParam", System.Data.SqlDbType.NVarChar) { Value = userForLoginDto.Email },
        //         new SqlParameter("@PasswordHashParam", System.Data.SqlDbType.VarBinary) { Value = passwordHash },
        //         new SqlParameter("@PasswordSaltParam", System.Data.SqlDbType.VarBinary) { Value = passwordSalt }
        // ];
        // return await _dapper.ExecuteSqlWithParamsAsync(sqlAddAuth, parameters);
    }

}
