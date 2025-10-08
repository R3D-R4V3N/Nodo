using IResult = Ardalis.Result.IResult;

namespace Rise.Server.Processors;

/// <summary>
/// Sends HTTP responses globally after processing by mapping <see cref="IResult"/> 
/// status codes to corresponding HTTP status codes, and writing them to the response.
/// </summary>
sealed class GlobalResponseSender : IGlobalPostProcessor
{
    public async Task PostProcessAsync(IPostProcessorContext ctx, CancellationToken ct)
    {
        if (!ctx.HttpContext.ResponseStarted())
        {
            if (ctx.Response is IResult result)
            {
                var statusCode = result.Status switch
                {
                    ResultStatus.Ok => StatusCodes.Status200OK,
                    ResultStatus.Created => StatusCodes.Status201Created,
                    ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                    ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
                    ResultStatus.Invalid => StatusCodes.Status400BadRequest,
                    ResultStatus.NotFound => StatusCodes.Status404NotFound,
                    ResultStatus.NoContent => StatusCodes.Status204NoContent,
                    ResultStatus.Conflict => StatusCodes.Status409Conflict,
                    ResultStatus.CriticalError => StatusCodes.Status500InternalServerError,
                    ResultStatus.Error => StatusCodes.Status422UnprocessableEntity,
                    ResultStatus.Unavailable => StatusCodes.Status503ServiceUnavailable,
                    _ => throw new ArgumentOutOfRangeException("Result status is not supported.")
                };

                await ctx.HttpContext.Response.SendAsync(result, statusCode, cancellation: ct);
            }
            else
            {
                await ctx.HttpContext.Response.SendAsync(ctx.Response, cancellation: ct);
            }
        }
    }
}