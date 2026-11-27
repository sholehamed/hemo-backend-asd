
namespace Application.Core.PasswordBlockList_core.Commands.CreatePasswordBlockList;

public record CreatePasswordBlockListCommand : IRequest<Guid>, IMapFrom<PasswordBlockList>
{
     
   [PropConfig("پسورد")]
   public string Password { get; init; }
    
   
   


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreatePasswordBlockListCommand, PasswordBlockList>();
    }
}

public class CreatePasswordBlockListCommandHandler : IRequestHandler<CreatePasswordBlockListCommand, Guid>
{
    private PasswordBlockListService PasswordBlockListService;
    public CreatePasswordBlockListCommandHandler(PasswordBlockListService  PasswordBlockListService)
    {
        this.PasswordBlockListService  = PasswordBlockListService;
    }

    public async Task<Guid> Handle(CreatePasswordBlockListCommand request, CancellationToken cancellationToken)
    {
        PasswordBlockList entity = PasswordBlockListService.mapper.Map<PasswordBlockList>(request);

        PasswordBlockListService.Table.Add(entity);

        await PasswordBlockListService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


