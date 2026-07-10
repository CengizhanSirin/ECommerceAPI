using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Results;
using ECommerce.Application.DTOs.Address.Requests;
using ECommerce.Application.DTOs.Address.Responses;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services
{
    public sealed class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private const int TestUserId = 1;

        //TODO: JWT eklenince Auth gelince AppUserId'yi buradan alıp kullanıcının adreslerini filtreleyeceğiz.

        public AddressService(IAddressRepository addressRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResultT<AddressResponse>> CreateAsync(CreateAddressRequest request, CancellationToken cancellationToken = default)
        {
            if (request.IsDefault)
            {
                var addresses = await _addressRepository.WhereAsync(x => true, cancellationToken);

                foreach (var address in addresses)
                    address.IsDefault = false;
            }

            var newAddress = _mapper.Map<Address>(request);

            newAddress.AppUserId = TestUserId;

            await _addressRepository.AddAsync(newAddress, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<AddressResponse>(newAddress);

            return ResultT<AddressResponse>.Success(response, AddressMessages.AddressCreated);
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var address = await _addressRepository.GetByIdAsync(id, cancellationToken);

            if (address is null)
                return Result.NotFound(AddressMessages.AddressNotFound);

            _addressRepository.Delete(address);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(AddressMessages.AddressDeleted);
        }

        public async Task<ResultT<IReadOnlyList<AddressListResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var addresses = await _addressRepository.WhereAsync(x => x.AppUserId == TestUserId, cancellationToken);

            var orderedAddresses = addresses.OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.CreatedDate).ToList();

            var response = _mapper.Map<IReadOnlyList<AddressListResponse>>(orderedAddresses);

            return ResultT<IReadOnlyList<AddressListResponse>>.Success(response, AddressMessages.AddressesRetrieved);
        }

        public async Task<ResultT<AddressDetailResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var address = await _addressRepository.GetByIdAsync(id, cancellationToken);

            if (address is null)
                return ResultT<AddressDetailResponse>.NotFound(AddressMessages.AddressNotFound);

            var response = _mapper.Map<AddressDetailResponse>(address);

            return ResultT<AddressDetailResponse>.Success(response, AddressMessages.AddressRetrieved);
        }

        public async Task<Result> SetDefaultAsync(int id, CancellationToken cancellationToken = default)
        {
            var address = await _addressRepository.GetByIdAsync(id, cancellationToken);

            if (address is null)
                return Result.NotFound(AddressMessages.AddressNotFound);

            var addresses = await _addressRepository.WhereAsync(x => x.AppUserId == TestUserId, cancellationToken);

            foreach (var item in addresses)
                item.IsDefault = item.Id == id;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(AddressMessages.DefaultAddressUpdated);
        }

        public async Task<Result> UpdateAsync(int id, UpdateAddressRequest request, CancellationToken cancellationToken = default)
        {
            var address = await _addressRepository.GetByIdAsync(id, cancellationToken);

            if (address is null)
                return Result.NotFound(AddressMessages.AddressNotFound);

            if (request.IsDefault)
            {
                var addresses = await _addressRepository.WhereAsync(x => x.AppUserId == TestUserId && x.Id != id, cancellationToken);

                foreach (var item in addresses)
                    item.IsDefault = false;
            }

            _mapper.Map(request, address);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(AddressMessages.AddressUpdated);
        }
    }
}
