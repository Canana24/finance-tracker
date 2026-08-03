using FinanceTracker.API.DTOs.Category;
using FinanceTracker.API.Models;
using FinanceTracker.API.Repositories;
using FinanceTracker.API.Services;
using Moq;

namespace FinanceTracker.API.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepository = new();
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _service = new CategoryService(_categoryRepository.Object);
        }

        // Regresión: hubo un bug real donde CreateCategory no seteaba IsActive,
        // y las categorías quedaban invisibles en los GET (que filtran por IsActive == true).
        [Theory]
        [InlineData("INCOME")]
        [InlineData("EXPENSE")]
        public async Task CreateCategory_SetsIsActiveTrue(string type)
        {
            Category? created = null;
            _categoryRepository.Setup(r => r.CreateCategory(It.IsAny<Category>()))
                .Callback<Category>(c => created = c)
                .ReturnsAsync((Category c) => c);

            var dto = new CreateCategoryDto { Name = "Comida", Type = type, Icon = "food" };

            await _service.CreateCategory(dto, userId: 10);

            Assert.NotNull(created);
            Assert.True(created!.IsActive);
        }

        [Theory]
        [InlineData("TRANSFER")]
        [InlineData("income")]
        [InlineData("")]
        public async Task CreateCategory_InvalidType_ThrowsArgumentException(string type)
        {
            var dto = new CreateCategoryDto { Name = "Comida", Type = type };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateCategory(dto, userId: 10));

            _categoryRepository.Verify(r => r.CreateCategory(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCategory_NotFound_ReturnsNull()
        {
            _categoryRepository.Setup(r => r.GetCategoryById(1, 10)).ReturnsAsync((Category?)null);

            var dto = new UpdateCategoryDto { Name = "Nuevo nombre" };
            var result = await _service.UpdateCategory(1, dto, userId: 10);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateCategory_DoesNotChangeType()
        {
            // El Type no se puede editar: UpdateCategoryDto ni siquiera tiene esa propiedad.
            // Este test confirma que el Type original de la categoría se preserva tras un update.
            var existing = new Category { Id = 1, UserId = 10, Name = "Vieja", Type = "EXPENSE", IsActive = true };
            _categoryRepository.Setup(r => r.GetCategoryById(1, 10)).ReturnsAsync(existing);

            var dto = new UpdateCategoryDto { Name = "Nueva", Icon = "new-icon" };
            var result = await _service.UpdateCategory(1, dto, userId: 10);

            Assert.NotNull(result);
            Assert.Equal("EXPENSE", result!.Type);
            Assert.Equal("Nueva", result.Name);
        }

        [Fact]
        public async Task DeleteCategory_NotFound_ReturnsFalse()
        {
            _categoryRepository.Setup(r => r.GetCategoryById(1, 10)).ReturnsAsync((Category?)null);

            var result = await _service.DeleteCategory(1, userId: 10);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteCategory_Found_SoftDeletesWithoutRemovingRecord()
        {
            var existing = new Category { Id = 1, UserId = 10, Name = "Comida", Type = "EXPENSE", IsActive = true };
            _categoryRepository.Setup(r => r.GetCategoryById(1, 10)).ReturnsAsync(existing);

            var result = await _service.DeleteCategory(1, userId: 10);

            Assert.True(result);
            Assert.False(existing.IsActive);
            Assert.NotNull(existing.DeletedAt);
            _categoryRepository.Verify(r => r.UpdateCategory(existing), Times.Once);
        }
    }
}
