using FinanceTracker.API.Data;
using FinanceTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.API.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly FinanceTrackerContext _context;

        public CategoryRepository(FinanceTrackerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesByUserId(int userId)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId && c.IsActive == true)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryById(int id, int userId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && c.IsActive == true);
        }

        public async Task<Category> CreateCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
