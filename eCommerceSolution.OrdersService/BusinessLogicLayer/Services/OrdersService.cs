using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services;
internal class OrdersService : IOrdersService
{
    private IOrdersRepository _orderRepo;
    private readonly IMapper _mapper;
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private UsersMicroserviceClient _usersMicroserviceClient;
    private ProductsMicroserviceClient _productsMicroserviceClient;
    public OrdersService(IOrdersRepository orderRepo, IMapper mapper, IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator, IValidator<OrderAddRequest> orderAddRequestValidator, UsersMicroserviceClient usersMicroserviceClient, ProductsMicroserviceClient productsMicroserviceClient)
    {
        _orderRepo = orderRepo;
        _mapper = mapper;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _orderAddRequestValidator = orderAddRequestValidator;
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
    }
    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        List<ProductDTO> productDTOs = new List<ProductDTO>();
        if (orderAddRequest == null)
        {
            throw new ArgumentNullException(nameof(orderAddRequest));
        }

        //Validate
        ValidationResult validationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);
        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        //Validate order items
        foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);
            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }
            //TO DO: Add logic for checking if ProductID exists in Products microservice
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemAddRequest.ProductID);
            if (product == null)
            {
                throw new ArgumentException($"Invalid product ID: {orderItemAddRequest.ProductID}");
            }
            // add product to productDTOS list
            productDTOs.Add(product);
        }

        //TO DO: Add logic for checking if UserID exists in Users microservice
        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);
        if (user == null)
        {
            throw new ArgumentException("Invalid user ID");
        }

        //Convert
        Orders orderInput = _mapper.Map<Orders>(orderAddRequest);

        //Generate values
        foreach (OrderItem orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }
        orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);

        //Invoke repository
        Orders? addedOrder = await _orderRepo.AddOrder(orderInput);

        if (addedOrder == null)
            return null;

        OrderResponse? orderResponse = _mapper.Map<OrderResponse>(addedOrder);

        if (orderResponse != null)
        {
            await MapProductDetailsToOrderItemsResponse(orderResponse, productDTOs!);
        }
        //user above UsersMicroserviceClient
        if (user == null)
            throw new ArgumentException("Invalid user ID");
        _mapper.Map<UserDTO, OrderResponse>(user, orderResponse!);

        return orderResponse;
    }

    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Orders> filter = Builders<Orders>.Filter.Eq(temp => temp.OrderID, orderID);
        Orders? existingOrder = await _orderRepo.GetOrderByCondition(filter);
        if (existingOrder == null)
        {
            return false;
        }

        bool isDeleted = await _orderRepo.DeleteOrder(orderID);
        return isDeleted;
    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Orders> filter)
    {
        Orders? order = await _orderRepo.GetOrderByCondition(filter);
        if (order == null)
            return null;
        OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);

        if (orderResponse != null)
        {
            await MapProductDetailsToOrderItemsResponse(orderResponse);

            UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (userDTO == null)
                throw new ArgumentException("Invalid user ID");
            _mapper.Map<UserDTO, OrderResponse>(userDTO, orderResponse);
        }
        return orderResponse;
    }

    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Orders?> orders = await _orderRepo.GetOrders();
        IEnumerable<OrderResponse?> ordersResponses = _mapper.Map<IEnumerable<OrderResponse?>>(orders);

        //TO DO: Load ProductName and Category in each OrderItem
        foreach (OrderResponse? orderResponse in ordersResponses)
        {
            if (orderResponse == null)
                continue;
            await MapProductDetailsToOrderItemsResponse(orderResponse);

            // Map User details(UserPersonName and Email) to OrderResponse (for each OrderResponse)
            UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if(userDTO==null)
                throw new ArgumentException("Invalid user ID");
            _mapper.Map<UserDTO, OrderResponse>(userDTO, orderResponse);
        }
        return ordersResponses.ToList();
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Orders> filter)
    {
        IEnumerable<Orders?> orders = await _orderRepo.GetOrdersByCondition(filter);
        IEnumerable<OrderResponse?> ordersResponses = _mapper.Map<IEnumerable<OrderResponse?>>(orders);

        foreach (OrderResponse? orderResponse in ordersResponses)
        {
            if (orderResponse == null)
                continue;
            await MapProductDetailsToOrderItemsResponse(orderResponse);

            UserDTO? userDTO = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (userDTO == null)
                throw new ArgumentException("Invalid user ID");
            _mapper.Map<UserDTO, OrderResponse>(userDTO, orderResponse);
        }

        return ordersResponses.ToList();
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {

        List<ProductDTO> productDTOs = new List<ProductDTO>();

        if (orderUpdateRequest == null)
            throw new ArgumentNullException(nameof(orderUpdateRequest));

        ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);

        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);
            if (!orderItemUpdateValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemUpdateValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }
            //TO DO: Add logic for checking if ProductID exists in Products microservice
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemUpdateRequest.ProductID);
            if (product == null)
            {
                throw new ArgumentException($"Invalid product ID: {orderItemUpdateRequest.ProductID}");
            }
            productDTOs.Add(product);
        }

        //TO DO: Add logic for checking if UserID exists in Users microservice
        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);
        if (user == null)
        {
            throw new ArgumentException("Invalid user ID");
        }

        Orders inputOrder = _mapper.Map<Orders>(orderUpdateRequest);
        foreach (OrderItem orderItem in inputOrder.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }
        inputOrder.TotalBill = inputOrder.OrderItems.Sum(temp => temp.TotalPrice);
        Orders? updatedOrder = await _orderRepo.UpdateOrder(inputOrder);
        OrderResponse? orderResponse = _mapper.Map<OrderResponse?>(updatedOrder);
        if (orderResponse != null)
        {
            await MapProductDetailsToOrderItemsResponse(orderResponse, productDTOs!);
        }

        //user above UsersMicroserviceClient
        if (user == null)
            throw new ArgumentException("Invalid user ID");
        _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
        return orderResponse;
    }

    private async Task MapProductDetailsToOrderItemsResponse(OrderResponse orderResponse, List<ProductDTO?> productDTOs = null)
    {
        // Communicate with Products microservice to get product details
        if (productDTOs == null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? proDTO = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);
                if (proDTO != null)
                {
                    _mapper.Map<ProductDTO, OrderItemResponse>(proDTO, orderItemResponse);
                }
            }
        }
        else
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = productDTOs.FirstOrDefault(temp => temp != null && temp.ProductID == orderItemResponse.ProductID);
                if (productDTO != null)
                {
                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, orderItemResponse);
                }
            }
        }
    }
}

