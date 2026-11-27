
namespace Application.Core.MenuApi_core.Commands.UpdateMenuApi;

public record UpdateMenuApiCommand : IRequest, IMapFrom<MenuApi>
{
 
   [PropConfig("منو")]
   public Guid MenuGUID { get; init; }
 
   [PropConfig("سرویس")]
   public Guid ApiUID { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateMenuApiCommand, MenuApi>();
    }
}

public class UpdateMenuApiCommandHandler : IRequestHandler<UpdateMenuApiCommand>
{
    private MenuApiService MenuApiService;

    public UpdateMenuApiCommandHandler(MenuApiService MenuApiService)
    {
        this.MenuApiService = MenuApiService;
    }

    public async Task Handle(UpdateMenuApiCommand request, CancellationToken cancellationToken)
    {
        MenuApi? OldEntity = await MenuApiService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        MenuApiService.mapper.Map(request, OldEntity);
        
        await MenuApiService.SaveChangesAsync(cancellationToken);
    }
}

