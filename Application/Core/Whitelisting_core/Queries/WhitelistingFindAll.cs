

namespace Application.Core.Whitelisting_core.Queries;

public record WhitelistingFindAllQuery : IRequest<List<WhitelistingFindAllDTO>>
{

}

public record WhitelistingFindAllDTO : IMapFrom<Whitelisting>
{

    [PropConfig("آدرس ای پی")]
    public string IPAddress { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }

    
    



    public void Mapping(Profile profile)
    {
        profile.CreateMap<Whitelisting, WhitelistingFindAllDTO>();
    }
}

public class
    WhitelistingFindAllQueryHandler : IRequestHandler<WhitelistingFindAllQuery,
    List<WhitelistingFindAllDTO>>
{
    private WhitelistingService WhitelistingService;
    public WhitelistingFindAllQueryHandler(WhitelistingService WhitelistingService)
    {
        this.WhitelistingService = WhitelistingService;
    }

    public async Task<List<WhitelistingFindAllDTO>> Handle(WhitelistingFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await WhitelistingService.Table.AsNoTracking()
            .ProjectTo<WhitelistingFindAllDTO>(WhitelistingService.mapper.ConfigurationProvider).ToListAsync();
    }
}



