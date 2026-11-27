
namespace Application.Core.sso_core.Commands.Createsso;

public record CreatessoCommand : IRequest<Guid>, IMapFrom<sso>
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


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreatessoCommand, sso>();
    }
}

public class CreatessoCommandHandler : IRequestHandler<CreatessoCommand, Guid>
{
    private ssoService ssoService;
    public CreatessoCommandHandler(ssoService  ssoService)
    {
        this.ssoService  = ssoService;
    }

    public async Task<Guid> Handle(CreatessoCommand request, CancellationToken cancellationToken)
    {
        sso entity = ssoService.mapper.Map<sso>(request);

        ssoService.Table.Add(entity);

        await ssoService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


