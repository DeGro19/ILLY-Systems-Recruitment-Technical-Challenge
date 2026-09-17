using System.Globalization;

namespace SqlQueryBuilder;

/// <summary>Represents a condition used by a JOIN or WHERE clause.</summary>
public abstract class Condition
{
    public Condition And(Condition other) => new CombinedCondition(this, "AND", other);
    public Condition Or(Condition other) => new CombinedCondition(this, "OR", other);

    internal abstract string ToSql();

    internal static Condition EqualToValue<T>(Column<T> column, T value) =>
        new ValueCondition(column, value);

    internal static Condition EqualToColumn<T>(Column<T> left, Column<T> right) =>
        new ColumnCondition(left, right);

    // This condition compares a column with a value supplied by the caller.
    private sealed class ValueCondition : Condition
    {
        private readonly Column _column;
        private readonly object? _value;

        public ValueCondition(Column column, object? value)
        {
            _column = column;
            _value = value;
        }

        internal override string ToSql() =>
            $"{_column.ReferenceSql} = {FormatValue(_value)}";
    }

    // This condition compares two columns, as required by a JOIN.
    private sealed class ColumnCondition : Condition
    {
        private readonly Column _left;
        private readonly Column _right;

        public ColumnCondition(Column left, Column right)
        {
            _left = left;
            _right = right;
        }

        internal override string ToSql() =>
            $"{_left.ReferenceSql} = {_right.ReferenceSql}";
    }

    // Parentheses preserve the intended order when AND and OR are combined.
    private sealed class CombinedCondition : Condition
    {
        private readonly Condition _left;
        private readonly string _operator;
        private readonly Condition _right;

        public CombinedCondition(Condition left, string logicalOperator, Condition right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _operator = logicalOperator;
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        internal override string ToSql() =>
            $"({_left.ToSql()} {_operator} {_right.ToSql()})";
    }

    // These value types are sufficient for the comparisons required by the brief.
    // Null is rejected because "= NULL" would not be valid SQL comparison logic.
    private static string FormatValue(object? value) => value switch
    {
        null => throw new ArgumentNullException(nameof(value)),
        string text => $"'{text.Replace("'", "''", StringComparison.Ordinal)}'",
        bool boolean => boolean ? "1" : "0",
        sbyte or byte or short or ushort or int or uint or long or ulong or
        float or double or decimal =>
            ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture),
        _ => throw new NotSupportedException(
            $"The value type {value.GetType().Name} is not supported.")
    };
}
