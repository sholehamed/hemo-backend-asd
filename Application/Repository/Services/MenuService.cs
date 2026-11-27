using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Application.Repository.Services;

public class MenuService : BaseRepository<Menu>
{
    public MenuService(IApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    public async Task<string> GetIdentityPath()
    {
        return "/1/";
        string IdentityPath = db.Menu.Where(a => a.Gkey == 1).Select(a => a.Path).First().ToString();
        return IdentityPath;
    }


}