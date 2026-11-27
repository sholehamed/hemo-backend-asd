using Application.Common.Models;

namespace Application.Core.Menu_core.Queries;

public record MenuWithPaginationQuery : PageFilter<Menu, MenuWithPaginationDTO>
{
    public Guid? PGUID { get; set; }
    
}

public record MenuWithPaginationDTO : TreeNode<MenuDTO,MenuWithPaginationDTO>, IMapFrom<Menu>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Menu, MenuWithPaginationDTO>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(a => new MenuDTO
            {
                label = a.label,
                icon = a.icon,
                routerLink = a.routerLink,
                Gorder = a.Gorder,
                GUID = a.GUID,
                ShowInMenu= a.ShowInMenu,
                IsPublic = a.IsPublic
            }))
            .ForMember(dest => dest.Leaf, opt => opt.MapFrom(a => !a.Menus.Any()));
    }
}

public record MenuDTO
{
    [PropConfig("عنوان")] public string label { get; init; }

    [PropConfig("آیکون")] public string icon { get; init; }

    [PropConfig("آدرس")] public string? routerLink { get; init; }

    [PropConfig("ترتیب نمایش")] public int Gorder { get; init; }

    [PropConfig("کلید")] public Guid GUID { get; init; }
    [PropConfig("نمایش در منو")] public bool ShowInMenu { get; init; }
    [PropConfig("منو عمومی می باشد")] public bool IsPublic { get; init; }
    
    
    
}

public class
    MenuWithPaginationQueryHandler : IRequestHandler<MenuWithPaginationQuery,
    PaginatedList<MenuWithPaginationDTO>>
{
    private readonly MenuService MenuService;

    public MenuWithPaginationQueryHandler(MenuService MenuService)
    {
        this.MenuService = MenuService;
    }

    public async Task<PaginatedList<MenuWithPaginationDTO>> Handle(MenuWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        
        IQueryable<Menu> queryable  = MenuService.Table;

        foreach (var Filter in request.Filters)
        {
            switch (Filter.FilterBy)
            {
                case "label":
                {
                    queryable= queryable.Where(a => 
                        MenuService.db.Menu.Any((b=>b.Path.StartsWith(a.Path)&&b.label.Contains(Filter.Value)))
                    );
                    break;
                }
                case "icon":
                {
                    queryable= queryable.Where(a => 
                        MenuService.db.Menu.Any((b=>b.Path.StartsWith(a.Path)&&b.icon.Contains(Filter.Value)))
                    );
                    break;
                }
                case "routerLink":
                {
                    queryable= queryable.Where(a => 
                        MenuService.db.Menu.Any((b=>b.Path.StartsWith(a.Path)&&b.routerLink.Contains(Filter.Value)))
                    );
                    break;
                }
                case "Gorder":
                {
                    int gorder = int.Parse(Filter.Value);
                    queryable= queryable.Where(a => 
                        MenuService.db.Menu.Any((b=>b.Path.StartsWith(a.Path)&&b.Gorder==gorder))
                    );
                    break;
                }
            } 
        }
 
        PaginatedList<MenuWithPaginationDTO> data =await request.ToPaging( queryable.Where(a=>a.PMenuGUID==request.PGUID),MenuService.mapper,SkipPaging:(request.PGUID.HasValue),SkipFilter:true);

        if (request.Filters.Any())
        {
            foreach (var VARIABLE in data.Data)
            {
                await SetChild(queryable, VARIABLE);
            }
        }
        
        return data;
    }

    public async Task SetChild(IQueryable<Menu> queryable,MenuWithPaginationDTO Item)
    {
        Item.Children= await queryable.Where(a =>
                a.PMenuGUID == Item.Data.GUID
            ).ProjectTo<MenuWithPaginationDTO>(MenuService.mapper.ConfigurationProvider).OrderBy(a=>a.Data.Gorder)
            .ToListAsync();
        
        Item.Expanded = Item.Children.Any();
        
        foreach (var child in Item.Children)
        {
            await SetChild(queryable,child);
        }
    }
}