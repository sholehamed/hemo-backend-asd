namespace Application.Core.Api_core.Commands.DeleteApi;

public record DeleteApiCommand(Guid GUID) : IRequest;

public class DeleteApiCommandHandler : IRequestHandler<DeleteApiCommand>
{
    private readonly ApiService ApiService;

    public DeleteApiCommandHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task Handle(DeleteApiCommand request, CancellationToken cancellationToken)
    {
        var entity = await ApiService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        ApiService.Table.Remove(entity);

        await ApiService.SaveChangesAsync(cancellationToken);
    }
}