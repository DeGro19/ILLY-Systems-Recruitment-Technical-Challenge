namespace SqlQueryBuilder;

/// <summary>
/// Base class used when columns of different types share one SELECT list.
/// </summary>
public abstract class Column
{
    protected Column(Table table, string name, string? alias = null)
    {
        Table = table ?? throw new ArgumentNullException(nameof(table));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Alias = alias;
    }

    public Table Table { get; }
    public string Name { get; }
    public string? Alias { get; }

    internal string ReferenceSql => $"{Table.ReferenceName}.{Table.Quote(Name)}";

    internal string SelectSql => Alias is null
        ? ReferenceSql
        : $"{ReferenceSql} AS {Table.Quote(Alias)}";
}

/// <summary>Represents a strongly typed column belonging to a table.</summary>
public sealed class Column<T> : Column
{
    internal Column(Table table, string name, string? alias = null)
        : base(table, name, alias)
    {
    }

    public Column<T> As(string alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias);
        return new Column<T>(Table, Name, alias);
    }

    // Literal comparisons are used in WHERE clauses, for example Name = 'bob'.
    public Condition EqualTo(T value) => Condition.EqualToValue(this, value);

    // Requiring the same T prevents joins between incompatible column types.
    public Condition EqualToColumn(Column<T> other) =>
        Condition.EqualToColumn(this, other);
}
