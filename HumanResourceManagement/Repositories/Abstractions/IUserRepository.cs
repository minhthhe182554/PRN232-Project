using HumanResourceManagement.Models;
namespace HumanResourceManagement.Repositories.Abstractions;

public interface IUserRepository 
{
    Task<User> GetById(int id);
    Task<List<User>> GetAll(); 
}