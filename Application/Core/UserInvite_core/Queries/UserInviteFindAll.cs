

namespace Application.Core.UserInvite_core.Queries;

public record UserInviteFindAllQuery : IRequest<List<UserInviteFindAllDTO>>
{

}

public record UserInviteFindAllDTO : IMapFrom<UserInvite>
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
        profile.CreateMap<UserInvite, UserInviteFindAllDTO>();
    }
}

public class
    UserInviteFindAllQueryHandler : IRequestHandler<UserInviteFindAllQuery,
    List<UserInviteFindAllDTO>>
{
    private UserInviteService UserInviteService;
    public UserInviteFindAllQueryHandler(UserInviteService UserInviteService)
    {
        this.UserInviteService = UserInviteService;
    }

    public async Task<List<UserInviteFindAllDTO>> Handle(UserInviteFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await UserInviteService.Table.AsNoTracking()
            .ProjectTo<UserInviteFindAllDTO>(UserInviteService.mapper.ConfigurationProvider).ToListAsync();
    }
}



