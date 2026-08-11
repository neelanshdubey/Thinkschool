using FluentAssertions;
using QuotesApi.Models;

namespace Tests.Domain;

public class QuoteTests
{
    [Fact]
    public void Create_WithEmptyAuthor_ThrowsDomainException()
    {
        var act = () => Quote.Create("", "Some text");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithAuthorLongerThan200Characters_ThrowsDomainException()
    {
        var author = new string('A', 201);

        var act = () => Quote.Create(author, "Some text");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyText_ThrowsDomainException()
    {
        var act = () => Quote.Create("Author", "");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithTextLongerThan1000Characters_ThrowsDomainException()
    {
        var text = new string('T', 1001);

        var act = () => Quote.Create("Author", text);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithValidAuthorAndText_ReturnsTrimmedQuote()
    {
        var quote = Quote.Create("  Author  ", "  Some text  ");

        quote.Author.Should().Be("Author");
        quote.Text.Should().Be("Some text");
        quote.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedTrue()
    {
        var quote = Quote.Create("Author", "Some text");

        quote.SoftDelete();

        quote.IsDeleted.Should().BeTrue();
    }
}
