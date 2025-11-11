using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllByUserAsync(string userId);
        Task<List<CategoryDto>> GetByUserAndTypeAsync(string userId, TransactionType type);
        Task<CategoryDto?> GetByIdAsync(int id, string userId);
        Task<CategoryDto> CreateAsync(CategoryDto dto, string userId);
        Task<bool> UpdateAsync(int id, CategoryDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);
        // Obtiene la cantidad de transacciones relacionadas a una categoría
        Task<int> GetRelatedTransactionsCountAsync(int categoryId, string userId);

        //Verifica si pertenece a un prestamo
        Task<(bool BelongsToLoan, string? LoanTitle)> CheckIfBelongsToLoanAsync(int categoryId, string userId);

        // Verifica si ya existe una categoría con ese nombre
        Task<bool> ExistsCategoryWithNameAsync(string title, TransactionType type, string userId, int? excludeCategoryId = null);
        // Verifica si ya existe una categoría con ese ícono
        Task<bool> ExistsCategoryWithIconAsync(string icon, TransactionType type, string userId, int? excludeCategoryId = null);
    }
}