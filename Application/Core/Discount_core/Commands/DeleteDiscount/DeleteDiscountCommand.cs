
namespace Application.Core.Discount_core.Commands.DeleteDiscount;

public record DeleteDiscountCommand(Guid GUID) : IRequest;

public class DeleteDiscountCommandHandler : IRequestHandler<DeleteDiscountCommand>
{
    private DiscountService DiscountService;

    public DeleteDiscountCommandHandler(DiscountService DiscountService)
    {
        this.DiscountService = DiscountService;
    }

    public async Task Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
    {
        var entity = await DiscountService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        DiscountService.Table.Remove(entity);

        await DiscountService.SaveChangesAsync(cancellationToken);
    }

}

