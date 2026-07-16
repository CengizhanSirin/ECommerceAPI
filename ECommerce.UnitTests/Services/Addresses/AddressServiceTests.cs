using AutoMapper;
using ECommerce.Application.Abstractions.Persistence;
using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.DTOs.Address.Requests;
using ECommerce.Application.DTOs.Address.Responses;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace ECommerce.UnitTests.Services.Addresses
{
    public sealed class AddressServiceTests
    {
        private const int CurrentUserId = 1881;

        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IAddressRepository> _addressRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly AddressService _addressService;

        public AddressServiceTests()
        {
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _addressRepositoryMock = new Mock<IAddressRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            _currentUserServiceMock.SetupGet(service => service.UserId).Returns(CurrentUserId);
            _addressService = new AddressService(_addressRepositoryMock.Object, _unitOfWorkMock.Object, _mapperMock.Object, _currentUserServiceMock.Object);
        }

        // =========================================================
        // CREATE
        // =========================================================

        [Fact]
        public async Task CreateAsync_Should_SetAddressAsDefault_When_UserHasNoAddress()
        {
            // Arrange
            var request = CreateValidCreateRequest(isDefault: false);

            var newAddress = CreateAddress(id: 0, userId: 0, isDefault: false);

            var response = CreateAddressResponse(newAddress, isDefault: true);


            _addressRepositoryMock.Setup(repository => repository.CountAsync(It.IsAny<Expression<Func<Address, bool>>?>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);

            _mapperMock.Setup(mapper => mapper.Map<Address>(request)).Returns(newAddress);

            _mapperMock.Setup(mapper => mapper.Map<AddressResponse>(newAddress)).Returns(response);

            // Act
            var result = await _addressService.CreateAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressCreated, result.Message);
            Assert.NotNull(result.Data);
            Assert.Same(response, result.Data);

            Assert.Equal(CurrentUserId, newAddress.AppUserId);
            Assert.True(newAddress.IsDefault);

            _addressRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _addressRepositoryMock.Verify(repository => repository.AddAsync(newAddress, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_Should_ClearExistingDefaults_When_NewAddressIsDefault()
        {
            // Arrange
            var request = CreateValidCreateRequest(isDefault: true);

            var existingAddresses = new List<Address>
            {
                CreateAddress(id: 1, isDefault: true),
                CreateAddress(id: 2, isDefault: false)
            };

            var newAddress = CreateAddress(id: 0, userId: 0, isDefault: false);

            var response = CreateAddressResponse(newAddress, isDefault: true);

            _addressRepositoryMock.Setup(repository => repository.CountAsync(It.IsAny<Expression<Func<Address, bool>>?>(), It.IsAny<CancellationToken>())).ReturnsAsync(existingAddresses.Count);

            _addressRepositoryMock.Setup(repository => repository.WhereAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(existingAddresses);

            _mapperMock.Setup(mapper => mapper.Map<Address>(request)).Returns(newAddress);

            _mapperMock.Setup(mapper => mapper.Map<AddressResponse>(newAddress)).Returns(response);

            // Act
            var result = await _addressService.CreateAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressCreated, result.Message);

            Assert.Equal(CurrentUserId, newAddress.AppUserId);
            Assert.True(newAddress.IsDefault);

            Assert.All(existingAddresses, address => Assert.False(address.IsDefault));

            _addressRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>()), Times.Once());

            _addressRepositoryMock.Verify(repository => repository.AddAsync(newAddress, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_Should_CreateNonDefaultAddress_When_UserHasAddressesAndRequestIsNotDefault()
        {
            // Arrange
            var request = CreateValidCreateRequest(isDefault: false);

            var newAddress = CreateAddress(id: 0, userId: 0, isDefault: false);

            var response = CreateAddressResponse(newAddress, isDefault: false);

            _addressRepositoryMock.Setup(repository => repository.CountAsync(It.IsAny<Expression<Func<Address, bool>>?>(), It.IsAny<CancellationToken>())).ReturnsAsync(2);

            _mapperMock.Setup(mapper => mapper.Map<Address>(request)).Returns(newAddress);

            _mapperMock.Setup(mapper => mapper.Map<AddressResponse>(newAddress)).Returns(response);

            // Act
            var result = await _addressService.CreateAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressCreated, result.Message);

            Assert.Equal(CurrentUserId, newAddress.AppUserId);
            Assert.False(newAddress.IsDefault);

            _addressRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _addressRepositoryMock.Verify(repository => repository.AddAsync(newAddress, It.IsAny<CancellationToken>()), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }


        // =========================================================
        // DELETE
        // =========================================================


        [Fact]
        public async Task DeleteAsync_Should_ReturnNotFound_When_AddressDoesNotExist()
        {
            // Arrange
            const int addressId = 999;

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((Address?)null);

            // Act
            var result = await _addressService.DeleteAsync(addressId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressNotFound, result.Message);

            _addressRepositoryMock.Verify(repository => repository.Delete(It.IsAny<Address>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        }

        [Fact]
        public async Task DeleteAsync_Should_DeleteAddress_When_AddressExists()
        {
            // Arrange
            var address = CreateAddress();
            address.IsDefault = false;

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            // Act
            var result = await _addressService.DeleteAsync(address.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressDeleted, result.Message);

            _addressRepositoryMock.Verify(repository => repository.Delete(address), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_Should_SetAnotherAddressAsDefault_When_DefaultAddressIsDeleted()
        {
            // Arrange
            var defaultAddress = CreateAddress(id: 1);
            defaultAddress.IsDefault = true;

            var replacementAddress = CreateAddress(id: 2);
            replacementAddress.IsDefault = false;

            _addressRepositoryMock.SetupSequence(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(defaultAddress)
                .ReturnsAsync(replacementAddress);

            // Act
            var result = await _addressService.DeleteAsync(defaultAddress.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressDeleted, result.Message);

            Assert.False(defaultAddress.IsDefault);
            Assert.True(replacementAddress.IsDefault);

            _addressRepositoryMock.Verify(repository => repository.Delete(defaultAddress), Times.Once());

            _addressRepositoryMock.Verify(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_Should_DeleteSuccessfully_When_DefaultAddressIsTheOnlyAddress()
        {
            // Arrange
            var defaultAddress = CreateAddress(id: 1);

            defaultAddress.IsDefault = true;

            _addressRepositoryMock.SetupSequence(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(defaultAddress)
                .ReturnsAsync((Address?)null);

            // Act
            var result = await _addressService.DeleteAsync(defaultAddress.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressDeleted, result.Message);

            Assert.False(defaultAddress.IsDefault);

            _addressRepositoryMock.Verify(repository => repository.Delete(defaultAddress), Times.Once());

            _addressRepositoryMock.Verify(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }


        // =========================================================
        // GET ALL
        // =========================================================


        [Fact]
        public async Task GetAllAsync_Should_ReturnAddressesOrderedByDefaultAndCreatedDate()
        {
            // Arrange
            var oldNonDefaultAddress = CreateAddress(id: 1, isDefault: false, createdDate: new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));

            var defaultAddress = CreateAddress(id: 2, isDefault: true, createdDate: new DateTimeOffset(2026, 1, 2, 10, 0, 0, TimeSpan.Zero));

            var newNonDefaultAddress = CreateAddress(id: 3, isDefault: false, createdDate: new DateTimeOffset(2026, 1, 3, 10, 0, 0, TimeSpan.Zero));

            var addresses = new List<Address>
            {
               oldNonDefaultAddress,
               defaultAddress,
               newNonDefaultAddress
            };


            IReadOnlyList<AddressListResponse> responses = new List<AddressListResponse>
            {
                CreateAddressListResponse(defaultAddress),
                CreateAddressListResponse(newNonDefaultAddress),
                CreateAddressListResponse(oldNonDefaultAddress)
            };

            _addressRepositoryMock.Setup(repository => repository.WhereAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(addresses);

            _mapperMock.Setup(mapper => mapper.Map<IReadOnlyList<AddressListResponse>>(It.Is<List<Address>>(orderedAddresses =>
            orderedAddresses.Select(address => address.Id).SequenceEqual(new[] { 2, 3, 1 })))).Returns(responses);

            // Act
            var result = await _addressService.GetAllAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressesRetrieved, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(responses, result.Data);

            _mapperMock.Verify(mapper => mapper.Map<IReadOnlyList<AddressListResponse>>(It.Is<List<Address>>(orderedAddresses =>
                orderedAddresses.Select(address => address.Id).SequenceEqual(new[] { 2, 3, 1 }))), Times.Once());
        }


        // =========================================================
        // GET BY ID
        // =========================================================


        [Fact]
        public async Task GetByIdAsync_Should_ReturnNotFound_When_AddressDoesNotExist()
        {
            // Arrange
            const int addressId = 999;

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((Address?)null);

            // Act
            var result = await _addressService.GetByIdAsync(addressId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressNotFound, result.Message);
            Assert.Null(result.Data);

            _mapperMock.Verify(mapper => mapper.Map<AddressDetailResponse>(It.IsAny<Address>()), Times.Never());
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnAddress_When_AddressExists()
        {
            // Arrange
            const int addressId = 1;

            var address = CreateAddress(id: addressId, userId: CurrentUserId, isDefault: true);

            var response = CreateAddressDetailResponse(address);

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            _mapperMock.Setup(mapper => mapper.Map<AddressDetailResponse>(address)).Returns(response);

            // Act
            var result = await _addressService.GetByIdAsync(addressId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressRetrieved, result.Message);

            Assert.NotNull(result.Data);
            Assert.Same(response, result.Data);

            _mapperMock.Verify(mapper => mapper.Map<AddressDetailResponse>(address), Times.Once());
        }


        // =========================================================
        // SET DEFAULT
        // =========================================================


        [Fact]
        public async Task SetDefaultAsync_Should_ReturnNotFound_When_AddressDoesNotExist()
        {
            // Arrange
            const int addressId = 999;

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((Address?)null);

            // Act
            var result = await _addressService.SetDefaultAsync(addressId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressNotFound, result.Message);

            _addressRepositoryMock.Verify(repository => repository.WhereAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task SetDefaultAsync_Should_SetSelectedAddressAsDefault()
        {
            // Arrange
            const int selectedAddressId = 2;

            var firstAddress = CreateAddress(id: 1, isDefault: true);

            var selectedAddress = CreateAddress(id: selectedAddressId, isDefault: false);

            var thirdAddress = CreateAddress(id: 3, isDefault: false);

            var addresses = new List<Address>
            {
                firstAddress,
                selectedAddress,
                thirdAddress
            };


            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(selectedAddress);

            _addressRepositoryMock.Setup(repository => repository.WhereAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(addresses);

            // Act
            var result = await _addressService.SetDefaultAsync(selectedAddressId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.DefaultAddressUpdated, result.Message);

            Assert.False(firstAddress.IsDefault);
            Assert.True(selectedAddress.IsDefault);
            Assert.False(thirdAddress.IsDefault);

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }


        // =========================================================
        // UPDATE
        // =========================================================


        [Fact]
        public async Task UpdateAsync_Should_ReturnNotFound_When_AddressDoesNotExist()
        {
            // Arrange
            const int addressId = 999;

            var request = CreateValidUpdateRequest();

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync((Address?)null);

            // Act
            var result = await _addressService.UpdateAsync(addressId, request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressNotFound, result.Message);

            _mapperMock.Verify(mapper => mapper.Map<UpdateAddressRequest, Address>(It.IsAny<UpdateAddressRequest>(), It.IsAny<Address>()), Times.Never());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task UpdateAsync_Should_UpdateAddress_When_AddressExists()
        {
            // Arrange
            const int addressId = 1;

            var address = CreateAddress(id: addressId, userId: CurrentUserId);

            var request = CreateValidUpdateRequest();

            _addressRepositoryMock.Setup(repository => repository.FirstOrDefaultAsync(It.IsAny<Expression<Func<Address, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(address);

            _mapperMock.Setup(mapper => mapper.Map<UpdateAddressRequest, Address>(request, address)).Returns(address);

            // Act
            var result = await _addressService.UpdateAsync(addressId, request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AddressMessages.AddressUpdated, result.Message);

            _mapperMock.Verify(mapper => mapper.Map<UpdateAddressRequest, Address>(request, address), Times.Once());

            _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }


        // =========================================================
        // TEST DATA HELPERS
        // =========================================================


        private static CreateAddressRequest CreateValidCreateRequest(bool isDefault = false)
        {
            return new CreateAddressRequest(
                Title: "Ev",
                FirstName: "Cengizhan",
                LastName: "Şirin",
                Phone: "05555555555",
                City: "İstanbul",
                District: "Kadıköy",
                Neighborhood: "Caferağa",
                Street: "Moda Caddesi No: 1",
                PostalCode: "34710",
                IsDefault: isDefault);
        }

        private static UpdateAddressRequest CreateValidUpdateRequest()
        {
            return new UpdateAddressRequest(
                Title: "İş",
                FirstName: "Cengizhan",
                LastName: "Şirin",
                Phone: "05555555555",
                City: "İstanbul",
                District: "Şişli",
                Neighborhood: "Esentepe",
                Street: "Büyükdere Caddesi No: 10",
                PostalCode: "34394");
        }

        private static Address CreateAddress(int id = 1, int userId = CurrentUserId, bool isDefault = false, DateTimeOffset? createdDate = null)
        {
            return new Address
            {
                Id = id,
                AppUserId = userId,
                Title = "Ev",
                FirstName = "Cengizhan",
                LastName = "Şirin",
                Phone = "05555555555",
                City = "İstanbul",
                District = "Kadıköy",
                Neighborhood = "Caferağa",
                Street = "Moda Caddesi No: 1",
                PostalCode = "34710",
                IsDefault = isDefault,
                CreatedDate = createdDate ?? DateTimeOffset.UtcNow
            };
        }

        private static AddressResponse CreateAddressResponse(Address address, bool? isDefault = null)
        {
            return new AddressResponse
            {
                Id = address.Id,
                Title = address.Title,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Phone = address.Phone,
                City = address.City,
                District = address.District,
                Neighborhood = address.Neighborhood,
                Street = address.Street,
                PostalCode = address.PostalCode,
                IsDefault = isDefault ?? address.IsDefault
            };
        }

        private static AddressListResponse CreateAddressListResponse(Address address)
        {
            return new AddressListResponse
            {
                Id = address.Id,
                Title = address.Title,
                City = address.City,
                District = address.District,
                IsDefault = address.IsDefault
            };
        }

        private static AddressDetailResponse CreateAddressDetailResponse(Address address)
        {
            return new AddressDetailResponse
            {
                Id = address.Id,
                Title = address.Title,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Phone = address.Phone,
                City = address.City,
                District = address.District,
                Neighborhood = address.Neighborhood,
                Street = address.Street,
                PostalCode = address.PostalCode,
                IsDefault = address.IsDefault,
                CreatedDate = address.CreatedDate,
                UpdatedDate = address.UpdatedDate
            };
        }

    }
}
