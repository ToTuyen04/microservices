using Dapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using eCommerce.Core.RepositoryContracts;
using eCommerce.Infrastructure.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.Infrastructure.Repositories;

internal class UsersRepository : IUsersRepository
{
    private readonly DapperDbContext _dbContext;
    public UsersRepository(DapperDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<ApplicationUser?> AddUser(ApplicationUser user)
    {
        user.UserID = Guid.NewGuid();

        // SQL Query to insert user into "ApplicationUsers" table
        string query = "INSERT INTO public.\"Users\" (\"UserID\", \"Email\", \"Password\", \"PersonName\", \"Gender\") " +
                       "VALUES (@UserID, @Email, @Password, @PersonName, @Gender);";
        int rowCountAffected = await _dbContext.DbConnection.ExecuteAsync(query, user);
        if (rowCountAffected > 0)
        {
            return user;
        }
        else
        {
            return null;
        }
    }

    public async Task<ApplicationUser?> GetUserByEmailAndPassword(string? email, string? password)
    {
        //SQL Query to get a user by email and password
        string query = "SELECT * FROM public.\"Users\" " +
                       "WHERE \"Email\" = @Email AND \"Password\" = @Password;";
        var parameters = new { Email = email, Password = password };
        ApplicationUser? user = await _dbContext.DbConnection.QueryFirstOrDefaultAsync<ApplicationUser>(query, parameters);

        return user;
    }

    public async Task<ApplicationUser?> GetUserByUserID(Guid? userID)
    {
        string query = "SELECT * FROM public.\"Users\" " +
                       "WHERE \"UserID\" = @UserID;";
        var parameters = new { UserID = userID };
        using var connection = _dbContext.DbConnection;
        ApplicationUser? user = await connection.QueryFirstOrDefaultAsync<ApplicationUser>(query, parameters);
        return user;
    }
}
