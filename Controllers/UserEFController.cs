using DotNetApi.Data;
using DotNetApi.Models;
using DotNetApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace DotNetApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController(IUserRepository userRepository) : ControllerBase
{

    readonly IUserRepository _userRepository = userRepository;
    private readonly Mapper _mapper = new(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, User>();
        }));

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _userRepository.GetUsers();
        return users;
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        return _userRepository.GetSingleUser(userId);
    }

    [HttpPut("EditUser")]
    public async Task<IActionResult> EditUser(User user)
    {
        User? userDb = _userRepository.GetSingleUser(user.UserId);

        if (userDb != null)
        {
            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;

            if (await _userRepository.SaveChangesAsync())
            {
                return Ok(userDb);
            }
            throw new Exception("Failed to update user");
        }

        throw new Exception("User not found");

    }

    [HttpPost("AddUser")]
    public async Task<IActionResult> AddUser(UserToAddDto user)
    {
        User newUser = _mapper.Map<User>(user);
        _userRepository.AddEntity(newUser);

        if (await _userRepository.SaveChangesAsync())
        {
            return Ok(newUser);
        }

        throw new Exception("Failed to add new user");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        User? userDb = _userRepository.GetSingleUser(userId);

        if (userDb != null)
        {
            _userRepository.RemoveEntity(userDb);

            if (await _userRepository.SaveChangesAsync())
            {
                return Ok(userDb);
            }
            throw new Exception("Failed to delete user");
        }

        throw new Exception("User not found");
    }

    [HttpGet("UserSalary/{userId}")]
    public ActionResult<UserSalary> GetUserSalary(int userId)
    {
        return _userRepository.GetSingleUserSalary(userId);
    }

    [HttpPost("AddUserSalary")]
    public async Task<ActionResult<UserSalary>> AddUserSalary(UserSalary userSalary)
    {
        _userRepository.AddEntity(userSalary);
        if (await _userRepository.SaveChangesAsync())
        {
            return Ok(userSalary);
        }

        return BadRequest(new { message = "Failed to add user salary" });
    }

    [HttpPut("EditUserSalary")]
    public async Task<ActionResult<UserSalary>> EditUserSalary(UserSalary userSalary)
    {
        UserSalary? userSalaryDb = _userRepository.GetSingleUserSalary(userSalary.UserId);

        if (userSalaryDb != null)
        {
            userSalaryDb.UserId = userSalary.UserId;
            userSalaryDb.Salary = userSalary.Salary;
            userSalaryDb.AvgSalary = userSalary.AvgSalary;

            if (await _userRepository.SaveChangesAsync())
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
        UserSalary? userSalaryDb = _userRepository.GetSingleUserSalary(userId);

        if (userSalaryDb != null)
        {
            _userRepository.RemoveEntity(userSalaryDb);
            if (await _userRepository.SaveChangesAsync())
            {
                return Ok("User salary deleted");
            }

            return BadRequest(new { message = "Failed to delete user salary" });
        }

        return NotFound(new { message = "User salary not found" });
    }

    [HttpGet("UserJobInfo/{userId}")]
    public async Task<ActionResult<UserJobInfo>> GetUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfo = _userRepository.GetSingleUserJobInfo(userId);
        if (userJobInfo != null)
        {
            return Ok(userJobInfo);
        }

        return NotFound(new { message = "User job info not found" });
    }

    [HttpPost("AddUserJobInfo")]
    public async Task<ActionResult<UserJobInfo>> AddUserJobInfo(UserJobInfo userJobInfo)
    {
        _userRepository.AddEntity(userJobInfo);
        if (await _userRepository.SaveChangesAsync())
        {
            return Ok(userJobInfo);
        }

        return BadRequest(new { message = "Failed to add user job info" });
    }

    [HttpPut("EditUserJobInfo")]
    public async Task<ActionResult<UserJobInfo>> EditUserJobInfo(UserJobInfo userJobInfo)
    {
        UserJobInfo? userJobInfoDb = _userRepository.GetSingleUserJobInfo(userJobInfo.UserId);
        if (userJobInfoDb != null)
        {
            userJobInfoDb.UserId = userJobInfo.UserId;
            userJobInfoDb.JobTitle = userJobInfo.JobTitle;
            userJobInfoDb.Department = userJobInfo.Department;

            if (await _userRepository.SaveChangesAsync())
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
        UserJobInfo? userJobInfoDb = _userRepository.GetSingleUserJobInfo(userId);

        if (userJobInfoDb != null)
        {
            _userRepository.RemoveEntity(userJobInfoDb);
            if (await _userRepository.SaveChangesAsync())
            {
                return Ok("User job info deleted");
            }

            return BadRequest(new { message = "Failed to delete user job info" });
        }

        return NotFound(new { message = "User job info not found" });
    }


}
