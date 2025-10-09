using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
        public class CategoryService : ICategoryService
        {
            private readonly ApplicationDbContext _context;

            public CategoryService(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<List<CategoryDto>> GetAllAsync(string userId)
            {
                return await _context.Categories
                    .Where(c => c.UserId == userId)
                    .OrderBy(c => c.Title)
                    .Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Title = c.Title,
                        Icon = c.Icon,
                        Type = c.Type
                    })
                    .ToListAsync();
            }

            public async Task<CategoryDto?> GetByIdAsync(int categoryId, string userId)
            {
                var category = await _context.Categories
                    .Where(c => c.CategoryId == categoryId && c.UserId == userId)
                    .Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Title = c.Title,
                        Icon = c.Icon,
                        Type = c.Type
                    })
                    .FirstOrDefaultAsync();

                return category;
            }

            public async Task<CategoryDto> CreateAsync(CategoryDto categoryDto, string userId)
            {
                var category = new Category
                {
                    Title = categoryDto.Title,
                    Icon = categoryDto.Icon,
                    Type = categoryDto.Type,
                    UserId = userId
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                categoryDto.CategoryId = category.CategoryId;
                return categoryDto;
            }

            public async Task<CategoryDto> UpdateAsync(CategoryDto categoryDto, string userId)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryDto.CategoryId && c.UserId == userId);

                if (category == null)
                    throw new InvalidOperationException("Categoría no encontrada");

                category.Title = categoryDto.Title;
                category.Icon = categoryDto.Icon;
                category.Type = categoryDto.Type;

                await _context.SaveChangesAsync();

                return categoryDto;
            }

            public async Task<bool> DeleteAsync(int categoryId, string userId)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId);

                if (category == null)
                    return false;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return true;
            }

            public async Task<List<CategoryDto>> GetByTypeAsync(string userId, Domain.Enums.TransactionType type)
            {
                return await _context.Categories
                    .Where(c => c.UserId == userId && c.Type == type)
                    .OrderBy(c => c.Title)
                    .Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Title = c.Title,
                        Icon = c.Icon,
                        Type = c.Type
                    })
                    .ToListAsync();
            }
        }
    
}
