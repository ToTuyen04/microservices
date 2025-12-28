using FluentValidation;

namespace BusinessLogicLayer.DTO;
public class OrderItemAddRequestValidator : AbstractValidator<OrderItemAddRequest>
{
    public OrderItemAddRequestValidator()
    {
        RuleFor(x => x.ProductID)
            .NotEmpty().WithErrorCode("ProductID can't be blank.");

        RuleFor(x => x.UnitPrice)
            .NotEmpty().WithErrorCode("UnitPrice can't be blank.")
            .GreaterThan(0).WithErrorCode("UnitPrice must be greater than zero.");

        RuleFor(x => x.Quantity)
            .NotEmpty().WithErrorCode("Quantity can't be blank.")
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}