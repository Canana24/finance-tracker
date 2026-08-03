namespace FinanceTracker.API.DTOs.Category
{
    
        public class CreateCategoryDto
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string? Icon { get; set; }
        }

        public class UpdateCategoryDto
        {
            public string Name { get; set; } = string.Empty;
            public string? Icon { get; set; }
        }

        public class CategoryResponseDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string? Icon { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }

