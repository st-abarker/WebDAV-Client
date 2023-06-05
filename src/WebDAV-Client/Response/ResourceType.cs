using System;

namespace WebDav.Response
{
    [Flags]
    public enum ResourceType
    {
        Other = 0,
        Collection = 1,
        Calendar = 2,
        AddressBook = 3,
        ScheduleInbox = 8,
        ScheduleOutbox = 16,
        Notification = 23,
    }
}
