
namespace Application.Core.PasswordBlockList_core.Commands.UpdatePasswordBlockList;

public record UpdatePasswordBlockListCommand : IRequest, IMapFrom<PasswordBlockList>
{
 
   [PropConfig("پسورد")]
   public string Password { get; init; }
 
   [PropConfig("کلید")]
   public Guid GUID { get; init; }
 
   
   

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UpdatePasswordBlockListCommand, PasswordBlockList>();
    }
}

public class UpdatePasswordBlockListCommandHandler : IRequestHandler<UpdatePasswordBlockListCommand>
{
    private PasswordBlockListService PasswordBlockListService;

    public UpdatePasswordBlockListCommandHandler(PasswordBlockListService PasswordBlockListService)
    {
        this.PasswordBlockListService = PasswordBlockListService;
    }

    public async Task Handle(UpdatePasswordBlockListCommand request, CancellationToken cancellationToken)
    {
        PasswordBlockList? OldEntity = await PasswordBlockListService.Table
            .FindAsync(request.GUID, cancellationToken);

        Guard.Against.NotFound(request.GUID, OldEntity);
        
        PasswordBlockListService.mapper.Map(request, OldEntity);
        
        await PasswordBlockListService.SaveChangesAsync(cancellationToken);
    }
}

