
namespace Application.Core.UserInvite_core.Queries;

public record UserInviteFindByIdQuery(Guid GUID) : IRequest<UserInviteFindByIdDTO>;

public record UserInviteFindByIdDTO : IMapFrom<UserInvite>
{
    
    [PropConfig("کاربر")]
    public Guid UserGUID { get; init; }
    
    [PropConfig("شرکت")]
    public Guid OrgLocationGUID { get; init; }
    
    [PropConfig("قبول کردن کاربر")]
    public bool? UserAccepted { get; init; }
    
    [PropConfig("قبول کردن شرکت")]
    public bool? OrgLocationAccepted { get; init; }
    
    [PropConfig("کاربر")]
    public string UserInfo { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }



    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserInvite, UserInviteFindByIdDTO>();
    }
}

public class
    UserInviteFindByIdQueryHandler : IRequestHandler<UserInviteFindByIdQuery, UserInviteFindByIdDTO>
{
    private UserInviteService UserInviteService;

    public UserInviteFindByIdQueryHandler(UserInviteService UserInviteService)
    {
        this.UserInviteService = UserInviteService;
    }

    public async Task<UserInviteFindByIdDTO> Handle(UserInviteFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        UserInviteFindByIdDTO? Data = UserInviteService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<UserInviteFindByIdDTO>(UserInviteService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

