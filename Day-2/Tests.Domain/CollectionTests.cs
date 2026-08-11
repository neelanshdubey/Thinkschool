using Collections.Domain;
using FluentAssertions;

namespace Tests.Domain;

public class CollectionTests
{
    [Fact]
    public void Empty_name_throws()
    {
        var act = () => new Collection("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Name_over_80_characters_throws()
    {
        var name = new string('A', 81);

        var act = () => new Collection(name);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Adding_51st_item_throws()
    {
        var collection = new Collection("My Collection");

        for (var i = 1; i <= 50; i++)
            collection.AddQuote(i);

        var act = () => collection.AddQuote(51);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Duplicate_quote_id_throws()
    {
        var collection = new Collection("My Collection");

        collection.AddQuote(1);

        var act = () => collection.AddQuote(1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Removing_non_existent_item_throws()
    {
        var collection = new Collection("My Collection");

        var act = () => collection.RemoveQuote(1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Adding_then_removing_leaves_zero_items()
    {
        var collection = new Collection("My Collection");

        collection.AddQuote(1);
        collection.RemoveQuote(1);

        collection.QuoteIds.Should().BeEmpty();
    }
}