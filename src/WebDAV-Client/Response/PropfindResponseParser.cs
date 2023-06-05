using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics.CodeAnalysis;

namespace WebDav.Response
{
    internal class PropfindResponseParser : IResponseParser<PropfindResponse>
    {
        private readonly LockResponseParser _lockResponseParser;

        public PropfindResponseParser([DisallowNull] LockResponseParser lockResponseParser)
		{
			if (lockResponseParser is null)
				throw new ArgumentNullException(nameof(lockResponseParser));
            _lockResponseParser = lockResponseParser;
        }

        public PropfindResponse Parse(string response, int statusCode, string description)
        {
            if (string.IsNullOrEmpty(response))
                return new PropfindResponse(statusCode, description);

            var xresponse = XDocumentExtensions.TryParse(response);
            if (xresponse == null || xresponse.Root == null)
                return new PropfindResponse(statusCode, description);

            var resources = xresponse.Root.LocalNameElements("response", StringComparison.OrdinalIgnoreCase)
                .Select(ParseResource)
                .ToList();
            return new PropfindResponse(statusCode, description, resources);
        }

        private WebDavResource ParseResource(XElement xresponse)
        {
            var uriValue = xresponse.LocalNameElement("href", StringComparison.OrdinalIgnoreCase).GetValueOrNull();
            var propstats = MultiStatusParser.GetPropstats(xresponse);
            return CreateResource(uriValue, propstats);
        }

        private WebDavResource CreateResource(string uri, List<MultiStatusParser.Propstat> propstats)
        {
            var properties = MultiStatusParser.GetProperties(propstats);
            var resourceType = PropertyValueParser.ParseResourceType(FindProp("{DAV:}resourcetype", properties));
            var resourceBuilder = new WebDavResource.Builder()
                .WithActiveLocks(_lockResponseParser.ParseLockDiscovery(FindProp("{DAV:}lockdiscovery", properties)))
                .WithCalendarComponents(PropertyValueParser.ParseSupportedCalendarComponents(FindProp("{urn:ietf:params:xml:ns:caldav}supported-calendar-component-set", properties)))
                .WithContentLanguage(PropertyValueParser.ParseString(FindProp("{DAV:}getcontentlanguage", properties)))
                .WithContentLength(PropertyValueParser.ParseInteger(FindProp("{DAV:}getcontentlength", properties)))
                .WithContentType(PropertyValueParser.ParseString(FindProp("{DAV:}getcontenttype", properties)))
                .WithCreationDate(PropertyValueParser.ParseDateTime(FindProp("{DAV:}creationdate", properties)))
                .WithDisplayName(PropertyValueParser.ParseString(FindProp("{DAV:}displayname", properties)))
                .WithETag(PropertyValueParser.ParseString(FindProp("{DAV:}getetag", properties)))
                .WithLastModifiedDate(PropertyValueParser.ParseDateTime(FindProp("{DAV:}getlastmodified", properties)))
                .WithProperties(new ReadOnlyCollection<WebDavProperty>(properties.Select(x => new WebDavProperty(x.Name, x.GetInnerXml())).ToList()))
                .WithPropertyStatuses(MultiStatusParser.GetPropertyStatuses(propstats))
                .WithResourceType(resourceType);

            var isHidden = PropertyValueParser.ParseInteger(FindProp("{DAV:}ishidden", properties)) > 0;
            if (isHidden)
                resourceBuilder.IsHidden();

            var isCollection = PropertyValueParser.ParseInteger(FindProp("{DAV:}iscollection", properties)) > 0 ||
                               resourceType.HasFlag(ResourceType.Collection);
            if (isCollection)
            {
                resourceBuilder.IsCollection();
                resourceBuilder.WithUri(uri.TrimEnd('/') + "/");
            }
            else
            {
                resourceBuilder.WithUri(uri);
            }
            return resourceBuilder.Build();
        }

        private static XElement FindProp(XName name, IEnumerable<XElement> properties)
        {
            return properties.FirstOrDefault(x => x.Name.Equals(name));
        }
    }
}
