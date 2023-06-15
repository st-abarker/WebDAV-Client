using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace WebDav.Client.Response
{
	public class ReportResponse : WebDavResponse
	{
		public ReportResponse(int statusCode)
			: this(statusCode, null, new List<WebDavResource>())
		{ }

		public ReportResponse(int statusCode, IEnumerable<WebDavResource> resources)
			: this(statusCode, null, resources)
		{ }

		public ReportResponse(int statusCode, string description)
			: this(statusCode, description, new List<WebDavResource>())
		{ }

		public ReportResponse(int statusCode, string description, [DisallowNull] IEnumerable<WebDavResource> resources)
			: base(statusCode, description)
		{
			if (resources is null)
				throw new ArgumentNullException(nameof(resources));
			Resources = resources.ToList();
		}
		public IReadOnlyCollection<WebDavResource> Resources { get; private set; }

		public override string ToString()
		{
			return $"REPORT WebDAV response - StatusCode: {StatusCode}, Description {Description}";
		}
	}
}
