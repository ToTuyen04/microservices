using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entity;
using DataAccessLayer.IRepository;
using eCommerceSolution.ProductService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using FluentValidation.Results;
using System.Linq.Expressions;


namespace BusinessLogicLayer.Services;
internal class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly IValidator<ProductAddRequest> _proAddRequestValidator;
    private readonly IValidator<ProductUpdateRequest> _proUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IRabbitMQPublisher _rabbitMQPublisher;
    public ProductService(IProductRepository productRepo, IValidator<ProductAddRequest> addValidator, IValidator<ProductUpdateRequest> updateValidator, IMapper mapper, IRabbitMQPublisher rabbitMQPublisher)
    {
        _productRepo = productRepo;
        _proAddRequestValidator = addValidator;
        _proUpdateRequestValidator = updateValidator;
        _mapper = mapper;
        _rabbitMQPublisher = rabbitMQPublisher;
    }
    public async Task<ProductResponse?> AddProduct(ProductAddRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(ProductAddRequest));
        }

        //Validate the product by using Fluent Validation
        ValidationResult validationResult = await _proAddRequestValidator.ValidateAsync(request);

        //Check the validation result
        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errors);
        }

        //Attempt to add product
        Product pInput = _mapper.Map<Product>(request);
        Product? pAdded = await _productRepo.AddProduct(pInput);
        if (pAdded == null)
            return null;

        return _mapper.Map<ProductResponse>(pAdded);

    }

    public async Task<bool> DeleteProduct(Guid productId)
    {
        Product? p = await _productRepo.GetProductByCondition(p => p.ProductID == productId);
        if (p == null)
            return false;
        bool isDeleted = await _productRepo.DeleteProduct(p.ProductID);

        if (isDeleted)
        {
            //string routingKey = "product.delete";
            Dictionary<string, object> headers = new Dictionary<string, object>(){
                {"event", "product.delete" },
                {"rowCount", 1 }
            };
            ProductDeletionMessage productDeletionMessage = new ProductDeletionMessage(p.ProductID, p.ProductName);
            _rabbitMQPublisher.Publish<ProductDeletionMessage>(headers, productDeletionMessage);
        }

        return isDeleted;
    }

    public async Task<ProductResponse?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        Product? p = await _productRepo.GetProductByCondition(conditionExpression);
        if (p == null)
            return null;
        return _mapper.Map<ProductResponse?>(p);
    }

    public async Task<List<ProductResponse?>> GetProducts()
    {
        IEnumerable<Product?> listProducts = await _productRepo.GetAllProducts();
        IEnumerable<ProductResponse?> listResponse = _mapper.Map<IEnumerable<ProductResponse>>(listProducts);
        return listResponse.ToList();
    }

    public async Task<List<ProductResponse>> GetProductsByCondition(Expression<Func<Product, bool>> expression)
    {
        IEnumerable<Product?> listProduct = await _productRepo.GetProductsByCondition(expression);
        IEnumerable<ProductResponse> listResponse = _mapper.Map<IEnumerable<ProductResponse>>(listProduct);
        return listResponse.ToList();
    }

    public async Task<ProductResponse?> UpdateProduct(ProductUpdateRequest request)
    {
        if(request == null)
        {
            throw new ArgumentNullException(nameof(ProductUpdateRequest));
        }
        Product? existingProduct = await _productRepo.GetProductByCondition(p => p.ProductID == request.ProductID);
        if(existingProduct == null)
        {
            throw new ArgumentException($"Invalid Product ID: {request.ProductID}");
        }

        ValidationResult validationResult = _proUpdateRequestValidator.Validate(request);
        if(!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ArgumentException(errors);
        }

        //Map to product entity
        Product inputP = _mapper.Map<Product>(request);

        //Check if product name is changed
        bool isProductNameChanged = request.ProductName != existingProduct.ProductName;

        Product? updatedP =  await _productRepo.UpdateProduct(inputP);
        if (isProductNameChanged)
        {
            //string routingKey = "product.update.name";
            Dictionary<string, object> headers = new Dictionary<string, object>(){
                {"event", "product.update" },
                {"field", "name" },
                {"rowCount", 1 }
            };

            var message = new ProductNameUpdateMessage(inputP.ProductID, inputP.ProductName);
            _rabbitMQPublisher.Publish<ProductNameUpdateMessage>(headers, message);
        }
        return _mapper.Map<ProductResponse?>(updatedP);
    }
}

