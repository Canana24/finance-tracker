using System.Net;
using System.Net.Http.Json;
using FinanceTracker.API.DTOs.Category;

namespace FinanceTracker.API.Tests.Integration
{
    public class CategoryControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly ApiTestFactory _factory;

        public CategoryControllerTests(ApiTestFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateCategory_Valid_Returns201AndCategoryIsListedInGetAll()
        {
            var client = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);
            var dto = new CreateCategoryDto { Name = "Comida", Type = "EXPENSE", Icon = "food" };

            var createResponse = await client.PostAsJsonAsync("/api/category", dto);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Regresión: si CreateCategory no setea IsActive=true, esta categoría no aparecería acá.
            var getResponse = await client.GetAsync("/api/category");
            var categories = await getResponse.Content.ReadFromJsonAsync<List<CategoryResponseDto>>();

            Assert.Contains(categories!, c => c.Name == "Comida" && c.Type == "EXPENSE");
        }

        [Fact]
        public async Task CreateCategory_InvalidType_Returns400()
        {
            // El Service lanza ArgumentException para un Type fuera de INCOME/EXPENSE,
            // mapeada por ExceptionMiddleware a 400. (Antes caía a 500; ver historial.)
            var client = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);
            var dto = new CreateCategoryDto { Name = "Rara", Type = "TRANSFER" };

            var response = await client.PostAsJsonAsync("/api/category", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCategoryById_NotFound_Returns404()
        {
            var client = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);

            var response = await client.GetAsync("/api/category/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_WithTransactions_StillSucceeds()
        {
            // Decisión de producto documentada: borrar (lógicamente) una categoría con
            // transacciones asociadas no debe fallar ni borrarlas en cascada.
            var client = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);
            var createResponse = await client.PostAsJsonAsync("/api/category",
                new CreateCategoryDto { Name = "Transporte", Type = "EXPENSE" });
            var category = await createResponse.Content.ReadFromJsonAsync<CategoryResponseDto>();

            var deleteResponse = await client.DeleteAsync($"/api/category/{category!.Id}");

            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            var getResponse = await client.GetAsync($"/api/category/{category.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode); // ya no aparece: borrado lógico
        }

        [Fact]
        public async Task GetAll_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/category");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
