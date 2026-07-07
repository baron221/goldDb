using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Filters;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is not null)
        {

            var argument = context.Arguments.OfType<T>().FirstOrDefault();

            if (argument is not null)
            {
                var validationResult = await validator.ValidateAsync(argument);

                if (!validationResult.IsValid)
                {

                    return Results.BadRequest(ApiResponse<string>.Failure(validationResult.Errors.First().ErrorMessage, 400));
                }
            }
        }

        return await next(context);
    }
}

public static class ValidationFilterExtensions
{

    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>();
    }
}
