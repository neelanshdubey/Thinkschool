namespace Collections.Domain;

public class Collection
{
    private readonly List<int> _quoteIds = new();

    public string Name { get; }

    public IReadOnlyCollection<int> QuoteIds => _quoteIds.AsReadOnly();

    public Collection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Collection name is required.",
                nameof(name));

        if (name.Length > 80)
            throw new ArgumentException(
                "Collection name cannot exceed 80 characters.",
                nameof(name));

        Name = name;
    }

    public void AddQuote(int quoteId)
    {
        if (_quoteIds.Count >= 50)
            throw new InvalidOperationException(
                "A collection cannot contain more than 50 quotes.");

        if (_quoteIds.Contains(quoteId))
            throw new InvalidOperationException(
                "Quote is already in the collection.");

        _quoteIds.Add(quoteId);
    }

    public void RemoveQuote(int quoteId)
    {
        if (!_quoteIds.Remove(quoteId))
            throw new InvalidOperationException(
                "Quote does not exist in the collection.");
    }
}