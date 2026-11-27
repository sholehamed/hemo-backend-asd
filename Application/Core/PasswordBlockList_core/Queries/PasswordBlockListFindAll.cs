

namespace Application.Core.PasswordBlockList_core.Queries;

public record PasswordBlockListFindAllQuery : IRequest<List<PasswordBlockListFindAllDTO>>
{

}

public record PasswordBlockListFindAllDTO : IMapFrom<PasswordBlockList>
{

    [PropConfig("پسورد")]
    public string Password { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }

    
    



    public void Mapping(Profile profile)
    {
        profile.CreateMap<PasswordBlockList, PasswordBlockListFindAllDTO>();
    }
}

public class
    PasswordBlockListFindAllQueryHandler : IRequestHandler<PasswordBlockListFindAllQuery,
    List<PasswordBlockListFindAllDTO>>
{
    private PasswordBlockListService PasswordBlockListService;
    public PasswordBlockListFindAllQueryHandler(PasswordBlockListService PasswordBlockListService)
    {
        this.PasswordBlockListService = PasswordBlockListService;
    }

    public async Task<List<PasswordBlockListFindAllDTO>> Handle(PasswordBlockListFindAllQuery request,
        CancellationToken cancellationToken)
    {
        
        return await PasswordBlockListService.Table.AsNoTracking()
            .ProjectTo<PasswordBlockListFindAllDTO>(PasswordBlockListService.mapper.ConfigurationProvider).ToListAsync();
    }
}



