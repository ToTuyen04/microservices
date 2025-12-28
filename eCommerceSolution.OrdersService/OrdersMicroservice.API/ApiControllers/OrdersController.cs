using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersMicroservice.API.ApiControllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        //GET: /api/orders
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            List<OrderResponse?> orders = await _ordersService.GetOrders();
            return Ok(orders);
        }

        //GET: /api/orders/search/orderid/{orderID}
        [HttpGet("search/orderid/{orderID}")]
        public async Task<IActionResult> GetOrdersByOrderID(Guid orderID)
        {
            FilterDefinition<Orders> filter = Builders<Orders>.Filter.Eq(temp => temp.OrderID, orderID);
            OrderResponse? order = await _ordersService.GetOrderByCondition(filter);
            return Ok(order);
        }

        //GET: /api/orders/search/productid/{productID}
        [HttpGet("search/productid/{productID}")]
        public async Task<IActionResult> GetOrdersByProductID(Guid productID)
        {
            FilterDefinition<Orders> filter = Builders<Orders>.Filter.ElemMatch(temp => temp.OrderItems, Builders<OrderItem>.Filter.Eq(temp => temp.ProductID, productID));
            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return Ok(orders);
        }

        //GET: /api/orders/search/orderdate/{orderDate}
        [HttpGet("search/orderdate/{orderDate}")]
        public async Task<IActionResult> GetOrdersByOrderDate(DateTime orderDate)
        {
            FilterDefinition<Orders> filter = Builders<Orders>.Filter.Eq(temp => temp.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyyy-MM-dd"));
            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return Ok(orders);
        }

        //POST: /api/orders
        [HttpPost]
        public async Task<IActionResult> AddOrder([FromBody] OrderAddRequest orderAddRequest)
        {
            if (orderAddRequest == null)
                return BadRequest("Invalid order data");
            OrderResponse? addedOrder = await _ordersService.AddOrder(orderAddRequest);
            if(addedOrder == null)
                return Problem("Failed to add order");
            return Created($"/api/orders/search/orderid/{addedOrder.OrderID}", addedOrder);
        }

        //PUT: /api/orders/{orderID}
        [HttpPut("{orderID}")]
        public async Task<IActionResult> UpdateOrder(Guid orderID, [FromBody] OrderUpdateRequest orderUpdateRequest)
        {
            if (orderID != orderUpdateRequest.OrderID)
            {
                return BadRequest("Order ID in the URL does not match Order ID in the request body.");
            }
            OrderResponse? updatedOrder = await _ordersService.UpdateOrder(orderUpdateRequest);
            return Ok(updatedOrder);
        }

        //DELETE: /api/orders/{orderID}
        [HttpDelete("{orderID}")]
        public async Task<IActionResult> DeleteOrder(Guid orderID)
        {

            if(orderID == Guid.Empty)
            {
                return BadRequest("Invalid Order ID");
            }
            bool isDeleted = await _ordersService.DeleteOrder(orderID);
            if (!isDeleted)
            {
                return NotFound();
            }
            return NoContent();
        }

        //GET: /api/orders/search/userid/{userID}
        [HttpGet("search/userid/{userID}")]
        public async Task<IActionResult> GetOrdersByUserID(Guid userID)
        {
            FilterDefinition<Orders> filter = Builders<Orders>.Filter.Eq(temp => temp.UserID, userID);
            List<OrderResponse?> orders = await _ordersService.GetOrdersByCondition(filter);
            return Ok(orders);
        }
    }
}
