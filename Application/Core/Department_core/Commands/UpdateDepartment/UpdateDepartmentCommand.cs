
namespace Application.Core.Department_core.Commands.UpdateDepartment;

public record UpdateDepartmentCommand : IRequest, IMapFrom<Department>
{
 
   [PropConfig("عنوان")]
   public string Name { get; init; }
 
   [PropConfig("کد")]
   public long Gkey { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateDepartmentCommand, Department>();
    }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand>
{
    private DepartmentService DepartmentService;

    public UpdateDepartmentCommandHandler(DepartmentService DepartmentService)
    {
        this.DepartmentService = DepartmentService;
    }

    public async Task Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        Department? OldEntity = await DepartmentService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        DepartmentService.mapper.Map(request, OldEntity);
        
        await DepartmentService.SaveChangesAsync(cancellationToken);
    }
}

