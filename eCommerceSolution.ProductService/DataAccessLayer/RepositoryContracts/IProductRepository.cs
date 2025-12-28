using DataAccessLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.IRepository;
public interface IProductRepository
{
    Task<IEnumerable<Product?>> GetAllProducts();
    Task<IEnumerable<Product?>> GetProductsByCondition(Expression<Func<Product, bool>> expression);
    Task<Product?> GetProductByCondition(Expression<Func<Product, bool>> expression);
    Task<Product?> AddProduct(Product product);
    Task<Product?> UpdateProduct(Product product);
    Task<bool> DeleteProduct(Guid productId);

}
