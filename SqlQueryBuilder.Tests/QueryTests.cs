using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlQueryBuilder.Tests;

[TestClass]
public sealed class QueryTests
{
    [TestMethod]
    public void Select_WithAliasesAndDynamicFields_ProducesExpectedSql()
    {
        var users = new Table("Users").As("u");
        var fields = new List<Column>
        {
            users.Column<int>("Id"),
            users.Column<string>("Name").As("UserName")
        };

        string sql = Query.From(users).Select(fields).ToSql();

        Assert.AreEqual(
            "SELECT [u].[Id], [u].[Name] AS [UserName]\nFROM [Users] AS [u];",
            NormaliseLineEndings(sql));
    }

    [TestMethod]
    public void Query_WithMultipleInnerJoinsAndOrCondition_ProducesExpectedSql()
    {
        var events = new Table("Events").As("e");
        var links = new Table("EventAttendee").As("ea");
        var attendees = new Table("Attendee").As("a");

        var eventId = events.Column<int>("Id");
        var important = events.Column<int>("Important");
        var linkEventId = links.Column<int>("EventId");
        var linkAttendeeId = links.Column<int>("AttendeeId");
        var attendeeId = attendees.Column<int>("Id");
        var attendeeName = attendees.Column<string>("Name");

        string sql = Query.From(events)
            .Select(eventId, attendeeName)
            .InnerJoin(links, eventId.EqualToColumn(linkEventId))
            .InnerJoin(attendees, linkAttendeeId.EqualToColumn(attendeeId))
            .Where(attendeeName.EqualTo("bob").Or(important.EqualTo(1)))
            .ToSql();

        string expected = """
            SELECT [e].[Id], [a].[Name]
            FROM [Events] AS [e]
            INNER JOIN [EventAttendee] AS [ea] ON [e].[Id] = [ea].[EventId]
            INNER JOIN [Attendee] AS [a] ON [ea].[AttendeeId] = [a].[Id]
            WHERE ([a].[Name] = 'bob' OR [e].[Important] = 1);
            """;

        Assert.AreEqual(NormaliseLineEndings(expected), NormaliseLineEndings(sql));
    }

    [TestMethod]
    public void Query_WithOuterJoinAndAndCondition_ProducesExpectedSql()
    {
        var customers = new Table("Customers").As("c");
        var orders = new Table("Orders").As("o");
        var customerId = customers.Column<int>("Id");
        var orderCustomerId = orders.Column<int>("CustomerId");
        var status = orders.Column<string>("Status");

        string sql = Query.From(customers)
            .Select(customerId, status)
            .LeftOuterJoin(orders, customerId.EqualToColumn(orderCustomerId))
            .Where(status.EqualTo("Open").And(customerId.EqualTo(10)))
            .ToSql();

        StringAssert.Contains(sql, "LEFT OUTER JOIN [Orders] AS [o]");
        StringAssert.Contains(sql, "WHERE ([o].[Status] = 'Open' AND [c].[Id] = 10);");
    }

    [TestMethod]
    public void Query_WithoutSelectedColumns_ThrowsClearException()
    {
        var query = Query.From(new Table("Events"));

        var exception = Assert.ThrowsException<InvalidOperationException>(query.ToSql);

        Assert.AreEqual("A query must select at least one column.", exception.Message);
    }

    private static string NormaliseLineEndings(string text) =>
        text.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
}
