using Dapper;
using DotNetApi.Data;
using DotNetApi.Models;

namespace DotNetApi.Helpers;

public class ReusableSql(IConfiguration config)
{
    private readonly DataContextDapper _dapper = new(config);

    public async Task<bool> UpsertUser(UserComplete user)
    {
        string sql = @"TutorialAppSchema.spUser_Upsert 
                            @FirstName=@FirstNameParam, 
                            @LastName=@LastNameParam, 
                            @Email=@EmailParam, 
                            @Gender=@GenderParam, 
                            @JobTitle=@JobTitleParam, 
                            @Department=@DepartmentParam, 
                            @Salary=@SalaryParam, 
                            @Active=@ActiveParam, 
                            @UserId=@UserIdParam";
        DynamicParameters parameters = new();
        parameters.Add("@FirstNameParam", user.FirstName, System.Data.DbType.String);
        parameters.Add("@LastNameParam", user.LastName, System.Data.DbType.String);
        parameters.Add("@EmailParam", user.Email, System.Data.DbType.String);
        parameters.Add("@GenderParam", user.Gender, System.Data.DbType.String);
        parameters.Add("@JobTitleParam", user.JobTitle, System.Data.DbType.String);
        parameters.Add("@DepartmentParam", user.Department, System.Data.DbType.String);
        parameters.Add("@SalaryParam", user.Salary, System.Data.DbType.Decimal);
        parameters.Add("@ActiveParam", user.Active, System.Data.DbType.Boolean);
        parameters.Add("@UserIdParam", user.UserId, System.Data.DbType.Int32);

        return await _dapper.ExecuteSqlAsync(sql, parameters);
    }
}