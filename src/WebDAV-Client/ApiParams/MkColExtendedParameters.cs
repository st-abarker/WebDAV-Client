using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace WebDav
{
	/// <summary>
	/// Represents parameters for the extended version of the MKCOL WebDAV method.
	/// </summary>
	public class MkColExtendedParameters
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ProppatchParameters"/> class.
		/// </summary>
		public MkColExtendedParameters()
		{
			PropertiesToSet = new Dictionary<XName, string>();
			Namespaces = new List<NamespaceAttr>();
			CancellationToken = CancellationToken.None;
		}

		/// <summary>
		/// Gets or sets properties to set on the resource.
		/// </summary>
		public IDictionary<XName, string> PropertiesToSet { get; set; }
		
		/// <summary>
		/// Gets or sets the collection of xml namespaces of properties.
		/// </summary>
		public IReadOnlyCollection<NamespaceAttr> Namespaces { get; set; }

		/// <summary>
		/// Gets or sets the resource lock token.
		/// </summary>
		public string LockToken { get; set; }

		/// <summary>
		/// Gets or sets the cancellation token.
		/// </summary>
		public CancellationToken CancellationToken { get; set; }
	}
}
