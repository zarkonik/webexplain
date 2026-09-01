using Microsoft.EntityFrameworkCore;
using WebExplain.Api.Data;
using WebExplain.Api.Models;

namespace WebExplain.Api.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<User> AddAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}
