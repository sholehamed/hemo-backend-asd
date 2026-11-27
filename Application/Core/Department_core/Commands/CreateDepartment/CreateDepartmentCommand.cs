
namespace Application.Core.Department_core.Commands.CreateDepartment;

public record CreateDepartmentCommand : IRequest<Guid>, IMapFrom<Department>
{
     
   [PropConfig("عنوان")]
   public string Name { get; init; }
    
   [PropConfig("کد")]
   public long Gkey { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateDepartmentCommand, Department>();
    }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Guid>
{
    private DepartmentService DepartmentService;
    public CreateDepartmentCommandHandler(DepartmentService  DepartmentService)
    {
        this.DepartmentService  = DepartmentService;
    }

    public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        Department entity = DepartmentService.mapper.Map<Department>(request);

        DepartmentService.Table.Add(entity);

        await DepartmentService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


