using DotNetApi.Data;
using DotNetApi.Dtos;
using DotNetApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserSalaryController(IConfiguration config) : ControllerBase
{
    DataContextDapper _dapper = new(config);

    [HttpGet("GetUserSalaries")]
    public IEnumerable<UserSalary> GetUserSalaries()
    {
        string sql = @"
        SELECT [UserId],
                [Salary],
                [AvgSalary]
        FROM TutorialAppSchema.UserSalary;";
        IEnumerable<UserSalary> userSalaries = _dapper.LoadData<UserSalary>(sql);

        return userSalaries;
    }

    [HttpGet("GetSingleUserSalary/{userId}")]
    public ActionResult<UserSalary> GetSingleUserSalary(int userId)
    {
        string sql = @"
            SELECT [UserId],
                [Salary],
                [AvgSalary]
            FROM TutorialAppSchema.UserSalary
            WHERE [UserId] = @UserId";
        UserSalary? userSalary = _dapper.LoadDataSingleWithParams<UserSalary>(sql, new { UserId = userId });

        if(userSalary == null)
        {
            return NotFound();
        }

        return userSalary;
    }

    [HttpPut("EditUserSalary")]
    public IActionResult EditUserSalary([FromBody] UserSalary userSalary)
    {
        string sql = @"
            UPDATE TutorialAppSchema.UserSalary
            SET [Salary] = @Salary,
                [AvgSalary] = @AvgSalary
            WHERE [UserId] = @UserId";

        if (_dapper.ExecuteSql(sql, new { userSalary.UserId, userSalary.Salary, userSalary.AvgSalary }))
        {
            return Ok("User Salary updated successfully");
        }

        return BadRequest();
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary([FromBody] UserSalaryToAddDto userSalary)
    {
        string sql = @"
            INSERT INTO TutorialAppSchema.UserSalary ([UserId], [Salary], [AvgSalary])
            VALUES (@UserId, @Salary, @AvgSalary)";

        if (_dapper.ExecuteSql(sql, new { userSalary.UserId, userSalary.Salary, userSalary.AvgSalary }))
        {
            return Ok("User Salary added successfully");
        }

        return BadRequest();
    }

    [HttpDelete("DeleteUserSalary/{userId}")]
    public IActionResult DeleteUserSalary(int userId)
    {
        string sql = @"
            DELETE FROM TutorialAppSchema.UserSalary
            WHERE [UserId] = @UserId";

        if (_dapper.ExecuteSql(sql, new { UserId = userId }))
        {
            return Ok("User Salary deleted successfully");
        }

        return BadRequest();
    }
}