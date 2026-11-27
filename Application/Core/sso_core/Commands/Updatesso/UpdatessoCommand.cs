
namespace Application.Core.sso_core.Commands.Updatesso;

public record UpdatessoCommand : IRequest, IMapFrom<sso>
{
 
   [PropConfig("کاربر")]
   public Guid UserGUID { get; init; }
 
   [PropConfig("پکیج")]
   public Guid PackageGUID { get; init; }
 
   [PropConfig("آدرس IP")]
   public string IPAddress { get; init; }
 
   [PropConfig("مرورگر")]
   public string UserAgent { get; init; }
 
   [PropConfig("تاریخ انقضا")]
   public DateTime ExpireDate { get; init; }
 
   [PropConfig("کلید")]
   public string Key { get; init; }
 
   [PropConfig("اعتبارسنجی")]
   public string Sign { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdatessoCommand, sso>();
    }
}

public class UpdatessoCommandHandler : IRequestHandler<UpdatessoCommand>
{
    private ssoService ssoService;

    public UpdatessoCommandHandler(ssoService ssoService)
    {
        this.ssoService = ssoService;
    }

    public async Task Handle(UpdatessoCommand request, CancellationToken cancellationToken)
    {
        sso? OldEntity = await ssoService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        ssoService.mapper.Map(request, OldEntity);
        
        await ssoService.SaveChangesAsync(cancellationToken);
    }
}

