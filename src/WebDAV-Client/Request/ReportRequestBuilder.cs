using System.Xml.Linq;
using WebDav.Report;

namespace WebDav.Request
{
	internal static class ReportRequestBuilder
	{
		public static string BuildRequestBody(IReportParameters parameters)
		{
			switch (parameters)
			{
				case AddressBookMultiGetParameters abParams:
					return BuildRequestBody(abParams);
				case CalendarMultiGetParameters calParams:
					return BuildRequestBody(calParams);
				default: 
					return null;
			}
		}

		public static string BuildRequestBody(AddressBookMultiGetParameters parameters)
		{
			var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
			var abMG = new XElement("{urn:ietf:params:xml:ns:carddav}addressbook-multiget",
				new XAttribute(XNamespace.Xmlns + "C", "urn:ietf:params:xml:ns:carddav"));

			foreach (var ns in parameters.Namespaces)
			{
				var nsAttr = string.IsNullOrEmpty(ns.Prefix) ? "xmlns" : XNamespace.Xmlns + ns.Prefix;
				abMG.SetAttributeValue(nsAttr, ns.Namespace);
			}

			if (parameters.StandardProperties is null || parameters.StandardProperties.Count == 0)
				abMG.Add(new XElement("{DAV:}allprop"));
			else
			{
				var propEl = new XElement("{DAV:}prop");
				foreach(var prop in parameters.StandardProperties)
					propEl.Add(new XElement(new XElement(prop)));
				abMG.Add(propEl);
			}

			if (parameters.GetUris is not null && parameters.GetUris.Count != 0)
			{
				foreach (var uri in parameters.GetUris)
				{
					var href = new XElement("{DAV:}href") { Value = uri };
					abMG.Add(href);
				}
			}

			doc.Add(abMG);
			return doc.ToStringWithDeclaration();
		}
		
		public static string BuildRequestBody(CalendarMultiGetParameters parameters)
		{
			var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
			var calMG = new XElement("{urn:ietf:params:xml:ns:caldav}calendar-multiget",
				new XAttribute(XNamespace.Xmlns + "C", "urn:ietf:params:xml:ns:caldav"));

			foreach (var ns in parameters.Namespaces)
			{
				var nsAttr = string.IsNullOrEmpty(ns.Prefix) ? "xmlns" : XNamespace.Xmlns + ns.Prefix;
				calMG.SetAttributeValue(nsAttr, ns.Namespace);
			}

			if (parameters.StandardProperties is null || parameters.StandardProperties.Count == 0)
				calMG.Add(new XElement("{DAV:}allprop"));
			else
			{
				var propEl = new XElement("{DAV:}prop");
				foreach(var prop in parameters.StandardProperties)
					propEl.Add(new XElement(new XElement(prop)));
				calMG.Add(propEl);
			}

			if (parameters.GetUris is not null && parameters.GetUris.Count != 0)
			{
				foreach (var uri in parameters.GetUris)
				{
					var href = new XElement("{DAV:}href") { Value = uri };
					calMG.Add(href);
				}
			}

			doc.Add(calMG);
			return doc.ToStringWithDeclaration();
		}
	}
}
