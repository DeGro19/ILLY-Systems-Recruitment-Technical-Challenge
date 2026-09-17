using SqlQueryBuilder;

// Example 1 reproduces the abstract query supplied in the challenge.
Console.WriteLine("Example 1 - supplied Events query");
Console.WriteLine(BuildEventsQuery().ToSql());
Console.WriteLine();

// Example 2 proves the library is reusable and demonstrates the remaining criteria.
Console.WriteLine("Example 2 - dynamic fields and an outer join");
Console.WriteLine(BuildCustomerQuery().ToSql());

static Query BuildEventsQuery()
{
    var events = new Table("Events").As("e");
    var eventAttendees = new Table("EventAttendee").As("ea");
    var attendees = new Table("Attendee").As("a");

    var eventId = events.Column<int>("Id");
    var eventName = events.Column<string>("Name");
    var important = events.Column<int>("Important");
    var eventLink = eventAttendees.Column<int>("EventId");
    var attendeeLink = eventAttendees.Column<int>("AttendeeId");
    var attendeeId = attendees.Column<int>("Id");
    var attendeeName = attendees.Column<string>("Name");

    return Query
        .From(events)
        .Select(eventId, eventName, attendeeName.As("AttendeeName"))
        .InnerJoin(eventAttendees, eventId.EqualToColumn(eventLink))
        .InnerJoin(attendees, attendeeLink.EqualToColumn(attendeeId))
        .Where(attendeeName.EqualTo("bob").Or(important.EqualTo(1)));
}

static Query BuildCustomerQuery()
{
    var customers = new Table("Customers").As("c");
    var orders = new Table("Orders").As("o");

    var customerId = customers.Column<int>("Id");
    var customerName = customers.Column<string>("Name");
    var orderCustomerId = orders.Column<int>("CustomerId");
    var orderStatus = orders.Column<string>("Status");

    // This list could be changed at runtime before Select receives it.
    var fields = new List<Column>
    {
        customerId,
        customerName.As("CustomerName"),
        orderStatus
    };

    return Query
        .From(customers)
        .Select(fields)
        .LeftOuterJoin(orders, customerId.EqualToColumn(orderCustomerId))
        .Where(orderStatus.EqualTo("Open").And(customerName.EqualTo("Alice")));
}
