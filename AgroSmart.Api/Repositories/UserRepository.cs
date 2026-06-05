using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Data;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public Task<User?> GetByEmailAsync(string email) =>
        Set.FirstOrDefaultAsync(u => u.Email == email);
}
