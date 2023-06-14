using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WebDav.Request
{
	internal static class MkCalendarRequestBuilder
	{
		public static string BuildRequestBody(IDictionary<XName, string> propertiesToSet, IReadOnlyCollection<NamespaceAttr> namespaces)
		{
			var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
			var mkcal = new XElement("{DAV:}mkcalendar", new XAttribute(XNamespace.Xmlns + "D", "DAV:"));

			foreach (var ns in namespaces)
			{
				var nsAttr = string.IsNullOrEmpty(ns.Prefix) ? "xmlns" : XNamespace.Xmlns + ns.Prefix;
				mkcal.SetAttributeValue(nsAttr, ns.Namespace);
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
				mkcal.Add(setEl);
			}

			doc.Add(mkcal);
			return doc.ToStringWithDeclaration();
		}
	}
}
