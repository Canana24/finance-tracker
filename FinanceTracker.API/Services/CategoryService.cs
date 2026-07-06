using FinanceTracker.API.DTOs.Category;
using FinanceTracker.API.Models;
using FinanceTracker.API.Repositories;
namespace FinanceTracker.API.Services

{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesByUserId(int userId)
        {
            var categories = await _categoryRepository.GetAllCategoriesByUserId(userId);
            return categories.Select(MapToResponseDto);
        }

        public async Task<CategoryResponseDto?> GetCategoryById(int id, int userId)
        {
            var category = await _categoryRepository.GetCategoryById(id, userId);
            return category == null ? null : MapToResponseDto(category);
        }

        public async Task<CategoryResponseDto> CreateCategory(CreateCategoryDto dto, int userId)
        {
            if (dto.Type != "INCOME" && dto.Type != "EXPENSE")
                throw new ArgumentException("El tipo debe ser INCOME o EXPENSE.");

            var category = new Category
            {
                UserId = userId,
                Name = dto.Name,
                Type = dto.Type,
                Icon = dto.Icon,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            var created = await _categoryRepository.CreateCategory(category);
            return MapToResponseDto(created);
        }

        public async Task<CategoryResponseDto?> UpdateCategory(int id, UpdateCategoryDto dto, int userId)
        {
            var category = await _categoryRepository.GetCategoryById(id, userId);
            if (category == null)
                return null;
            category.Name = dto.Name;
            category.Icon = dto.Icon;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = userId;
            await _categoryRepository.UpdateCategory(category);
            return MapToResponseDto(category);
        }

        public async Task<bool> DeleteCategory(int id, int userId)
        {
            var category = await _categoryRepository.GetCategoryById(id, userId);
            if (category == null)
                return false;
            category.IsActive = false;
            category.DeletedAt = DateTime.UtcNow;
            category.DeletedBy = userId;

            await _categoryRepository.UpdateCategory(category);
            return true;
        }

        private static CategoryResponseDto MapToResponseDto(Category category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                Icon = category.Icon,
                CreatedAt = category.CreatedAt
            };
        }
    }
}
