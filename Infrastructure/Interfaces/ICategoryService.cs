using MisFinanzas.Domain.DTOs;


namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync(string userId);
        Task<CategoryDto?> GetByIdAsync(int categoryId, string userId);
        Task<CategoryDto> CreateAsync(CategoryDto categoryDto, string userId);
        Task<CategoryDto> UpdateAsync(CategoryDto categoryDto, string userId);
        Task<bool> DeleteAsync(int categoryId, string userId);
        Task<List<CategoryDto>> GetByTypeAsync(string userId, Domain.Enums.TransactionType type);
    }
}
