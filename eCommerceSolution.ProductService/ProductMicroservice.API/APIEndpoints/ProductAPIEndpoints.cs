using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using FluentValidation;
using FluentValidation.Results;

namespace ProductMicroservice.API.APIEndpoints;
public static class ProductAPIEndpoints
{
    public static IEndpointRouteBuilder MapProductAPIEndpoints(this IEndpointRouteBuilder app)
    {
        //GET /api/products
        app.MapGet("/api/products", async
            (IProductService productService) =>
        {
            List<ProductResponse?> products = await productService.GetProducts();
            return Results.Ok(products);
        });

        //GET /api/products/search/product-id/{ProductID}
        app.MapGet("/api/products/search/product-id/{ProductID:guid}", async
            (IProductService productService, Guid ProductID) =>
        {
            ProductResponse? product = await productService.GetProductByCondition(p => p.ProductID == ProductID);
            if(product == null)
                return Results.NotFound($"Product with ID: {ProductID} not found.");

            return Results.Ok(product);
        });

        //DELETE /api/products/{ProductID}
        app.MapDelete("/api/products/{ProductID:guid}", async
            (IProductService productService, Guid ProductID) =>
        {
            bool isDeleted = await productService.DeleteProduct(ProductID);
            if (isDeleted)
                return Results.Ok($"Product with ID: {ProductID} deleted successfully.");
            else
                return Results.Problem($"Error when deleting product");
        });

        //PUT /api/products
        app.MapPut("/api/products", async (IProductService productService, IValidator<ProductUpdateRequest> validator, ProductUpdateRequest request) =>
        {
            //Validate the ProductAddRequest using Fluent Validation
            ValidationResult validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors = validationResult.Errors.GroupBy(temp => temp.PropertyName).ToDictionary(grp =>
                 grp.Key, grp => grp.Select(err => err.ErrorMessage).ToArray());
                return Results.ValidationProblem(errors);
            }
            ProductResponse? response = await productService.UpdateProduct(request);
            if (response != null)
                return Results.Ok(response);
            else
                return Results.NotFound($"Product with ID: {request.ProductID} not found.");
        });

        //POST api/products
        app.MapPost("/api/products", async (IProductService productService, IValidator<ProductAddRequest> validator, ProductAddRequest request) =>
        {
            //Validate the ProductAddRequest using Fluent Validation
            ValidationResult validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors = validationResult.Errors.GroupBy(temp => temp.PropertyName).ToDictionary(grp =>
                 grp.Key, grp => grp.Select(err => err.ErrorMessage).ToArray());
                return Results.ValidationProblem(errors);
            }
            ProductResponse? response = await productService.AddProduct(request);
            if (response != null)
                return Results.Created($"/api/products/search/product-id/{response.ProductID}", response);
            return Results.Problem("Error in adding product");
        });

        //GET /api/products/search/{SearchString}
        app.MapGet("/api/products/search/{SearchString}", async
            (IProductService productService, string SearchString) =>
        {
            List<ProductResponse?> productsByProductName = await productService.GetProductsByCondition(
                p => p.ProductName != null && p.ProductName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
            );
            List<ProductResponse?> productsByCategory = await productService.GetProductsByCondition(
                p => p.Category != null && p.Category.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
            );
            var products = productsByProductName.Union(productsByCategory);
            return Results.Ok(products);
        });

        return app;
    }
}

