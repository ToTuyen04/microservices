using BusinessLogicLayer.DTO;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Validator;
public class ProductUpdateRequestValidator : AbstractValidator<ProductUpdateRequest>
{
    public ProductUpdateRequestValidator()
    {
        RuleFor(temp => temp.ProductName)
            .NotEmpty().WithMessage("Product name must not be empty.");

        RuleFor(temp => temp.Category)
            .IsInEnum().WithMessage("Invalid category option.");

        RuleFor(temp => temp.UnitPrice)
            .InclusiveBetween(0, double.MaxValue).WithMessage($"Unit price must be between 0 and {double.MaxValue}.");

        RuleFor(temp => temp.QuantityInStock)
            .NotNull().WithMessage("Quantity in stock must not be null.")
            .GreaterThanOrEqualTo(0).WithMessage("Quantity in stock must be greater than or equal to 0.");
    }
}

