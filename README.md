# ILLY System's Recruitment Technical Challenge

## Running it

```
dotnet run --project SqlQueryBuilder.Console
```

This prints two worked examples: the `Events` / `EventAttendee` / `Attendee` query from the brief, and a second query against `Customers` / `Orders` that builds its field list dynamically at runtime and uses a `LEFT OUTER JOIN`, to show the library isn't hard-coded to one shape of query.

To run the tests:

```
dotnet test
```

## Project structure

| File | Responsibility |
|---|---|
| `Table.cs` | Represents a table and its alias; quotes identifiers for T-SQL |
| `Column.cs` | A strongly typed column belonging to a table; knows how to render itself for a `SELECT` list or a reference elsewhere in the query |
| `Condition.cs` | Represents anything that can appear in a `WHERE` or `ON` clause: a column compared to a literal, a column compared to another column, or two conditions combined with `AND`/`OR` |
| `Query.cs` | Assembles a `FROM`, any number of joins, an optional `WHERE`, and the selected columns into the final SQL string |
| `Program.cs` | The two worked examples described above |
| `QueryTests.cs` | Unit tests covering aliasing, dynamic field lists, multiple joins with `OR`, an outer join with `AND`, and the "no columns selected" error case |
