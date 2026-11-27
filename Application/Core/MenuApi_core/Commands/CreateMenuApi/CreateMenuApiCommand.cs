namespace Application.Core.MenuApi_core.Commands.CreateMenuApi;

public record CreateMenuApiCommand : IRequest<Guid>, IMapFrom<MenuApi>
{
    [PropConfig("منو")] public Guid MenuGUID { get; init; }

    [PropConfig("سرویس")] public Guid ApiGUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateMenuApiCommand, MenuApi>();
    }
}

public class CreateMenuApiCommandHandler : IRequestHandler<CreateMenuApiCommand, Guid>
{
    private MenuApiService MenuApiService;

    public CreateMenuApiCommandHandler(MenuApiService MenuApiService)
    {
        this.MenuApiService = MenuApiService;
    }

    public async Task<Guid> Handle(CreateMenuApiCommand request, CancellationToken cancellationToken)
    {
        bool Exist = MenuApiService.db.MenuApi.Any(a => a.MenuGUID == request.MenuGUID && a.ApiGUID == request.ApiGUID);
        if (Exist == false)
        {
            MenuApi entity = MenuApiService.mapper.Map<MenuApi>(request);

            MenuApiService.Table.Add(entity);

            await MenuApiService.SaveChangesAsync(cancellationToken);

            return entity.GUID;
        }
        else
        {
            return Guid.Empty;
        }
    }
}