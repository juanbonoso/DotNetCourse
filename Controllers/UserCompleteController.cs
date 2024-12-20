using DotNetApi.Data;
using DotNetApi.Models;
using DotNetApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using DotNetApi.Helpers;

namespace DotNetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCompleteController(IConfiguration config) : ControllerBase
{
    private readonly DataContextDapper _dapper = new(config);
    private readonly ReusableSql _reusableSql = new(config);

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetUsers/{userId?}")]
    public async Task<IEnumerable<UserComplete>> GetUsers(int userId = 0, [FromQuery] bool? isActive = null)
    {
        string sql = "EXEC TutorialAppSchema.spUsers_Get @UserId=@UserIdParam, @Active=@ActiveParam";
        DynamicParameters parameters = new();
        parameters.Add("@UserIdParam", userId == 0 ? (int?)null : userId, System.Data.DbType.Int32);
        parameters.Add("@ActiveParam", isActive, System.Data.DbType.Boolean);

        IEnumerable<UserComplete> users = await _dapper.LoadDataWithParamsAsync<UserComplete>(sql, parameters);

        return users;
    }

    // [HttpGet("GetUsers/{userId}/{isActive}")]
    // public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    // {
    //     string sql = @"EXEC TutorialAppSchema.spUsers_Get";
    //     string parameters = "";


    //     if (userId != 0)
    //     {
    //         parameters += ", @UserId=" + userId.ToString();
    //     }

    //     if (isActive)
    //     {
    //         parameters += ", @Active=" + isActive.ToString();
    //     }

    //     sql += parameters.Substring(1);

    //     Console.WriteLine(sql);

    //     IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);

    //     return users;
    // }


    [HttpPut("UpsertUser")]
    public async Task<IActionResult> UpsertUser(UserComplete user)
    {
        if (await _reusableSql.UpsertUser(user))
        {
            return Ok("User saved successfully");
        }

        return StatusCode(500, "Internal server error");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        string sql = "TutorialAppSchema.spUser_Delete @UserId=@UserIdParam";
        DynamicParameters parameters = new();
        parameters.Add("@UserIdParam", userId, System.Data.DbType.Int32);
        var isSuccess = await _dapper.ExecuteSqlAsync(sql, parameters);
        if (!isSuccess)
        {
            return StatusCode(500, "Internal server error");
        }
        return Ok("User deleted successfully");
    }
}


