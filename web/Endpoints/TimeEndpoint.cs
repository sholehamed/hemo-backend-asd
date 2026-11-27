
using Application.Core.Api_core.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using NSwag.Annotations;

namespace web.Endpoints
{
    public class Time : EndpointGroupBase
    {
        public override void Map(WebApplication app)
        {
            app.MapGroup(this)
                .MapGet(DateTimeNow, "DateTimeNow");
        }


        [OpenApiOperation("", "تاریخ الان")]
        public async Task<Ok<DateTime>> DateTimeNow(ISender sender)
        {
            var result = DateTime.Now;
            return TypedResults.Ok(result);
        }
    }
}
