
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entity;
using System.Linq.Expressions;

namespace BusinessLogicLayer.ServiceContracts;
public interface IProductService
{
    Task<List<ProductResponse?>> GetProducts();
    Task<ProductResponse?> GetProductByCondition(Expression<Func<Product, bool>> expression);
    Task<List<ProductResponse?>> GetProductsByCondition(Expression<Func<Product, bool>> expression);
    Task<ProductResponse?> AddProduct(ProductAddRequest product);
    Task<ProductResponse?> UpdateProduct(ProductUpdateRequest product);
    Task<bool> DeleteProduct(Guid productId);


}
