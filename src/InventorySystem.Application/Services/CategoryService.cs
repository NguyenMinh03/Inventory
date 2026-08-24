using AutoMapper;
using InventorySystem.Application.DTOs;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Exceptions;
using InventorySystem.Domain.Interfaces;

namespace InventorySystem.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync("ParentCategory");
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, "ParentCategory");
        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new DomainException("Category name is required.");

        if (dto.ParentCategoryId is not null && await _unitOfWork.Categories.GetByIdAsync(dto.ParentCategoryId.Value) is null)
            throw new KeyNotFoundException($"Parent category {dto.ParentCategoryId} was not found.");

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId,
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} was not found.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new DomainException("Category name is required.");

        if (dto.ParentCategoryId == id)
            throw new DomainException("A category cannot be its own parent.");

        if (dto.ParentCategoryId is not null && await _unitOfWork.Categories.GetByIdAsync(dto.ParentCategoryId.Value) is null)
            throw new KeyNotFoundException($"Parent category {dto.ParentCategoryId} was not found.");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ParentCategoryId = dto.ParentCategoryId;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} was not found.");

        var products = await _unitOfWork.Products.GetAllAsync();
        if (products.Any(p => p.CategoryId == id))
            throw new DomainException("Cannot delete a category that still has products assigned to it.");

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
