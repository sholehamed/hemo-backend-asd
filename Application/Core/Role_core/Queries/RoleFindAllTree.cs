using Application.Common.Models;
using System;

namespace Application.Core.Role_core.Queries;

public record RoleFindAllTreeQuery : IRequest<List<RoleFindAllTreeDTO>>
{
}

public record RoleFindAllTreeDTO : TreeNode<RoleFindAllTreeData, RoleFindAllTreeDTO>, IMapFrom<Role>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Role, RoleFindAllTreeDTO>()
            .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.GUID))
            .ForMember(dest => dest.Data, opt => opt.MapFrom(a => new RoleFindAllTreeData
            {
            }));
    }
}

public record RoleFindAllTreeData
{
}

public class
    RoleFindAllTreeQueryHandler : IRequestHandler<RoleFindAllTreeQuery,
    List<RoleFindAllTreeDTO>>
{
    private RoleService RoleService;
    private UserRoleService UserRoleService;

    public RoleFindAllTreeQueryHandler(RoleService roleService, UserRoleService userRoleService)
    {
        RoleService = roleService;
        UserRoleService = userRoleService;
    }

    public async Task<List<RoleFindAllTreeDTO>> Handle(RoleFindAllTreeQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        IQueryable<Role> queryable = RoleService.Table.Where(a => a.Path.StartsWith(userRoleInfo.RolePath) || userRoleInfo.OrgLocationGkey == 1).AsNoTracking();

        int MinRoleLevel = 0;
        try
        {
            MinRoleLevel = queryable.Min(a => a.Level);
        }
        catch (Exception)
        {


        }

        List<RoleFindAllTreeDTO> Data = await queryable.Where(a => a.Level == MinRoleLevel)
            .ProjectTo<RoleFindAllTreeDTO>(RoleService.mapper.ConfigurationProvider).ToListAsync();
        foreach (var Item in Data)
        {
            await SetChild(queryable, Item);
        }

        return Data;
    }

    public async Task SetChild(IQueryable<Role> queryable, RoleFindAllTreeDTO Item)
    {
        Item.Children = await queryable.Where(a =>
                a.PRoleGUID == Item.Key
            ).OrderBy(a => a.Created).ProjectTo<RoleFindAllTreeDTO>(RoleService.mapper.ConfigurationProvider)
            .ToListAsync();

        Item.Expanded = false;
        Item.Leaf = !Item.Children.Any();
        foreach (var child in Item.Children)
        {
            await SetChild(queryable, child);
        }
    }
}