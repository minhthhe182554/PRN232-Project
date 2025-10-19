using HumanResourceManagement.Models;
using HumanResourceManagement.Repositories.Abstractions;

namespace HumanResourceManagement.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    public Task<List<User>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<User> GetById(int id)
    {
        throw new NotImplementedException();
    }
}