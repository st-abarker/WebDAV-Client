using System;
using System.Linq;

namespace WebDav.Response
{
	internal class MkCalendarResponseParser : IResponseParser<MkCalendarResponse>
	{
		public MkCalendarResponse Parse(string response, int statusCode, string description)
		{
			if (string.IsNullOrEmpty(response))
				return new MkCalendarResponse(statusCode, description);

			var xResp = XDocumentExtensions.TryParse(response);
			if (xResp?.Root is null)
				return new MkCalendarResponse(statusCode, description);

			var statuses = xResp.Root.LocalNameElements("response", StringComparison.OrdinalIgnoreCase)
				.SelectMany(MultiStatusParser.GetPropertyStatuses)
				.ToList();
			return new MkCalendarResponse(statusCode, description, statuses);
		}
	}
}
