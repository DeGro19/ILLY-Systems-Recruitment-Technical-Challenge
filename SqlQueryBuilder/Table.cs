namespace SqlQueryBuilder;

/// <summary>Represents a table used by a SQL query.</summary>
public sealed class Table
{
    public Table(string name, string? alias = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Alias = alias;
    }

    public string Name { get; }
    public string? Alias { get; }

    // Return a new object so the original table can still be used without an alias.
    public Table As(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        return new Table(Name, alias);
    }

    // T records the column type, allowing the compiler to check comparisons later.
    public Column<T> Column<T>(string name) => new(this, name);

    internal string ToSql()
    {
        string tableName = Quote(Name);
        return Alias is null ? tableName : $"{tableName} AS {Quote(Alias)}";
    }

    internal string ReferenceName => Quote(Alias ?? Name);

    // Square brackets are the normal way to quote identifiers in T-SQL.
    internal static string Quote(string name) =>
        $"[{name.Replace("]", "]]", StringComparison.Ordinal)}]";
}
