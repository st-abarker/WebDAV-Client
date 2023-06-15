using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WebDav.Report
{
	/// <summary>
	/// Represents parameters for the REPORT addressbook-multiget WebDAV method
	/// </summary>
	public class AddressBookMultiGetParameters : IReportParameters
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AddressBookMultiGetParameters"/> class.
		/// </summary>
		public AddressBookMultiGetParameters()
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
