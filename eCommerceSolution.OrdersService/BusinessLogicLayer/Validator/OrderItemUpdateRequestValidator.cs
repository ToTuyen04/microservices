using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO;
public class OrderItemUpdateRequestValidator : AbstractValidator<OrderItemUpdateRequest>
{
    public OrderItemUpdateRequestValidator()
    {
        //ProductID
        RuleFor(x => x.ProductID)
            .NotEmpty().WithErrorCode("ProductID can't be blank.");
        //UnitPrice
        RuleFor(x => x.UnitPrice)
            .NotEmpty().WithErrorCode("UnitPrice can't be blank.")
            .GreaterThan(0).WithErrorCode("UnitPrice must be greater than zero.");
        //Quantity
        RuleFor(x => x.Quantity)
            .NotEmpty().WithErrorCode("Quantity can't be blank.")
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
