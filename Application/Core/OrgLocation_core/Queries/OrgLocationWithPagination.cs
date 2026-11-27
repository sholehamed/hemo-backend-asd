
using Application.Common.Models;

namespace Application.Core.OrgLocation_core.Queries;

public record OrgLocationWithPaginationQuery : PageFilter<OrgLocation, OrgLocationWithPaginationDTO>
{
    public Guid? PGUID { get; set; }
}

public record OrgLocationWithPaginationDTO : TreeNode<OrgLocationDTO, OrgLocationWithPaginationDTO>, IMapFrom<Menu>
{


    public void Mapping(Profile profile)
    {
        profile.CreateMap<OrgLocation, OrgLocationWithPaginationDTO>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(a => new OrgLocationDTO
            {
                Address = a.Address,
                Email = a.Email,
                Fax = a.Fax,
                Name = a.Name,
                Tell = a.Tell,
                Url = a.Url,
                GUID = a.GUID
            }))
            .ForMember(dest => dest.Leaf, opt => opt.MapFrom(a => !a.ChildOrgLocations.Any()));
    }
}

public record OrgLocationDTO
{

    [PropConfig("عنوان")]
    public string Name { get; init; }

    [PropConfig("آدرس")]
    public string? Address { get; init; }

    [PropConfig("تلفن")]
    public string? Tell { get; init; }

    [PropConfig("فکس")]
    public string? Fax { get; init; }

    [PropConfig("ایمیل")]
    public string? Email { get; init; }

    [PropConfig("آدرس وبسایت")]
    public string? Url { get; init; }

    [PropConfig("کلید")]
    public Guid GUID { get; init; }
}

public class
    OrgLocationWithPaginationQueryHandler : IRequestHandler<OrgLocationWithPaginationQuery,
    PaginatedList<OrgLocationWithPaginationDTO>>
{
    private OrgLocationService OrgLocationService;
    private UserRoleService UserRoleService;

    public OrgLocationWithPaginationQueryHandler(OrgLocationService orgLocationService, UserRoleService userRoleService)
    {
        OrgLocationService = orgLocationService;
        UserRoleService = userRoleService;
    }

    public async Task<PaginatedList<OrgLocationWithPaginationDTO>> Handle(OrgLocationWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        UserRoleInfo userRoleInfo = await UserRoleService.GetUserRoleInfo();
        IQueryable<OrgLocation> queryable = OrgLocationService.Table.Where(a => a.Path.StartsWith(userRoleInfo.OrgLocationPath) && (userRoleInfo.OrgLocationGkey == 1 || a.Path != userRoleInfo.OrgLocationPath)); ;

        foreach (var Filter in request.Filters)
        {
            switch (Filter.FilterBy.ToLower())
            {
                case "name":
                    {
                        queryable = queryable.Where(a =>
                            OrgLocationService.db.OrgLocation.Any((b => b.Path.StartsWith(a.Path) && b.Name.Contains(Filter.Value)))
                        );
                        break;
                    }

                case "address":
                    {
                        queryable = queryable.Where(a =>
                            OrgLocationService.db.OrgLocation.Any((b => b.Path.StartsWith(a.Path) && b.Address.Contains(Filter.Value)))
                        );
                        break;
                    }
                case "tell":
                    {
                        queryable = queryable.Where(a =>
                            OrgLocationService.db.OrgLocation.Any((b => b.Path.StartsWith(a.Path) && b.Tell.Contains(Filter.Value)))
                        );
                        break;
                    }
                case "fax":
                    {
                        queryable = queryable.Where(a =>
                            OrgLocationService.db.OrgLocation.Any((b => b.Path.StartsWith(a.Path) && b.Fax.Contains(Filter.Value)))
                        );
                        break;
                    }
                case "email":
                    {
                        queryable = queryable.Where(a =>
                            OrgLocationService.db.OrgLocation.Any((b => b.Path.StartsWith(a.Path) && b.Email.Contains(Filter.Value)))
                        );
                        break;
                    }
                case "url":
                    {
                        queryable = queryable.Where(a =>
                            OrgLocationService.db.OrgLocation.Any((b => b.Path.StartsWith(a.Path) && b.Url.Contains(Filter.Value)))
                        );
                        break;
                    }
            }
        }
        Guid? ParentOrglocationGUID = request.PGUID ?? userRoleInfo.OrgLocationGUID;
        PaginatedList<OrgLocationWithPaginationDTO> data = await request.ToPaging(queryable.Where(a => a.POrgLocationGUID == ParentOrglocationGUID || a.GUID == ParentOrglocationGUID), OrgLocationService.mapper, SkipPaging: (request.PGUID.HasValue), SkipFilter: true);

        if (request.Filters.Any())
        {
            foreach (var VARIABLE in data.Data)
            {
                await SetChild(queryable, VARIABLE);
            }
        }

        return data;
    }

    public async Task SetChild(IQueryable<OrgLocation> queryable, OrgLocationWithPaginationDTO Item)
    {
        Item.Children = await queryable.Where(a =>
                a.POrgLocationGUID == Item.Data.GUID
            ).ProjectTo<OrgLocationWithPaginationDTO>(OrgLocationService.mapper.ConfigurationProvider)
            .ToListAsync();

        Item.Expanded = Item.Children.Any();

        foreach (var child in Item.Children)
        {
            await SetChild(queryable, child);
        }
    }
}

