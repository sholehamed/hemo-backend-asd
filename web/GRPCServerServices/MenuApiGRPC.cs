using Application.Common.Interfaces;
using Application.Repository.Services;
using Domain.Entities;
using Grpc.Core;

namespace web.GRPCServerServices;

public class MenuApiGRPC : MenuApiGRPCService.MenuApiGRPCServiceBase
{
    private IApplicationDbContext db;
    private ApiService ApiService;
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    public MenuApiGRPC(IApplicationDbContext db, ApiService apiService)
    {
        this.db = db;
        ApiService = apiService;
    }

    public async override Task<InsertMenuApiReply> InsertMenuApi(InsertMenuApiRequest request,
        ServerCallContext context)
    {
        await _lock.WaitAsync();
        try
        {
            Menu? Menu = db.Menu.FirstOrDefault(a => a.routerLink == request.Url);

            if (Menu == null)
            {
                return new InsertMenuApiReply();

                string Url = request.Url.Replace("/:", "@");
                List<string> Paths = Url.Split("/").Where(a => !string.IsNullOrEmpty(a)).ToList();
                Menu? LastChild = null;
                bool FirstChild = true;
                foreach (var path in Paths)
                {
                    string _path = "/" + path.Replace("@", "/:");
                    _path = (FirstChild) ? _path : LastChild.routerLink + _path;
                    Menu CurrentMenu = db.Menu.FirstOrDefault(a => a.routerLink == _path);
                    if (CurrentMenu == null)
                    {
                        CurrentMenu = new Menu()
                        {
                            PMenuGUID = (FirstChild) ? null : LastChild.GUID,
                            label = "",
                            icon = "",
                            routerLink = _path,
                            Gorder = 1,
                            ShowInMenu = false
                        };
                        db.Menu.Add(CurrentMenu);
                        await db.SaveChangesAsync(CancellationToken.None);
                    }

                    LastChild = CurrentMenu;

                    FirstChild = false;
                }

                Menu = LastChild;
            }

            Guid APIID = Guid.Parse(request.Apiid);
            if (APIID == Guid.Parse("915E78BA-B6FA-4036-ACE0-01BD02E4268F"))
            {
                var s = "";
            }
            bool Exist = db.MenuApi.Any(a => a.ApiGUID == APIID && a.MenuGUID == Menu.GUID);
            if (!Exist)
            {
                db.MenuApi.Add(new MenuApi()
                {
                    ApiGUID = APIID,
                    MenuGUID = Menu.GUID
                });
                await db.SaveChangesAsync(CancellationToken.None);

            }
        }
        catch (Exception e)
        {

        }
        finally
        {
            _lock.Release();
        }


        return new InsertMenuApiReply();
    }
}