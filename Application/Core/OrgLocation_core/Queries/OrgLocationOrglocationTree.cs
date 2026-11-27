using Application.Common.Models;

namespace Application.Core.OrgLocation_core.Queries;

public record OrgLocationOrglocationTreeQuery : IRequest<List<OrgLocationOrglocationTreeDTO>>
{
}

public record OrgLocationOrglocationTreeDTO : TreeNode<OrgLocationOrglocationTreeData, OrgLocationOrglocationTreeDTO>,
    IMapFrom<OrgLocation>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgLocation, OrgLocationOrglocationTreeDTO>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.GUID))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(a => new OrgLocationOrglocationTreeData
            {
            }));
    }
}

public record OrgLocationOrglocationTreeData
{
}

public class
    OrgLocationOrglocationTreeQueryHandler : IRequestHandler<OrgLocationOrglocationTreeQuery,
    List<OrgLocationOrglocationTreeDTO>>
{
    private OrgLocationService OrgLocationService;
    private UserRoleService UserRoleService;

    public OrgLocationOrglocationTreeQueryHandler(OrgLocationService orgLocationService,
        UserRoleService userRoleService)
    {
        OrgLocationService = orgLocationService;
        UserRoleService = userRoleService;
    }

    public async Task<List<OrgLocationOrglocationTreeDTO>> Handle(OrgLocationOrglocationTreeQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        IQueryable<OrgLocation> queryable = OrgLocationService.Table
            .Where(a => a.Path.StartsWith(userRoleInfo.OrgLocationPath)).AsNoTracking();
        List<OrgLocationOrglocationTreeDTO> Data = await queryable.Where(a => a.GUID == userRoleInfo.OrgLocationGUID)
            .ProjectTo<OrgLocationOrglocationTreeDTO>(OrgLocationService.mapper.ConfigurationProvider).ToListAsync();

        foreach (var Item in Data)
        {
            await SetChild(queryable, Item);
        }

        return Data;
    }


    public async Task SetChild(IQueryable<OrgLocation> queryable, OrgLocationOrglocationTreeDTO Item)
    {
        Item.Children = await queryable.Where(a =>
                a.POrgLocationGUID == Item.Key
            ).OrderBy(a => a.Created)
            .ProjectTo<OrgLocationOrglocationTreeDTO>(OrgLocationService.mapper.ConfigurationProvider)
            .ToListAsync();

        Item.Expanded = false;
        Item.Leaf = !Item.Children.Any();
        foreach (var child in Item.Children)
        {
            await SetChild(queryable, child);
        } 
       
    }
}