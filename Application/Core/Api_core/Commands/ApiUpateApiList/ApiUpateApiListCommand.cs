namespace Application.Core.Api_core.Commands.ApiUpateApiList;

public record ApiUpateApiListCommand : IRequest<Guid>, IMapFrom<Api>
{
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ApiUpateApiListCommand, Api>();
    }
}

public class ApiUpateApiListCommandHandler : IRequestHandler<ApiUpateApiListCommand, Guid>
{
    private readonly ApiService ApiService;

    public ApiUpateApiListCommandHandler(ApiService ApiService)
    {
        this.ApiService = ApiService;
    }

    public async Task<Guid> Handle(ApiUpateApiListCommand request, CancellationToken cancellationToken)
    {
        await ApiService.UpateApiList();

        return Guid.Empty;
    }
}