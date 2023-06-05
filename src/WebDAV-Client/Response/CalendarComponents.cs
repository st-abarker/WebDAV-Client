using System;

namespace WebDav.Client.Response
{
	[Flags]
	public enum CalendarComponents
	{
		None = 0,
		VEvent = 1,
		VFreeBusy = 2,
		VTimeZone = 4,
		VToDo = 8,
	}
}
