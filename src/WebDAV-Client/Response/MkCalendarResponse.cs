using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDav.Response
{
	public class MkCalendarResponse : WebDavResponse
	{
		public MkCalendarResponse(int statusCode)
			: this(statusCode, null, new List<WebDavPropertyStatus>())
		{ }

		public MkCalendarResponse(int statusCode, IEnumerable<WebDavPropertyStatus> statuses)
			: this(statusCode, null, statuses)
		{ }

		public MkCalendarResponse(int statusCode, string description)
			: this(statusCode, description, new List<WebDavPropertyStatus>())
		{ }

		public MkCalendarResponse(int statusCode, string description, [DisallowNull] IEnumerable<WebDavPropertyStatus> statuses)
			: base(statusCode, description)
		{
			if (statuses is null)
				throw new ArgumentNullException(nameof(statuses));
			PropertyStatuses = statuses.ToList();
		}

		/// <summary>
		/// Gets the collection of WebDAV resources.
		/// </summary>
		public IReadOnlyCollection<WebDavPropertyStatus> PropertyStatuses { get; private set; }

		public override string ToString()
		{
			return $"MKCALENDAR WebDAV response - StatusCode: {StatusCode}, Description: {Description}";
		}
	}
}
