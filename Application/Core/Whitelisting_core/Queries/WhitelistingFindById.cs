
namespace Application.Core.Whitelisting_core.Queries;

public record WhitelistingFindByIdQuery(Guid GUID) : IRequest<WhitelistingFindByIdDTO>;

public record WhitelistingFindByIdDTO : IMapFrom<Whitelisting>
{
    
    [PropConfig("آدرس ای پی")]
    public string IPAddress { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }
    
    
    



    public void Mapping(Profile profile)
    {
        profile.CreateMap<Whitelisting, WhitelistingFindByIdDTO>();
    }
}

public class
    WhitelistingFindByIdQueryHandler : IRequestHandler<WhitelistingFindByIdQuery, WhitelistingFindByIdDTO>
{
    private WhitelistingService WhitelistingService;

    public WhitelistingFindByIdQueryHandler(WhitelistingService WhitelistingService)
    {
        this.WhitelistingService = WhitelistingService;
    }

    public async Task<WhitelistingFindByIdDTO> Handle(WhitelistingFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        WhitelistingFindByIdDTO? Data = WhitelistingService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<WhitelistingFindByIdDTO>(WhitelistingService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

