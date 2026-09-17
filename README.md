# SqlQueryBuilder

A small, code-first library for building T-SQL `SELECT` statements without an ORM. You compose a query out of C# objects — tables, columns, conditions — and the library turns that into valid Microsoft SQL Server syntax. It doesn't execute anything; it only produces the SQL text, as specified in the brief.

## Why this design

The brief asked for something **lighter weight than an ORM** that can `SELECT` from *any* table, with a dynamic list of fields decided at runtime. Those two requirements pull in slightly different directions, and the shape of the library follows from trying to satisfy both:

- **`Table` and `Column<T>` are generic wrappers, not generated model classes.** An ORM typically maps a C# class to a table, generated (or hand-written) once per table, which is what gives you full IntelliSense on column names. That approach doesn't fit "SELECTs from any table," because it would mean writing a new class every time you wanted to query a table you hadn't modelled yet. So a table and its columns are described directly where the query is built: `new Table("Events")`, `events.Column<int>("Id")`. That's the one place a string is still used for something the compiler can't check — the actual column and table names, because there's no fixed schema for the library to generate types from.
- **Everything downstream of that string is strongly typed.** Once you have a `Column<int>`, the library won't let you compare it to a string, join it to a `Column<string>`, or otherwise mix types. That's what `EqualTo(T value)` and `EqualToColumn(Column<T> other)` are for — the generic parameter is doing real work, not just decoration.
- **Aliasing returns a new object rather than mutating the original.** `table.As("t")` gives you back a new `Table`, leaving the original usable elsewhere. This is a small thing, but it avoids a class of bugs where reusing a `Table` reference somewhere else in a large query unexpectedly picks up an alias set for a different purpose.
- **`Query` only exposes the operations in the brief** — `Select`, `InnerJoin`, `LeftOuterJoin`, `Where` — and nothing else (no `OrderBy`, `GroupBy`, `Distinct`). Keeping the surface area to what was actually asked for felt more honest than padding it out with half-finished extras.

## Running it

Requires .NET 8 (installed automatically alongside Visual Studio 2022/2026 if you select the workload). No external services, no commercial packages — the only third-party dependency is the Microsoft-owned MSTest suite, used for the test project.

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

## A note on language

ILLY Systems' brief allows C#, so that's what this submission is written in. I'm aware from the person specification that F# is the team's primary language and that a willingness to work in it is something you're looking for — I'd be glad to rework a piece of this in F# if that's useful to see, or to talk through how the same design (particularly the `Condition` and `Query` types, which are already close to an immutable, expression-based style) would translate.

## Known limitations / what I'd add next

These weren't required by the brief's example, but are the natural next steps if this were going further:

- **Comparison operators beyond equality.** `Condition` currently only supports `=`, since that's all the brief's example needed. `GreaterThan`, `LessThan`, and `IsNull` would slot into the existing `CombinedCondition` mechanism without changing its shape.
- **More literal types.** `FormatValue` in `Condition.cs` currently handles strings, booleans, and numeric types. `DateTime` and `Guid` are the obvious next additions for anything beyond a toy example.
- **Calling `Where` more than once.** At the moment a second call replaces the first rather than combining them with `AND`. That's a deliberate simplification for now, but worth flagging as a design decision rather than an oversight.