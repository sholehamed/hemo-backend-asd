
namespace Application.Core.Whitelisting_core.Commands.UpdateWhitelisting;

public record UpdateWhitelistingCommand : IRequest, IMapFrom<Whitelisting>
{
 
   [PropConfig("آدرس ای پی")]
   public string IPAddress { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }
 
   
   

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdateWhitelistingCommand, Whitelisting>();
    }
}

public class UpdateWhitelistingCommandHandler : IRequestHandler<UpdateWhitelistingCommand>
{
    private WhitelistingService WhitelistingService;

    public UpdateWhitelistingCommandHandler(WhitelistingService WhitelistingService)
    {
        this.WhitelistingService = WhitelistingService;
    }

    public async Task Handle(UpdateWhitelistingCommand request, CancellationToken cancellationToken)
    {
        Whitelisting? OldEntity = await WhitelistingService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        WhitelistingService.mapper.Map(request, OldEntity);
        
        await WhitelistingService.SaveChangesAsync(cancellationToken);
    }
}

