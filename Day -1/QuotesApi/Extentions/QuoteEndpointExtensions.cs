using QuotesApi.Models;
using QuotesApi.Repositories;

namespace QuotesApi.Extensions;

public static class QuoteEndpointExtensions
{
    public static WebApplication MapQuoteEndpoints(
        this WebApplication app)
    {
        app.MapGet("/api/quotes", async (
            IQuoteRepository repository,
            CancellationToken cancellationToken,
            int? page,
            int? size) =>
        {
            var currentPage = page ?? 1;
            var currentSize = size ?? 10;

            if (currentPage < 1)
                currentPage = 1;

            if (currentSize < 1 || currentSize > 100)
                currentSize = 10;

            var quotes = await repository.GetAllAsync(
                currentPage,
                currentSize,
                cancellationToken);

            return Results.Ok(quotes);
        });

        app.MapPost("/api/quotes", async (
            CreateQuoteRequest request,
            IQuoteRepository repository,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Author) ||
                string.IsNullOrWhiteSpace(request.Text))
            {
                return Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        ["author"] = string.IsNullOrWhiteSpace(request.Author)
                            ? ["Author is required."]
                            : [],

                        ["text"] = string.IsNullOrWhiteSpace(request.Text)
                            ? ["Text is required."]
                            : []
                    });
            }

            var quote = new Quote
            {
                Author = request.Author.Trim(),
                Text = request.Text.Trim()
            };

            var created = await repository.AddAsync(
                quote,
                cancellationToken);

            return Results.Created(
                $"/api/quotes/{created.Id}",
                created);
        });

        app.MapGet("/api/quotes/{id:int}", async (
            int id,
            IQuoteRepository repository,
            CancellationToken cancellationToken) =>
        {
            var quote = await repository.GetByIdAsync(
                id,
                cancellationToken);

            return quote is null
                ? Results.NotFound()
                : Results.Ok(quote);
        });

        app.MapDelete("/api/quotes/{id:int}", async (
            int id,
            IQuoteRepository repository,
            CancellationToken cancellationToken) =>
        {
            var deleted = await repository.DeleteAsync(
                id,
                cancellationToken);

            return deleted
                ? Results.NoContent()
                : Results.NotFound();
        });

        return app;
    }
}