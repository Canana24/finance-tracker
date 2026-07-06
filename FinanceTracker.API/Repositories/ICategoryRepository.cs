using FinanceTracker.API.Models;
namespace FinanceTracker.API.Repositories
{
    public interface ICategoryRepository
    {
        Task <IEnumerable<Category>> GetAllCategoriesByUserId(int userId);
        Task<Category?> GetCategoryById(int id, int userId);
        Task <Category> CreateCategory(Category category);
        Task UpdateCategory (Category category);
    }
}
