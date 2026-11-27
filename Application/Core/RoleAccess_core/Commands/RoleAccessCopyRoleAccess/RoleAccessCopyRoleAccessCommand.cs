namespace Application.Core.RoleAccess_core.Commands.RoleAccessCopyRoleAccess;

public record RoleAccessCopyRoleAccessCommand : IRequest, IMapFrom<RoleAccess>
{
    [PropConfig("نقش مبدا")] public Guid SrcRoleGUID { get; init; }

    [PropConfig("نقش مقصد")] public Guid DestRoleGUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<RoleAccessCopyRoleAccessCommand, RoleAccess>();
    }
}

public class RoleAccessCopyRoleAccessCommandHandler : IRequestHandler<RoleAccessCopyRoleAccessCommand>
{
    private RoleAccessService RoleAccessService;
    private RoleService RoleService;

    public RoleAccessCopyRoleAccessCommandHandler(RoleAccessService roleAccessService, RoleService roleService)
    {
        RoleAccessService = roleAccessService;
        RoleService = roleService;
    }

    public async Task Handle(RoleAccessCopyRoleAccessCommand request, CancellationToken cancellationToken)
    {
        await RoleService.RoleIsValid(request.SrcRoleGUID);
        await RoleService.RoleIsValid(request.DestRoleGUID);

        List<RoleAccess> SrcList = RoleAccessService.Table.Where(a =>
            a.RoleGUID == request.SrcRoleGUID).ToList();

        List<RoleAccess> DestList = RoleAccessService.Table.Where(a =>
            a.RoleGUID == request.DestRoleGUID).ToList();

        foreach (var dest in DestList)
        {
            bool Exist = SrcList.Any(a => a.MenuGUID == dest.MenuGUID && a.ApiGUID == dest.ApiGUID);
            if (!Exist)
            {
                RoleAccessService.db.RoleAccess.Add(new RoleAccess()
                {
                    RoleGUID = request.SrcRoleGUID,
                    MenuGUID = dest.MenuGUID,
                    ApiGUID = dest.ApiGUID,
                });
                await RoleAccessService.SaveChangesAsync(cancellationToken);
            }
        }
    }
}