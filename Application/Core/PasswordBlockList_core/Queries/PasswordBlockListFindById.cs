
namespace Application.Core.PasswordBlockList_core.Queries;

public record PasswordBlockListFindByIdQuery(Guid GUID) : IRequest<PasswordBlockListFindByIdDTO>;

public record PasswordBlockListFindByIdDTO : IMapFrom<PasswordBlockList>
{
    
    [PropConfig("پسورد")]
    public string Password { get; init; }
    
    [PropConfig("کلید")]
    public Guid GUID { get; init; }
    
    
    



    public void Mapping(Profile profile)
    {
        profile.CreateMap<PasswordBlockList, PasswordBlockListFindByIdDTO>();
    }
}

public class
    PasswordBlockListFindByIdQueryHandler : IRequestHandler<PasswordBlockListFindByIdQuery, PasswordBlockListFindByIdDTO>
{
    private PasswordBlockListService PasswordBlockListService;

    public PasswordBlockListFindByIdQueryHandler(PasswordBlockListService PasswordBlockListService)
    {
        this.PasswordBlockListService = PasswordBlockListService;
    }

    public async Task<PasswordBlockListFindByIdDTO> Handle(PasswordBlockListFindByIdQuery request,
        CancellationToken cancellationToken)
    {
        PasswordBlockListFindByIdDTO? Data = PasswordBlockListService.Table.AsNoTracking().Where(a=>a.GUID==request.GUID)
            .ProjectTo<PasswordBlockListFindByIdDTO>(PasswordBlockListService.mapper.ConfigurationProvider).FirstOrDefault();
        Guard.Against.NotFound(request.GUID, Data);
        return Data;
    }
}

