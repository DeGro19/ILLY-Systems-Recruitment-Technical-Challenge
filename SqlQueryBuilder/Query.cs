using System.Text;

namespace SqlQueryBuilder;

/// <summary>Stores a SELECT query and converts it into T-SQL.</summary>
public sealed class Query
{
    private readonly Table _fromTable;
    private readonly List<Column> _selectedColumns = [];
    private readonly List<Join> _joins = [];
    private Condition? _whereCondition;

    private Query(Table fromTable)
    {
        _fromTable = fromTable ?? throw new ArgumentNullException(nameof(fromTable));
    }

    public static Query From(Table table) => new(table);

    // Convenient when the fields are known while writing the query.
    public Query Select(params Column[] columns) =>
        Select((IEnumerable<Column>)columns);

    // IEnumerable supports a field list assembled dynamically at runtime.
    public Query Select(IEnumerable<Column> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);
        _selectedColumns.AddRange(columns);
        return this;
    }

    public Query InnerJoin(Table table, Condition onCondition)
    {
        _joins.Add(new Join("INNER JOIN", table, onCondition));
        return this;
    }

    // One outer-join form meets the stated OUTER JOIN requirement.
    public Query LeftOuterJoin(Table table, Condition onCondition)
    {
        _joins.Add(new Join("LEFT OUTER JOIN", table, onCondition));
        return this;
    }

    public Query Where(Condition condition)
    {
        _whereCondition = condition ?? throw new ArgumentNullException(nameof(condition));
        return this;
    }

    public string ToSql()
    {
        if (_selectedColumns.Count == 0)
        {
            throw new InvalidOperationException("A query must select at least one column.");
        }

        var sql = new StringBuilder();
        sql.Append("SELECT ");
        sql.AppendLine(string.Join(", ", _selectedColumns.Select(c => c.SelectSql)));
        sql.Append("FROM ");
        sql.Append(_fromTable.ToSql());

        foreach (Join join in _joins)
        {
            sql.AppendLine();
            sql.Append(join.ToSql());
        }

        if (_whereCondition is not null)
        {
            sql.AppendLine();
            sql.Append("WHERE ");
            sql.Append(_whereCondition.ToSql());
        }

        sql.Append(';');
        return sql.ToString();
    }

    // Join is private because callers only need the two clearly named methods above.
    private sealed class Join
    {
        private readonly string _keyword;
        private readonly Table _table;
        private readonly Condition _condition;

        public Join(string keyword, Table table, Condition condition)
        {
            _keyword = keyword;
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public string ToSql() =>
            $"{_keyword} {_table.ToSql()} ON {_condition.ToSql()}";
    }
}
