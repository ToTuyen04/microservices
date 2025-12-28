using DataAccessLayer.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.RepositoryContracts;
public interface IOrdersRepository
{
    Task<IEnumerable<Orders>> GetOrders();
    Task<IEnumerable<Orders?>> GetOrdersByCondition(FilterDefinition<Orders> filter);
    Task<Orders?> GetOrderByCondition(FilterDefinition<Orders> filter);
    Task<Orders?> AddOrder(Orders order);
    Task<Orders?> UpdateOrder(Orders order);
    Task<bool> DeleteOrder(Guid orderID);
}

