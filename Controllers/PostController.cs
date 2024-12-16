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

    [HttpGet("Posts")]
    public IEnumerable<Post> GetPosts()
    {
        string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Post";
        return _dapper.LoadData<Post>(sql);
    }

    [HttpGet("PostSingle/{postId}")]
    public ActionResult<Post> GetPostSingle(int postId)
    {
        string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Post
                WHERE PostId = @PostId";
        var post = _dapper.LoadDataSingleWithParams<Post>(sql, new { PostId = postId });
        if (post == null)
        {
            return NotFound();
        }
        return post;
    }

    [HttpGet("PostsByUser/{userId}")]
    public ActionResult<IEnumerable<Post>> GetPostByUser(int userId)
    {
        string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Post
                WHERE UserId = @UserId";
        var posts = _dapper.LoadDataWithParams<Post>(sql, new { UserId = userId });
        if (posts == null)
        {
            return NotFound();
        }
        return Ok(posts);
    }

    [HttpGet("MyPosts")]
    public ActionResult<IEnumerable<Post>> GetMyPost()
    {
        string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Post
                WHERE UserId = @UserId";
        var userId = this.User.FindFirst("userId")?.Value;
        var posts = _dapper.LoadDataWithParams<Post>(sql, new { UserId = userId });
        if (posts == null)
        {
            return NotFound();
        }
        return Ok(posts);
    }

    [HttpGet("PostsBySearch/{searchParam}")]
    public ActionResult<IEnumerable<Post>> GetPostBySearch(string searchParam)
    {
        string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM TutorialAppSchema.Post
                WHERE PostTitle LIKE @SearchParam OR PostContent LIKE @SearchParam";
        var posts = _dapper.LoadDataWithParams<Post>(sql, new { SearchParam = "%" + searchParam + "%" });
        if (posts == null)
        {
            return NotFound();
        }
        return Ok(posts);
    }

    [HttpPost("Post")]
    public IActionResult AddPost(PostToAddDto postToAddDto)
    {
        var userId = this.User.FindFirst("userId")?.Value;
        string sql = @"INSERT INTO TutorialAppSchema.Post (
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated]
                ) VALUES (
                    @UserId,
                    @PostTitle,
                    @PostContent,
                    GETDATE(),
                    GETDATE()
                )";
        var parameters = new
        {
            UserId = userId,
            postToAddDto.PostTitle,
            postToAddDto.PostContent
        };
        var insertSuccess = _dapper.ExecuteSql(sql, parameters);
        if (!insertSuccess)
        {
            return StatusCode(500, "Internal server error");
        }
        return Ok();
    }

    [HttpPut("Post")]
    public IActionResult EditPost(PostToEditDto postToEditDto)
    {
        var userId = this.User.FindFirst("userId")?.Value;
        string sql = @"UPDATE TutorialAppSchema.Post
                SET PostTitle = @PostTitle,
                    PostContent = @PostContent,
                    PostUpdated = GETDATE()
                WHERE PostId = @PostId AND UserId = @UserId";
        var parameters = new
        {
            UserId = userId,
            postToEditDto.PostId,
            postToEditDto.PostTitle,
            postToEditDto.PostContent
        };
        var updateSuccess = _dapper.ExecuteSql(sql, parameters);
        if (!updateSuccess)
        {
            return StatusCode(500, "Failed to edit post");
        }
        return Ok();
    }

    [HttpDelete("Post/{postId}")]
    public IActionResult DeletePost(int postId)
    {
        var userId = this.User.FindFirst("userId")?.Value;
        string sql = @"DELETE FROM TutorialAppSchema.Post
                WHERE PostId = @PostId AND UserId = @UserId";
        var parameters = new
        {
            UserId = userId,
            PostId = postId
        };
        var deleteSuccess = _dapper.ExecuteSql(sql, parameters);
        if (!deleteSuccess)
        {
            return StatusCode(500, "Failed to delete post!");
        }
        return Ok();
    }
}