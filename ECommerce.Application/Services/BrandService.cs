using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Brand.Requests;
using ECommerce.Application.DTOs.Brand.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services
{
    public sealed class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BrandService(IBrandRepository brandRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResultT<BrandResponse>> CreateAsync(CreateBrandRequest request, CancellationToken cancellationToken = default)
        {
            var nameExists = await _brandRepository.AnyAsync(x => x.Name == request.Name, cancellationToken);

            if (nameExists)
                return ResultT<BrandResponse>.Conflict(BrandMessages.BrandNameAlreadyExists);

            var brand = _mapper.Map<Brand>(request);

            await _brandRepository.AddAsync(brand, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<BrandResponse>(brand);

            return ResultT<BrandResponse>.Success(response, BrandMessages.BrandCreated);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);

            if (brand is null)
                return Result.NotFound(BrandMessages.BrandNotFound);

            _brandRepository.Delete(brand);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(BrandMessages.BrandDeleted);
        }

        public async Task<ResultT<PagedResult<BrandListResponse>>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
        {
            var totalCount = await _brandRepository.CountAsync( cancellationToken: cancellationToken);
    
            var brands = await _brandRepository.GetPagedAsync(
                orderBy: q => q.OrderBy(x => x.Name),
                skip: request.Skip,
                take: request.PageSize,
                cancellationToken: cancellationToken);

            var items = _mapper.Map<List<BrandListResponse>>(brands);

            var pagedResult = PagedResult<BrandListResponse>.Create(items, totalCount, request.PageNumber, request.PageSize);

            return ResultT<PagedResult<BrandListResponse>>.Success(pagedResult, BrandMessages.BrandsRetrieved);    
        }

        public async Task<ResultT<BrandDetailResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);

            if (brand is null)
                return ResultT<BrandDetailResponse>.NotFound(BrandMessages.BrandNotFound);

            var response = _mapper.Map<BrandDetailResponse>(brand);

            return ResultT<BrandDetailResponse>.Success(response, BrandMessages.BrandRetrieved);
        }

        public async Task<Result> UpdateAsync(int id, UpdateBrandRequest request, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);

            if (brand is null)
                return Result.NotFound(BrandMessages.BrandNotFound);

            var nameExists = await _brandRepository.AnyAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);

            if (nameExists)
                return Result.Conflict(BrandMessages.BrandNameAlreadyExists);

            _mapper.Map(request, brand);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(BrandMessages.BrandUpdated);
        }
    }
}
