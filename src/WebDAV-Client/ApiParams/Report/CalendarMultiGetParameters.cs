using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

namespace WebDav.Report
{
	/// <summary>
	/// Represents parameters for the REPORT calendar-multiget WebDAV method
	/// </summary>
	public class CalendarMultiGetParameters : IReportParameters
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CalendarMultiGetParameters"/> class.
		/// </summary>
		public CalendarMultiGetParameters()
		{
			GetUris = new List<string>();
			StandardProperties = new List<XName>();
			Namespaces = new List<NamespaceAttr>();
			CancellationToken = CancellationToken.None;
		}

		/// <summary>
		/// Gets or sets the collection of URIs to get details about.
		/// </summary>
		public IReadOnlyCollection<string> GetUris { get; set; }

		/// <summary>
		/// Gets or sets the collection of standard properties. If null or empty, then allprop will be used.
		/// </summary>
		public IReadOnlyCollection<XName> StandardProperties { get; set; }

		/// <summary>
		/// Gets or sets the collection of xml namespaces of properties.
		/// </summary>
		public IReadOnlyCollection<NamespaceAttr> Namespaces { get; set; }
		
		/// <summary>
		/// Gets or sets the cancellation token.
		/// </summary>
		public CancellationToken CancellationToken { get; set; }
	}
}
