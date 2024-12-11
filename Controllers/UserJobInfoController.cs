

using DotNetApi.Data;
using DotNetApi.Dtos;
using DotNetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserJobInfoController(IConfiguration config) : ControllerBase
{
    DataContextDapper _dapper = new(config);

    [HttpGet("GetUserJobInfo")]
    public IEnumerable<UserJobInfo> GetUserJobInfo()
    {
        string sql = @"
        SELECT [UserId],
                [JobTitle],
                [Department]
        FROM TutorialAppSchema.UserJobInfo;";
        IEnumerable<UserJobInfo> userJobInfo = _dapper.LoadData<UserJobInfo>(sql);

        return userJobInfo;
    }

    [HttpGet("GetSingleUserJobInfo/{userId}")]
    public ActionResult<UserJobInfo> GetSingleUserJobInfo(int userId)
    {
        string sql = @"
            SELECT [UserId],
                [JobTitle],
                [Department]
            FROM TutorialAppSchema.UserJobInfo
            WHERE [UserId] = @UserId";
        UserJobInfo? userJobInfo = _dapper.LoadDataSingleWithParams<UserJobInfo>(sql, new { UserId = userId });

        if(userJobInfo == null)
        {
            return NotFound();
        }

        return userJobInfo;
    }

    [HttpPut("EditUserJobInfo")]
    public IActionResult EditUserJobInfo([FromBody] UserJobInfo userJobInfo)
    {
        string sql = @"
            UPDATE TutorialAppSchema.UserJobInfo
            SET [JobTitle] = @JobTitle,
                [Department] = @Department
            WHERE [UserId] = @UserId";

        if (_dapper.ExecuteSql(sql, new { userJobInfo.UserId, userJobInfo.JobTitle, userJobInfo.Department }))
        {
            return Ok("User Job Info updated successfully");
        }

        return BadRequest();
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo([FromBody] UserJobInfoToAddDto userJobInfo)
    {
        string sql = @"
            INSERT INTO TutorialAppSchema.UserJobInfo ([UserId], [JobTitle], [Department])
            VALUES (@UserId, @JobTitle, @Department)";

        if (_dapper.ExecuteSql(sql, new { userJobInfo.UserId, userJobInfo.JobTitle, userJobInfo.Department }))
        {
            return Ok("User Job Info added successfully");
        }

        return BadRequest();
    }

    [HttpDelete("DeleteUserJobInfo/{userId}")]
    public IActionResult DeleteUserJobInfo(int userId)
    {
        string sql = @"
            DELETE FROM TutorialAppSchema.UserJobInfo
            WHERE [UserId] = @UserId";

        if (_dapper.ExecuteSql(sql, new { UserId = userId }))
        {
            return Ok("User Job Info deleted successfully");
        }

        return BadRequest();
    }
}