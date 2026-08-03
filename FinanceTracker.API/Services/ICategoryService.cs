using FinanceTracker.API.DTOs.Category;

namespace FinanceTracker.API.Services
{
    public interface ICategoryService
    {
        Task <IEnumerable<CategoryResponseDto>> GetAllCategoriesByUserId(int userId);
        Task <CategoryResponseDto?> GetCategoryById(int id, int userId);
        Task <CategoryResponseDto> CreateCategory(CreateCategoryDto dto, int userId);
        Task <CategoryResponseDto> UpdateCategory(int id,UpdateCategoryDto dto, int userId);
        Task<bool> DeleteCategory(int id, int userId);
    }
}
