using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories;
internal class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Orders> _orders;
    private readonly string collectionName = "orders";
    public OrdersRepository(IMongoDatabase mongoDatabase)
    {
        _orders = mongoDatabase.GetCollection<Orders>(collectionName);
    }
    public async Task<Orders?> AddOrder(Orders order)
    {
        order.OrderID = Guid.NewGuid();
        order._id = order.OrderID;

        foreach(OrderItem orderItem in order.OrderItems)
        {
            orderItem._id = Guid.NewGuid();
        }

        await _orders.InsertOneAsync(order);
        return order;
    }

    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Orders> filter = Builders<Orders>.Filter.Eq(temp => temp.OrderID, orderID);
        Orders? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();
        if (existingOrder == null)
            return false;
        DeleteResult deleteResult = await _orders.DeleteOneAsync(filter);
        return deleteResult.DeletedCount > 0;
    }

    public async Task<Orders?> GetOrderByCondition(FilterDefinition<Orders> filter)
    {
        return (await _orders.FindAsync(filter)).FirstOrDefault();
    }

    public async Task<IEnumerable<Orders?>> GetOrders()
    {
        return (await _orders.FindAsync(Builders<Orders>.Filter.Empty)).ToList();
    }

    public async Task<IEnumerable<Orders?>> GetOrdersByCondition(FilterDefinition<Orders> filter)
    {
        return (await _orders.FindAsync(filter)).ToList();
    }

    public async Task<Orders?> UpdateOrder(Orders order)
    {
        FilterDefinition<Orders> filter = Builders<Orders>.Filter.Eq(temp => temp.OrderID, order.OrderID);
        Orders? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();
        if (existingOrder == null)
            return null;

        order._id = existingOrder._id;

        ReplaceOneResult replaceResult = await _orders.ReplaceOneAsync(filter, order);
        return replaceResult.ModifiedCount > 0 ? order : null;
    }
}
