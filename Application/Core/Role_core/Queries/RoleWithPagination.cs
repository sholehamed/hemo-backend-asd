using Application.Common.Models;
using FluentFTP.Helpers;
using System;

namespace Application.Core.Role_core.Queries;

public record RoleWithPaginationQuery : PageFilter<Role, RoleWithPaginationDTO>
{
    public Guid? PGUID { get; set; }
}

public record RoleWithPaginationDTO : TreeNode<RoleDTO, RoleWithPaginationDTO>, IMapFrom<Role>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Role, RoleWithPaginationDTO>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(a => new RoleDTO()
            {
                Name = a.Name,
                GUID = a.GUID
            }))
            .ForMember(dest => dest.Leaf, opt => opt.MapFrom(a => !a.Roles.Any()));
    }
}

public record RoleDTO
{
    [PropConfig("عنوان")] public string Name { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
}

public class
    RoleWithPaginationQueryHandler : IRequestHandler<RoleWithPaginationQuery,
    PaginatedList<RoleWithPaginationDTO>>
{
    private readonly RoleService RoleService;
    private UserRoleService UserRoleService;

    public RoleWithPaginationQueryHandler(RoleService roleService, UserRoleService userRoleService)
    {
        RoleService = roleService;
        UserRoleService = userRoleService;
    }

    public async Task<PaginatedList<RoleWithPaginationDTO>> Handle(RoleWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();

        IQueryable<Role> queryable = RoleService.Table.Where(a =>
            a.OrgLocation.Path.StartsWith(userRoleInfo.OrgLocationPath)&&a.Path.StartsWith(userRoleInfo.RolePath));

        int MinRoleLevel = 0;
        try
        {
            MinRoleLevel = queryable.Min(a => a.Level);
        }
        catch (Exception)
        {


        }

        foreach (var Filter in request.Filters)
        {
            switch (Filter.FilterBy.ToLower())
            {
                case "name":
                    {
                        queryable = queryable.Where(a =>
                            RoleService.db.Role.Any((b => b.Path.StartsWith(a.Path) && b.Name.Contains(Filter.Value)))
                        );
                        break;
                    }
            }
        }

        Guid ParentRoleGUID = request.PGUID ?? userRoleInfo.RoleGuid;
        PaginatedList<RoleWithPaginationDTO> data = await request.ToPaging(
            queryable.Where(a => (!request.PGUID.HasValue && a.Level == MinRoleLevel) ||
            (request.PGUID.HasValue && a.PRoleGUID == request.PGUID)
                                              ), RoleService.mapper,
            SkipPaging: (request.PGUID.HasValue), SkipFilter: true);

        if (request.Filters.Any())
        {
            foreach (var VARIABLE in data.Data)
            {
                await SetChild(queryable, VARIABLE);
            }
        }

        return data;
    }

    public async Task SetChild(IQueryable<Role> queryable, RoleWithPaginationDTO Item)
    {
        Item.Children = await queryable.Where(a =>
                a.PRoleGUID == Item.Data.GUID
            ).ProjectTo<RoleWithPaginationDTO>(RoleService.mapper.ConfigurationProvider)
            .ToListAsync();

        Item.Expanded = Item.Children.Any();

        foreach (var child in Item.Children)
        {
            await SetChild(queryable, child);
        }
    }
}