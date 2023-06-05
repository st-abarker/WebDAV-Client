using System;
using System.Numerics;
using System.Xml.Linq;
using Microsoft.VisualBasic.CompilerServices;
using WebDav.Client.Response;

namespace WebDav.Response
{
    internal static class PropertyValueParser
    {
        public static string ParseString(XElement element)
        {
            return element != null ? element.Value : null;
        }

        public static int? ParseInteger(XElement element)
        {
            if (element == null)
                return null;

            int value;
            return int.TryParse(element.Value, out value) ? (int?)value : null;
        }

        public static DateTime? ParseDateTime(XElement element)
        {
            if (element == null)
                return null;

            DateTime value;
            return DateTime.TryParse(element.Value, out value) ? (DateTime?)value : null;
        }
        
        public static CalendarComponents ParseSupportedCalendarComponents(XElement element)
        {
	        if (element == null)
		        return CalendarComponents.None;

			var components = CalendarComponents.None;
			foreach (var e in element.Elements(XName.Get("comp", element.Name.NamespaceName)))
			{
				var name = e.Attribute(XName.Get("name"));
				if (name is null)
					continue;
				switch (name.Value)
				{
					case "VEVENT":
						components |= CalendarComponents.VEvent;
						break;
					case "VFREEBUSY":
						components |= CalendarComponents.VFreeBusy;
						break;
					case "VTIMEZONE":
						components |= CalendarComponents.VTimeZone;
						break;
					case "VTODO":
						components |= CalendarComponents.VToDo;
						break;
				}
			}
            
	        return components;
        }

        public static ResourceType ParseResourceType(XElement element)
        {
            if (element == null)
                return ResourceType.Other;

            var type = ResourceType.Other;
            if (element.LocalNameElement("addressbook") is not null)
	            type |= ResourceType.AddressBook;
            if (element.LocalNameElement("calendar") is not null)
	            type |= ResourceType.Calendar;
            if (element.LocalNameElement("collection") is not null)
	            type |= ResourceType.Collection;
            if (element.LocalNameElement("notification") is not null)
	            type |= ResourceType.Notification;
            if (element.LocalNameElement("schedule-inbox") is not null)
	            type |= ResourceType.ScheduleInbox;
            if (element.LocalNameElement("schedule-outbox") is not null)
	            type |= ResourceType.ScheduleOutbox;

            return type;
        }

        public static LockScope? ParseLockScope(XElement element)
        {
            if (element == null)
                return null;

            if (element.LocalNameElement("shared", StringComparison.OrdinalIgnoreCase) != null)
                return LockScope.Shared;
            if (element.LocalNameElement("exclusive", StringComparison.OrdinalIgnoreCase) != null)
                return LockScope.Exclusive;

            return null;
        }

        public static ApplyTo.Lock? ParseLockDepth(XElement element)
        {
            if (element == null)
                return null;
            return element.Value.Equals("0") ? ApplyTo.Lock.ResourceOnly : ApplyTo.Lock.ResourceAndAllDescendants;
        }

        public static LockOwner ParseOwner(XElement element)
        {
            if (element == null)
                return null;

            var uri = element.LocalNameElement("href", StringComparison.OrdinalIgnoreCase);
            if (uri != null && Uri.IsWellFormedUriString(uri.Value, UriKind.Absolute))
                return new UriLockOwner(uri.Value);

            return !string.IsNullOrEmpty(element.Value) ? new PrincipalLockOwner(element.Value) : null;
        }

        public static TimeSpan? ParseLockTimeout(XElement element)
        {
            if (element == null)
                return null;

            var value = element.Value;
            if (value.Equals("infinity", StringComparison.OrdinalIgnoreCase))
                return null;

            if (value.StartsWith("Second-", StringComparison.OrdinalIgnoreCase))
            {
                int seconds;
                if (int.TryParse(value.Substring(value.IndexOf("-") + 1), out seconds))
                    return TimeSpan.FromSeconds(seconds);
            }
            return null;
        }
    }
}
