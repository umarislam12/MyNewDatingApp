using System.Collections.Generic;
using System.Threading.Tasks;
using socialMedia.API.Models;

namespace socialMedia.API.Data
{
  public interface ISocialRepository
  {
    void Add<T>(T entity) where T : class;
    void Delete<T>(T entity) where T : class;
    Task<bool> SaveAll();
    Task<IEnumerable<User>> GetUsers();
    Task<User> GetUser(int id);
        Task<Photo> GetPhoto(int id);
  }
}