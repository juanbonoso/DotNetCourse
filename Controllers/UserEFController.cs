using DotNetApi.Data;
using DotNetApi.Models;
using DotNetApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace DotNetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
    DataContextEF _entityFramework;
    IMapper _mapper;
    public UserEFController(IConfiguration config)
    {
        _entityFramework = new DataContextEF(config);
        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, User>();
        }));
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _entityFramework.Users.ToList<User>();
        return users;
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {

        User? user = _entityFramework.Users
        .Where(u => u.UserId == userId)
        .FirstOrDefault<User>();

        if (user != null)
        {
            return user;
        }

        throw new Exception("User not found");
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? userDb = _entityFramework.Users
        .Where(u => u.UserId == user.UserId)
        .FirstOrDefault<User>();

        if (userDb != null)
        {
            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;

            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok(userDb);
            }
            throw new Exception("Failed to update user");
        }

        throw new Exception("User not found");

    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        User newUser = _mapper.Map<User>(user);
        _entityFramework.Users.Add(newUser);

        if (_entityFramework.SaveChanges() > 0)
        {
            return Ok(newUser);
        }

        throw new Exception("Failed to add new user");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDb = _entityFramework.Users
        .Where(u => u.UserId == userId)
        .FirstOrDefault();

        if (userDb != null)
        {
            _entityFramework.Users.Remove(userDb);

            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok(userDb);
            }
            throw new Exception("Failed to delete user");
        }

        throw new Exception("User not found");
    }

    [HttpGet("UserSalary")]
    public async Task<IEnumerable<UserSalary>> GetUserSalaries()
    {
        IEnumerable<UserSalary> userSalaries = await _entityFramework.UserSalary.ToListAsync();
        return userSalaries;
    }

    [HttpGet("UserSalary/{userId}")]
    public async Task<ActionResult<UserSalary>> GetUserSalary(int userId)
    {
        UserSalary? userSalary = await _entityFramework.UserSalary
                                        .Where(u => u.UserId == userId)
                                        .FirstOrDefaultAsync();
        if (userSalary != null)
        {
            return userSalary;
        }

        return NotFound(new { message = "User salary not found" });
    }

    [HttpPost("AddUserSalary")]
    public async Task<ActionResult<UserSalary>> AddUserSalary(UserSalary userSalary)
    {
        _entityFramework.UserSalary.Add(userSalary);
        if (await _entityFramework.SaveChangesAsync() > 0)
        {
            return Ok(userSalary);
        }

        return BadRequest(new { message = "Failed to add user salary" });
    }

    [HttpPut("EditUserSalary")]
    public async Task<ActionResult<UserSalary>> EditUserSalary(UserSalary userSalary)
    {
        UserSalary? userSalaryDb = await _entityFramework.UserSalary
                                        .Where(u => u.UserId == userSalary.UserId)
                                        .FirstOrDefaultAsync();

        if (userSalaryDb != null)
        {
            userSalaryDb.UserId = userSalary.UserId;
            userSalaryDb.Salary = userSalary.Salary;
            userSalaryDb.AvgSalary = userSalary.AvgSalary;

            if (await _entityFramework.SaveChangesAsync() > 0)
            {
                return Ok(userSalaryDb);
            }

            return BadRequest(new { message = "Failed to update user salary" });
        }

        return NotFound(new { message = "User salary not found" });
    }

    [HttpDelete("DeleteUserSalary/{userId}")]
    public async Task<ActionResult<UserSalary>> DeleteUserSalary(int userId)
    {
        UserSalary? userSalaryDb = await _entityFramework.UserSalary
                                        .Where(u => u.UserId == userId)
                                        .FirstOrDefaultAsync();

        if (userSalaryDb != null)
        {
            _entityFramework.UserSalary.Remove(userSalaryDb);
            if (await _entityFramework.SaveChangesAsync() > 0)
            {
                return Ok("User salary deleted");
            }

            return BadRequest(new { message = "Failed to delete user salary" });
        }

        return NotFound(new { message = "User salary not found" });
    }

    [HttpGet("UserJobInfo")]
    public async Task<IEnumerable<UserJobInfo>> GetUserJobInfos()
    {
        IEnumerable<UserJobInfo> userJobInfos = await _entityFramework.UserJobInfo.ToListAsync();
        return userJobInfos;
    }

    [HttpGet("UserJobInfo/{userId}")]
    public async Task<ActionResult<UserJobInfo>> GetUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfo = await _entityFramework.UserJobInfo
                                        .Where(u => u.UserId == userId)
                                        .FirstOrDefaultAsync();
        if (userJobInfo != null)
        {
            return Ok(userJobInfo);
        }

        return NotFound(new { message = "User job info not found" });
    }

    [HttpPost("AddUserJobInfo")]
    public async Task<ActionResult<UserJobInfo>> AddUserJobInfo(UserJobInfo userJobInfo)
    {
        _entityFramework.UserJobInfo.Add(userJobInfo);
        if (await _entityFramework.SaveChangesAsync() > 0)
        {
            return Ok(userJobInfo);
        }

        return BadRequest(new { message = "Failed to add user job info" });
    }

    [HttpPut("EditUserJobInfo")]
    public async Task<ActionResult<UserJobInfo>> EditUserJobInfo(UserJobInfo userJobInfo)
    {
        UserJobInfo? userJobInfoDb = await _entityFramework.UserJobInfo
                                        .Where(u => u.UserId == userJobInfo.UserId)
                                        .FirstOrDefaultAsync();

        if (userJobInfoDb != null)
        {
            userJobInfoDb.UserId = userJobInfo.UserId;
            userJobInfoDb.JobTitle = userJobInfo.JobTitle;
            userJobInfoDb.Department = userJobInfo.Department;

            if (await _entityFramework.SaveChangesAsync() > 0)
            {
                return Ok(userJobInfoDb);
            }

            return BadRequest(new { message = "Failed to update user job info" });
        }

        return NotFound(new { message = "User job info not found" });
    }

    [HttpDelete("DeleteUserJobInfo/{userId}")]
    public async Task<ActionResult<UserJobInfo>> DeleteUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfoDb = await _entityFramework.UserJobInfo
                                        .Where(u => u.UserId == userId)
                                        .FirstOrDefaultAsync();

        if (userJobInfoDb != null)
        {
            _entityFramework.UserJobInfo.Remove(userJobInfoDb);
            if (await _entityFramework.SaveChangesAsync() > 0)
            {
                return Ok("User job info deleted");
            }

            return BadRequest(new { message = "Failed to delete user job info" });
        }

        return NotFound(new { message = "User job info not found" });
    }


}
