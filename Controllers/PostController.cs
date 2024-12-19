using System.Data;
using Dapper;
using DotNetApi.Data;
using DotNetApi.Dtos;
using DotNetApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PostController(IConfiguration config) : ControllerBase
{
    private readonly DataContextDapper _dapper = new(config);

    [HttpGet("Posts/{postId}/{userId}")]
    public async Task<IActionResult> GetPosts(int postId = 0, int userId = 0, [FromQuery] string? searchParam = null)
    {
        string sql = @"EXEC TutorialAppSchema.spPosts_Get @PostId=@PostIdParam, @UserId=@UserIdParam, @SearchValue=@SearchValueParam";
        var parameters = new DynamicParameters();
        parameters.Add("@PostIdParam", postId == 0 ? (int?)null : postId, DbType.Int32);
        parameters.Add("@UserIdParam", userId == 0 ? (int?)null : userId, DbType.Int32);
        parameters.Add("@SearchValueParam", string.IsNullOrEmpty(searchParam) ||
                        string.Equals(searchParam, "none", StringComparison.OrdinalIgnoreCase)
                        ? null : searchParam, DbType.String);

        IEnumerable<Post> posts = await _dapper.LoadDataWithParamsAsync<Post>(sql, parameters);
        return Ok(posts);
    }

    [HttpGet("MyPosts")]
    public async Task<ActionResult<IEnumerable<Post>>> GetMyPost()
    {
        var userId = this.User.FindFirst("userId")?.Value;
        string sql = "EXEC TutorialAppSchema.spPosts_Get @UserId=@UserIdParam";
        var parameters = new DynamicParameters();
        parameters.Add("@UserIdParam", userId, DbType.Int32);
        var posts = await _dapper.LoadDataWithParamsAsync<Post>(sql, parameters);
        if (posts == null)
        {
            return NotFound();
        }
        return Ok(posts);
    }

    [HttpPut("UpsertPost")]
    public async Task<IActionResult> UpsertPost(Post postToUsert)
    {
        var userId = this.User.FindFirst("userId")?.Value;
        bool isUpdatePost = postToUsert.PostId > 0;
        string sql = @"EXEC TutorialAppSchema.spPosts_Upsert @UserId=@UserIdParam,
                        @PostTitle=@PostTitleParam,
                        @PostContent=@PostContentParam, 
                        @PostId=@PostIdParam";
        var parameters = new DynamicParameters();
        parameters.Add("@UserIdParam", userId, DbType.Int32);
        parameters.Add("@PostTitleParam", postToUsert.PostTitle, DbType.String);
        parameters.Add("@PostContentParam", postToUsert.PostContent, DbType.String);
        parameters.Add("@PostIdParam", isUpdatePost ? postToUsert.PostId : null, DbType.Int32);


        var isUpsertSuccess = await _dapper.ExecuteSqlAsync(sql, parameters);
        if (!isUpsertSuccess)
        {
            return StatusCode(500, "Internal server error");
        }

        return Ok(isUpdatePost ? "Post updated successfully" : "Post added successfully");
    }

    [HttpDelete("Post/{postId}")]
    public async Task<IActionResult> DeletePost(int postId)
    {
        var userId = this.User.FindFirst("userId")?.Value;
        string sql = @"EXEC TutorialAppSchema.spPost_Delete @UserId=@UserIdParam, @PostId=@PostIdParam";
        var parameters = new DynamicParameters();
        parameters.Add("@UserIdParam", userId, DbType.Int32);
        parameters.Add("@PostIdParam", postId, DbType.Int32);
        var deleteSuccess = await _dapper.ExecuteSqlAsync(sql, parameters);
        if (!deleteSuccess)
        {
            return StatusCode(500, "Failed to delete post!");
        }
        return Ok("Post deleted successfully");
    }
}