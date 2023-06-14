using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WebDav.Request
{
	internal static class MkColExtendedRequestBuilder
	{
		public static string BuildRequestBody(IDictionary<XName, string> propertiesToSet, IReadOnlyCollection<NamespaceAttr> namespaces)
		{
			var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
			var mkcol = new XElement("{DAV:}mkcol", new XAttribute(XNamespace.Xmlns + "D", "DAV:"));

			foreach (var ns in namespaces)
			{
				var nsAttr = string.IsNullOrEmpty(ns.Prefix) ? "xmlns" : XNamespace.Xmlns + ns.Prefix;
				mkcol.SetAttributeValue(nsAttr, ns.Namespace);
			}

			if (propertiesToSet.Any())
			{
				var setEl = new XStreamingElement("{DAV:}set");
				foreach (var prop in propertiesToSet)
				{
					var propEl = new XElement(prop.Key);
					propEl.SetInnerXml(prop.Value);
					setEl.Add(new XElement(XName.Get("prop", "DAV:"), propEl));
				}
				mkcol.Add(setEl);
			}

			doc.Add(mkcol);
			return doc.ToStringWithDeclaration();
		}
	}
}
