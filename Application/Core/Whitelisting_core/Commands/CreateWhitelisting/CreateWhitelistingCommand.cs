
using Application.Common.Exceptions;

namespace Application.Core.Whitelisting_core.Commands.CreateWhitelisting;

public record CreateWhitelistingCommand : IRequest<Guid>, IMapFrom<Whitelisting>
{
     
   [PropConfig("آدرس ای پی")]
   public string IPAddress { get; init; }
 

    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateWhitelistingCommand, Whitelisting>();
    }
}

public class CreateWhitelistingCommandHandler : IRequestHandler<CreateWhitelistingCommand, Guid>
{
    private WhitelistingService WhitelistingService;
    public CreateWhitelistingCommandHandler(WhitelistingService  WhitelistingService)
    {
        this.WhitelistingService  = WhitelistingService;
    }

    public async Task<Guid> Handle(CreateWhitelistingCommand request, CancellationToken cancellationToken)
    {
        bool Exist=WhitelistingService.db.Whitelisting.Any(a=>a.IPAddress==request.IPAddress);
        if (Exist)
        {
            throw new ServiceExeption("رکورد مشابه قبلا ثبت شده است.");
        }
        
        Whitelisting entity = WhitelistingService.mapper.Map<Whitelisting>(request);

        WhitelistingService.Table.Add(entity);

        await WhitelistingService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


