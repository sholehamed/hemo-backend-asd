
namespace Application.Core.Department_core.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid GUID) : IRequest;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand>
{
    private DepartmentService DepartmentService;

    public DeleteDepartmentCommandHandler(DepartmentService DepartmentService)
    {
        this.DepartmentService = DepartmentService;
    }

    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await DepartmentService.Table
            .FindAsync(new object[] { request.GUID }, cancellationToken);

        Guard.Against.NotFound(request.GUID, entity);

        DepartmentService.Table.Remove(entity);

        await DepartmentService.SaveChangesAsync(cancellationToken);
    }

}

