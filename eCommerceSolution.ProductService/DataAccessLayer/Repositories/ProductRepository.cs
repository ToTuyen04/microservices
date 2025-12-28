using DataAccessLayer.Context;
using DataAccessLayer.Entity;
using DataAccessLayer.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories;
internal class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _dbContext;
    public ProductRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Product?> AddProduct(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteProduct(Guid productId)
    {
       var existingProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
        if (existingProduct == null)
            return false;
        _dbContext.Products.Remove(existingProduct);
        int affectedRowCount = await _dbContext.SaveChangesAsync();
        return affectedRowCount > 0;
    }

    public async Task<IEnumerable<Product?>> GetAllProducts()
    {
        return await _dbContext.Products.ToListAsync();
    }

    public async Task<Product?> GetProductByCondition(Expression<Func<Product, bool>> expression)
    {
        Product p = await _dbContext.Products.FirstOrDefaultAsync(expression);
        return p;
    }

    public async Task<IEnumerable<Product?>> GetProductsByCondition(Expression<Func<Product, bool>> expression)
    {
       return await _dbContext.Products.Where(expression).ToListAsync();
    }

    public async Task<Product?> UpdateProduct(Product product)
    {
        Product existingProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductID == product.ProductID);
        if (existingProduct == null)
            return null;
        existingProduct.ProductName = product.ProductName;
        existingProduct.Category = product.Category;
        existingProduct.UnitPrice = product.UnitPrice;
        existingProduct.QuantityInStock = product.QuantityInStock;
        _dbContext.Products.Update(existingProduct);
        _dbContext.SaveChanges();
        return existingProduct;
    }
}

