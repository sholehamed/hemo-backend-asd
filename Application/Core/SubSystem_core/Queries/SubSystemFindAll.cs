namespace Application.Core.SubSystem_core.Queries;

public record SubSystemFindAllQuery : IRequest<List<SubSystemFindAllDTO>>
{
}

public record SubSystemFindAllDTO : IMapFrom<SubSystem>
{
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }


    public void Mapping(Profile profile)
    {
        profile.CreateMap<SubSystem, SubSystemFindAllDTO>();
    }
}

public class
    SubSystemFindAllQueryHandler : IRequestHandler<SubSystemFindAllQuery,
    List<SubSystemFindAllDTO>>
{
    private readonly SubSystemService SubSystemService;

    public SubSystemFindAllQueryHandler(SubSystemService SubSystemService)
    {
        this.SubSystemService = SubSystemService;
    }

    public async Task<List<SubSystemFindAllDTO>> Handle(SubSystemFindAllQuery request,
        CancellationToken cancellationToken)
    {
        return await SubSystemService.Table.AsNoTracking()
            .ProjectTo<SubSystemFindAllDTO>(SubSystemService.mapper.ConfigurationProvider).ToListAsync();
    }
}