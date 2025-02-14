using BuildingBlocks.CQRS;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.Behaviors;

public class ValidationBehavior<TReruest, TResponse>
    (IEnumerable<IValidator<TReruest>> validators)
    : IPipelineBehavior<TReruest, TResponse>
    where TReruest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TReruest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TReruest>(request);

        var validatorResults = 
            await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = 
            validatorResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
