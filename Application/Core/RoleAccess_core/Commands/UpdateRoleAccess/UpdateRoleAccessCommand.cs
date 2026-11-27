
namespace Application.Core.RoleAccess_core.Commands.UpdateRoleAccess;

public record UpdateRoleAccessCommand : IRequest, IMapFrom<RoleAccess>
{
 
   [PropConfig("منو")]
   public Guid MenuGUID { get; init; }
 
   [PropConfig("نقش")]
   public Guid RoleGUID { get; init; }
 
   [PropConfig("سرویس")]
   public Guid ApiGUID { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateRoleAccessCommand, RoleAccess>();
    }
}

public class UpdateRoleAccessCommandHandler : IRequestHandler<UpdateRoleAccessCommand>
{
    private RoleAccessService RoleAccessService;

    public UpdateRoleAccessCommandHandler(RoleAccessService RoleAccessService)
    {
        this.RoleAccessService = RoleAccessService;
    }

    public async Task Handle(UpdateRoleAccessCommand request, CancellationToken cancellationToken)
    {
        RoleAccess? OldEntity = await RoleAccessService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        RoleAccessService.mapper.Map(request, OldEntity);
        
        await RoleAccessService.SaveChangesAsync(cancellationToken);
    }
}

