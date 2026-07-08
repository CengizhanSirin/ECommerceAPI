using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Category.Requests;
using ECommerce.Application.DTOs.Category.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services
{
    public sealed class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResultT<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var nameExists = await _categoryRepository.AnyAsync(x => x.Name == request.Name, cancellationToken);

            if (nameExists)
                return ResultT<CategoryResponse>.Conflict(CategoryMessages.CategoryNameAlreadyExists);

            var category = _mapper.Map<Category>(request);

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ResultT<CategoryResponse>.Success( _mapper.Map<CategoryResponse>(category), CategoryMessages.CategoryCreated);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

            if (category is null)
                return Result.NotFound(CategoryMessages.CategoryNotFound);

            _categoryRepository.Delete(category);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(CategoryMessages.CategoryDeleted);
        }

        public async Task<ResultT<PagedResult<CategoryListResponse>>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var totalCount = await _categoryRepository.CountAsync(cancellationToken: cancellationToken);

            var categories = await _categoryRepository.GetPagedAsync(orderBy: c => c.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name),
                           skip: request.Skip,
                           take: request.PageSize,
                           cancellationToken: cancellationToken);

            var items = _mapper.Map<List<CategoryListResponse>>(categories);

            return ResultT<PagedResult<CategoryListResponse>>.Success(
                PagedResult<CategoryListResponse>.Create(items, totalCount, request.PageNumber, request.PageSize), CategoryMessages.CategoryListRetrieved);
        }

        public async Task<ResultT<CategoryDetailResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

            if (category is null)
            return ResultT<CategoryDetailResponse>.NotFound(CategoryMessages.CategoryNotFound);

            return ResultT<CategoryDetailResponse>.Success(_mapper.Map<CategoryDetailResponse>(category), CategoryMessages.CategoryRetrieved);
        }

        public async Task<Result> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);

            if (category is null)
                return Result.NotFound(CategoryMessages.CategoryNotFound);

            var nameExists = await _categoryRepository
                .AnyAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);

            if (nameExists)
                return Result.Conflict(CategoryMessages.CategoryNameAlreadyExists);

            _mapper.Map(request, category);   
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(CategoryMessages.CategoryUpdated);
        }
    }
}
