using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebDav.Response;

namespace WebDav.Client.Response
{
	internal class MkColExtendedResponseParser : IResponseParser<MkColExtendedResponse>
	{
		public MkColExtendedResponse Parse(string response, int statusCode, string description)
		{
			if (string.IsNullOrEmpty(response))
				return new MkColExtendedResponse(statusCode, description);

			var xResp = XDocumentExtensions.TryParse(response);
			if (xResp?.Root is null)
				return new MkColExtendedResponse(statusCode, description);

			var statuses = xResp.Root.LocalNameElements("response", StringComparison.OrdinalIgnoreCase)
				.SelectMany(MultiStatusParser.GetPropertyStatuses)
				.ToList();
			return new MkColExtendedResponse(statusCode, description, statuses);
		}
	}
}
