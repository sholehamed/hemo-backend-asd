
using Application.Common.Exceptions;

namespace Application.Core.BlockListIP_core.Commands.CreateBlockListIP;

public record CreateBlockListIPCommand : IRequest<Guid>, IMapFrom<BlockListIP>
{
     
   [PropConfig("آدرس ای پی")]
   public string IPAddress { get; init; }
    
   
   


    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateBlockListIPCommand, BlockListIP>();
    }
}

public class CreateBlockListIPCommandHandler : IRequestHandler<CreateBlockListIPCommand, Guid>
{
    private BlockListIPService BlockListIPService;
    public CreateBlockListIPCommandHandler(BlockListIPService  BlockListIPService)
    {
        this.BlockListIPService  = BlockListIPService;
    }

    public async Task<Guid> Handle(CreateBlockListIPCommand request, CancellationToken cancellationToken)
    {
        bool Exist=BlockListIPService.db.BlockListIP.Any(a=>a.IPAddress==request.IPAddress);
        if (Exist)
        {
            throw new ServiceExeption("رکورد مشابه قبلا ثبت شده است.");
        }
        
        BlockListIP entity = BlockListIPService.mapper.Map<BlockListIP>(request);

        BlockListIPService.Table.Add(entity);

        await BlockListIPService.SaveChangesAsync(cancellationToken);

        return entity.GUID;
    }
}


