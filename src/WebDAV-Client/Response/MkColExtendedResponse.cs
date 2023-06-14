using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace WebDav.Client.Response
{
	public class MkColExtendedResponse : WebDavResponse
	{
		public MkColExtendedResponse(int statusCode)
            : this(statusCode, null, new List<WebDavPropertyStatus>())
		{ }

		public MkColExtendedResponse(int statusCode, IEnumerable<WebDavPropertyStatus> statuses)
			: this(statusCode, null, statuses)
		{ }

		public MkColExtendedResponse(int statusCode, string description)
			: this(statusCode, description, new List<WebDavPropertyStatus>())
		{ }

		public MkColExtendedResponse(int statusCode, string description, [DisallowNull] IEnumerable<WebDavPropertyStatus> statuses)
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
			return string.Format("Extended MKCOL WebDAV response - StatusCode: {0}, Description: {1}", StatusCode, Description);
		}
	}
}
