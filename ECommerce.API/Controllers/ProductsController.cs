using ECommerce.Application.Abstractions.Services;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.Product.Requests;
using ECommerce.Application.DTOs.ProductImage.Requests;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    public sealed class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;
        private readonly IProductImageService _productImageService;

        public ProductsController(IProductService productService, IProductImageService productImageService)
        {
            _productService = productService;
            _productImageService = productImageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
        {
            var result = await _productService.GetAllAsync(request, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _productService.GetByIdAsync(id, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        {
            var result = await _productService.CreateAsync(request, cancellationToken);
            return HandleCreatedResult(result, nameof(GetById), data => new { id = data.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
        {
            var result = await _productService.UpdateAsync(id, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _productService.DeleteAsync(id, cancellationToken);
            return HandleNoContent(result);
        }

        // Product Images Endpoints

        [HttpGet("{productId:int}/images")]
        public async Task<IActionResult> GetImages(int productId, CancellationToken cancellationToken)
        {
            var result = await _productImageService.GetByProductIdAsync(productId, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("{productId:int}/images")]
        public async Task<IActionResult> CreateImage(int productId, [FromBody] CreateProductImageRequest request, CancellationToken cancellationToken)
        {
            var result = await _productImageService.CreateAsync(productId, request, cancellationToken);
            return HandleCreatedResult(result, nameof(GetImages), data => new { productId });
        }

        [HttpPut("{productId:int}/images/{imageId:int}")]
        public async Task<IActionResult> UpdateImage(int productId, int imageId, [FromBody] UpdateProductImageRequest request, CancellationToken cancellationToken)
        {
            var result = await _productImageService.UpdateAsync(productId, imageId, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpDelete("{productId:int}/images/{imageId:int}")]
        public async Task<IActionResult> DeleteImage(int productId, int imageId, CancellationToken cancellationToken)
        {
            var result = await _productImageService.DeleteAsync(productId, imageId, cancellationToken);
            return HandleNoContent(result);
        }

        [HttpPatch("{productId:int}/images/{imageId:int}/set-main")]
        public async Task<IActionResult> SetMainImage(int productId, int imageId, CancellationToken cancellationToken)
        {
            var result = await _productImageService.SetMainImageAsync(productId, imageId, cancellationToken);
            return HandleResult(result);
        }
    }
}
