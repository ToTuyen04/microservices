using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.DTO;
public class OrderAddRequestValidator : AbstractValidator<OrderAddRequest>
{
    public OrderAddRequestValidator()
    {
        //UserID
        RuleFor(temp => temp.UserID)
            .NotEmpty()
            .WithErrorCode("UserID can't be blank");

        //OrderDate
        RuleFor(temp => temp.OrderDate)
            .NotEmpty()
            .WithErrorCode("OrderDate can't be blank");

        //Order Items
        RuleFor(temp => temp.OrderItems)
            .NotEmpty()
            .WithErrorCode("Order items can't be blank");
    }
}

