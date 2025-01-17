﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Markup;
using WebDav.Client.Response;
using WebDav.Infrastructure;
using WebDav.Report;
using WebDav.Request;
using WebDav.Response;
using RequestHeaders = System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, string>>;

namespace WebDav
{
    /// <summary>
    /// Represents a WebDAV client that can perform WebDAV operations.
    /// </summary>
    public class WebDavClient : IDisposable
    {
        private const string MediaTypeXml = "text/xml";
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;
        private static readonly Encoding FallbackEncoding = Encoding.UTF8;

        private IWebDavDispatcher _dispatcher;

        private IResponseParser<MkCalendarResponse> _mkcalendarResponseParser;
       
        private IResponseParser<MkColExtendedResponse> _mkcolExtendedResponseParser;

        private IResponseParser<PropfindResponse> _propfindResponseParser;

        private IResponseParser<ProppatchResponse> _proppatchResponseParser;

        private IResponseParser<ReportResponse> _reportResponseParser;

        private IResponseParser<LockResponse> _lockResponseParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavClient"/> class.
        /// </summary>
        public WebDavClient()
            : this(new WebDavClientParams())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavClient"/> class.
        /// </summary>
        /// <param name="params">The parameters of the WebDAV client.</param>
        public WebDavClient([DisallowNull] WebDavClientParams @params) : this(ConfigureHttpClient(@params))
		{
			if (@params is null)
				throw new ArgumentNullException(nameof(@params));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavClient"/> class using a HttpClient.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        public WebDavClient([DisallowNull] HttpClient httpClient)
		{
			if (httpClient is null)
				throw new ArgumentNullException(nameof(httpClient));

            SetWebDavDispatcher(new WebDavDispatcher(httpClient));

            var lockResponseParser = new LockResponseParser();
            SetMkCalendarResponseParser(new MkCalendarResponseParser());
            SetMkColExtendedResponseParser(new MkColExtendedResponseParser());
            SetPropfindResponseParser(new PropfindResponseParser(lockResponseParser));
            SetProppatchResponseParser(new ProppatchResponseParser());
            SetReportResponseParser(new ReportResponseParser(lockResponseParser));
            SetLockResponseParser(lockResponseParser);
        }

        /// <summary>
        /// Retrieves properties defined on the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <returns>An instance of <see cref="PropfindResponse" /></returns>
        public Task<PropfindResponse> Propfind(string requestUri)
        {
            return Propfind(CreateUri(requestUri), new PropfindParameters());
        }

        /// <summary>
        /// Retrieves properties defined on the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <returns>An instance of <see cref="PropfindResponse" /></returns>
        public Task<PropfindResponse> Propfind(Uri requestUri)
        {
            return Propfind(requestUri, new PropfindParameters());
        }

        /// <summary>
        /// Retrieves properties defined on the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the PROPFIND operation.</param>
        /// <returns>An instance of <see cref="PropfindResponse" /></returns>
        public Task<PropfindResponse> Propfind(string requestUri, PropfindParameters parameters)
        {
            return Propfind(CreateUri(requestUri), parameters);
        }

        /// <summary>
        /// Retrieves properties defined on the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="parameters">Parameters of the PROPFIND operation.</param>
        /// <returns>An instance of <see cref="PropfindResponse" /></returns>
        public async Task<PropfindResponse> Propfind([DisallowNull] Uri requestUri, [DisallowNull] PropfindParameters parameters)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

			var applyTo = parameters.ApplyTo ?? ApplyTo.Propfind.ResourceAndChildren;
            var headers = new RequestHeaders
            {
                new KeyValuePair<string, string>("Depth", DepthHeaderHelper.GetValueForPropfind(applyTo))
            };
            string requestBody = PropfindRequestBuilder.BuildRequestBody(parameters.StandardProperties, parameters.CustomProperties, parameters.Namespaces);
            var requestParams = new RequestParameters { Headers = headers, Content = new StringContent(requestBody, DefaultEncoding, MediaTypeXml) };
            var response = await _dispatcher.Send(requestUri, WebDavMethod.Propfind, requestParams, parameters.CancellationToken);
            var responseContent = await ReadContentAsString(response.Content).ConfigureAwait(false);
            return _propfindResponseParser.Parse(responseContent, response.StatusCode, response.Description);
        }

        /// <summary>
        /// Sets and/or removes properties defined on the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the PROPPATCH operation.</param>
        /// <returns>An instance of <see cref="ProppatchResponse" /></returns>
        public Task<ProppatchResponse> Proppatch(string requestUri, ProppatchParameters parameters)
        {
            return Proppatch(CreateUri(requestUri), parameters);
        }

        /// <summary>
        /// Sets and/or removes properties defined on the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="parameters">Parameters of the PROPPATCH operation.</param>
        /// <returns>An instance of <see cref="ProppatchResponse" /></returns>
        public async Task<ProppatchResponse> Proppatch([DisallowNull] Uri requestUri, [DisallowNull] ProppatchParameters parameters)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

			var headers = new RequestHeaders();
            if (!string.IsNullOrEmpty(parameters.LockToken))
                headers.Add(new KeyValuePair<string, string>("If", IfHeaderHelper.GetHeaderValue(parameters.LockToken)));

            string requestBody = ProppatchRequestBuilder.BuildRequestBody(
                    parameters.PropertiesToSet,
                    parameters.PropertiesToRemove,
                    parameters.Namespaces);

            var requestParams = new RequestParameters { Headers = headers, Content = new StringContent(requestBody, DefaultEncoding, MediaTypeXml) };

            var response = await _dispatcher.Send(requestUri, WebDavMethod.Proppatch, requestParams, parameters.CancellationToken);
            var responseContent = await ReadContentAsString(response.Content).ConfigureAwait(false);

            return _proppatchResponseParser.Parse(responseContent, response.StatusCode, response.Description);
        }

		/// <summary>
		/// Creates a new collection resource at the location specified by the request URI with the specified properties.
		/// </summary>
		/// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
		/// <param name="parameters">Parameters of the MKCALENDAR operation.</param>
		/// <returns>An instance of <see cref="WebDavResponse" /></returns>
		public Task<MkCalendarResponse> Mkcalendar(string requestUri, MkCalendarParameters parameters)
		{
			return Mkcalendar(CreateUri(requestUri), parameters);
		}

		/// <summary>
		/// Creates a new collection resource at the location specified by the request URI with the specified properties.
		/// </summary>
		/// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
		/// <param name="parameters">Parameters of the MKCALENDAR operation.</param>
		/// <returns>An instance of <see cref="WebDavResponse" /></returns>
		public async Task<MkCalendarResponse> Mkcalendar([DisallowNull] Uri requestUri, [DisallowNull] MkCalendarParameters parameters)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

			var headers = new RequestHeaders();
			if (!string.IsNullOrEmpty(parameters.LockToken))
				headers.Add(new KeyValuePair<string, string>("If", IfHeaderHelper.GetHeaderValue(parameters.LockToken)));

			var requestBody = MkCalendarRequestBuilder.BuildRequestBody(
				parameters.PropertiesToSet,
				parameters.Namespaces);

			var requestParams = new RequestParameters { Headers = headers, Content = new StringContent(requestBody, DefaultEncoding, MediaTypeXml) };

			var response = await _dispatcher.Send(requestUri, WebDavMethod.Mkcalendar, requestParams, parameters.CancellationToken);
			var responseContent = await ReadContentAsString(response.Content).ConfigureAwait(false);

			return _mkcalendarResponseParser.Parse(responseContent, response.StatusCode, response.Description);
		}

		/// <summary>
		/// Creates a new collection resource at the location specified by the request URI.
		/// </summary>
		/// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
		/// <returns>An instance of <see cref="WebDavResponse" /></returns>
		public Task<WebDavResponse> Mkcol(string requestUri)
        {
            return Mkcol(CreateUri(requestUri), new MkColParameters());
        }

        /// <summary>
        /// Creates a new collection resource at the location specified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Mkcol(Uri requestUri)
        {
            return Mkcol(requestUri, new MkColParameters());
        }

        /// <summary>
        /// Creates a new collection resource at the location specified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the MKCOL operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Mkcol(string requestUri, MkColParameters parameters)
        {
            return Mkcol(CreateUri(requestUri), parameters);
        }

        /// <summary>
        /// Creates a new collection resource at the location specified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="parameters">Parameters of the MKCOL operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public async Task<WebDavResponse> Mkcol([DisallowNull] Uri requestUri, [DisallowNull] MkColParameters parameters)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

			var headers = new RequestHeaders();
            if (!string.IsNullOrEmpty(parameters.LockToken))
                headers.Add(new KeyValuePair<string, string>("If", IfHeaderHelper.GetHeaderValue(parameters.LockToken)));

            var requestParams = new RequestParameters { Headers = headers };

            var response = await _dispatcher.Send(requestUri, WebDavMethod.Mkcol, requestParams, parameters.CancellationToken);

            return new WebDavResponse(response.StatusCode, response.Description);
        }

        /// <summary>
        /// Creates a new collection resource at the location specified by the request URI with the specified properties.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the extended MKCOL operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
		public Task<MkColExtendedResponse> MkcolExtended(string requestUri, MkColExtendedParameters parameters)
        {
	        return MkcolExtended(CreateUri(requestUri), parameters);
        }

		/// <summary>
		/// Creates a new collection resource at the location specified by the request URI with the specified properties.
		/// </summary>
		/// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
		/// <param name="parameters">Parameters of the extended MKCOL operation.</param>
		/// <returns>An instance of <see cref="WebDavResponse" /></returns>
		public async Task<MkColExtendedResponse> MkcolExtended([DisallowNull] Uri requestUri, [DisallowNull] MkColExtendedParameters parameters)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

			var headers = new RequestHeaders();
			if (!string.IsNullOrEmpty(parameters.LockToken))
				headers.Add(new KeyValuePair<string, string>("If", IfHeaderHelper.GetHeaderValue(parameters.LockToken)));

			var requestBody = MkColExtendedRequestBuilder.BuildRequestBody(
				parameters.PropertiesToSet,
				parameters.Namespaces);

			var requestParams = new RequestParameters { Headers = headers, Content = new StringContent(requestBody, DefaultEncoding, MediaTypeXml) };

			var response = await _dispatcher.Send(requestUri, WebDavMethod.Mkcol, requestParams, parameters.CancellationToken);
			var responseContent = await ReadContentAsString(response.Content).ConfigureAwait(false);

			return _mkcolExtendedResponseParser.Parse(responseContent, response.StatusCode, response.Description);
		}

		public Task<ReportResponse> Report(string requestUri, IReportParameters parameters)
		{
			return Report(CreateUri(requestUri), parameters);
		}

        public async Task<ReportResponse> Report([DisallowNull] Uri requestUri, [DisallowNull] IReportParameters parameters)
        {
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

			var requestBody = ReportRequestBuilder.BuildRequestBody(parameters);
            var requestParams = new RequestParameters{ Content = new StringContent(requestBody, DefaultEncoding, MediaTypeXml) };
            var response = await _dispatcher.Send(requestUri, WebDavMethod.Report, requestParams, parameters.CancellationToken);
            var responseContent = await ReadContentAsString(response.Content).ConfigureAwait(false);
            return _reportResponseParser.Parse(responseContent, response.StatusCode, response.Description);
        }
        
		/// <summary>
		/// Retrieves the file identified by the request URI telling the server to return it without processing.
		/// </summary>
		/// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
		/// <returns>An instance of <see cref="WebDavStreamResponse" /></returns>
		public Task<WebDavStreamResponse> GetRawFile(string requestUri)
        {
            return GetFile(CreateUri(requestUri), false, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the file identified by the request URI telling the server to return it without processing.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <returns>An instance of <see cref="WebDavStreamResponse" /></returns>
        public Task<WebDavStreamResponse> GetRawFile(Uri requestUri)
        {
            return GetFile(requestUri, false, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the file identified by the request URI telling the server to return it without processing.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the GET operation.</param>
        /// <returns>An instance of <see cref="WebDavStreamResponse" /></returns>
        public Task<WebDavStreamResponse> GetRawFile(string requestUri, GetFileParameters parameters)
        {
            return GetFile(CreateUri(requestUri), false, parameters.CancellationToken);
        }

        /// <summary>
        /// Retrieves the file identified by the request URI telling the server to return it without processing.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="parameters">Parameters of the GET operation.</param>
        /// <returns>An instance of <see cref="WebDavStreamResponse" /></returns>
        public Task<WebDavStreamResponse> GetRawFile(Uri requestUri, GetFileParameters parameters)
        {
            return GetFile(requestUri, false, parameters.CancellationToken);
        }

        /// <summary>
        /// Retrieves the file identified by the request URI telling the server to return a processed response, if possible.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <returns>An instance of <see cref="WebDavStreamResponse" /></returns>
        public Task<WebDavStreamResponse> GetProcessedFile(string requestUri)
        {
            return GetFile(CreateUri(requestUri), true, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the file identified by the request URI telling the server to return a processed response, if possible.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <returns>An instance of <see cref="WebDavStreamResponse" /></returns>
        public Task<WebDavStreamResponse> GetProcessedFile(Uri requestUri)
        {
            return GetFile(requestUri, true, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the file identified by the request URI telling the server to return a processed response, if possible.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the GET operation.</param>
        /// <returns>An instance of <see cref="WebDavStreamResponse" /></returns>
        public Task<WebDavStreamResponse> GetProcessedFile(string requestUri, GetFileParameters parameters)
        {
            return GetFile(CreateUri(requestUri), true, parameters.CancellationToken);
        }

        /// <summary>
        /// Retrieves the file identified by the request URI telling the server to return a processed response, if possible.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="parameters">Parameters of the GET operation.</param>
        /// <returns>An instance of <see cref="WebDavStreamResponse" /></returns>
        public Task<WebDavStreamResponse> GetProcessedFile(Uri requestUri, GetFileParameters parameters)
        {
            return GetFile(requestUri, true, parameters.CancellationToken);
        }

        internal virtual async Task<WebDavStreamResponse> GetFile([DisallowNull] Uri requestUri, bool translate, CancellationToken cancellationToken)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));

            var headers = new RequestHeaders
            {
                new KeyValuePair<string, string>("Translate", translate ? "t" : "f")
            };

            var requestParams = new RequestParameters { Headers = headers };

            var response = await _dispatcher.Send(requestUri, HttpMethod.Get, requestParams, cancellationToken);

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            return new WebDavStreamResponse(response.StatusCode, response.Description, stream);
        }

        /// <summary>
        /// Deletes the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Delete(string requestUri)
        {
            return Delete(CreateUri(requestUri), new DeleteParameters());
        }

        /// <summary>
        /// Deletes the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Delete(Uri requestUri)
        {
            return Delete(requestUri, new DeleteParameters());
        }

        /// <summary>
        /// Deletes the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the DELETE operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Delete(string requestUri, DeleteParameters parameters)
        {
            return Delete(CreateUri(requestUri), parameters);
        }

        /// <summary>
        /// Deletes the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="parameters">Parameters of the DELETE operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public async Task<WebDavResponse> Delete([DisallowNull] Uri requestUri, [DisallowNull] DeleteParameters parameters)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

            var headers = new RequestHeaders();
            if (!string.IsNullOrEmpty(parameters.LockToken))
                headers.Add(new KeyValuePair<string, string>("If", IfHeaderHelper.GetHeaderValue(parameters.LockToken)));

            var requestParams = new RequestParameters { Headers = headers };

            var response = await _dispatcher.Send(requestUri, HttpMethod.Delete, requestParams, parameters.CancellationToken);

            return new WebDavResponse(response.StatusCode, response.Description);
        }

		/// <summary>
		/// Checks the allowed operations for the request URI
		/// </summary>
		/// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
		/// <returns>An <see cref="HttpMethods"/> indicating the allowed operations.</returns>
		public Task<(HttpMethods allowedMethods, string[] davOptions)> Options(string requestUri)
		{
			return Options(CreateUri(requestUri));
        }

		/// <summary>
		/// Checks the allowed operations for the request URI
		/// </summary>
		/// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
		/// <returns>An <see cref="HttpMethods"/> indicating the allowed operations.</returns>
		public async Task<(HttpMethods allowedMethods, string[] davOptions)> Options([DisallowNull] Uri requestUri)
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            var response = await _dispatcher.Send(requestUri, HttpMethod.Options, CancellationToken.None);
            
            var allowed = HttpMethods.None;
            foreach (var method in response.Content.Headers.Allow)
            {
                if (Enum.TryParse(method, true, out HttpMethods enumVal))
                    allowed |= enumVal;
			}

            var davOpts = Array.Empty<string>();
            if (response.Content.Headers.TryGetValues("DAV", out var values))
	            davOpts = values
		            .SelectMany(x => x.Split(','))
		            .Select(x => x.Trim())
		            .ToArray();
            else if (response.Headers.TryGetValues("DAV", out values))
	            davOpts = values
		            .SelectMany(x => x.Split(','))
		            .Select(x => x.Trim())
		            .ToArray();

			if (davOpts.Any(v => v.Equals("extended-mkcol", StringComparison.OrdinalIgnoreCase)))
	            allowed |= HttpMethods.MkColExtended;

			return (allowed, davOpts);
        }

        /// <summary>
        /// Requests the resource to be stored under the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="stream">The stream of content of the resource.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> PutFile(string requestUri, Stream stream)
        {
            return PutFile(CreateUri(requestUri), stream, new PutFileParameters());
        }

        /// <summary>
        /// Requests the resource to be stored under the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="stream">The stream of content of the resource.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> PutFile(Uri requestUri, Stream stream)
        {
            return PutFile(requestUri, stream, new PutFileParameters());
        }

        /// <summary>
        /// Requests the resource to be stored under the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="stream">The stream of content of the resource.</param>
        /// <param name="contentType">The content type of the request body.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> PutFile(string requestUri, Stream stream, string contentType)
        {
            return PutFile(CreateUri(requestUri), stream, new PutFileParameters { ContentType = contentType });
        }

        /// <summary>
        /// Requests the resource to be stored under the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="stream">The stream of content of the resource.</param>
        /// <param name="contentType">The content type of the request body.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> PutFile(Uri requestUri, Stream stream, string contentType)
        {
            return PutFile(requestUri, stream, new PutFileParameters { ContentType = contentType });
        }

        /// <summary>
        /// Requests the resource to be stored under the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="stream">The stream of content of the resource.</param>
        /// <param name="parameters">Parameters of the PUT operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> PutFile(string requestUri, Stream stream, PutFileParameters parameters)
        {
            return PutFile(CreateUri(requestUri), stream, parameters);
        }

        /// <summary>
        /// Requests the resource to be stored under the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="stream">The stream of content of the resource.</param>
        /// <param name="parameters">Parameters of the PUT operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public async Task<WebDavResponse> PutFile([DisallowNull] Uri requestUri, [DisallowNull] Stream stream, [DisallowNull] PutFileParameters parameters)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (stream is null)
				throw new ArgumentNullException(nameof(stream));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

            var headers = new RequestHeaders();
            if (!string.IsNullOrEmpty(parameters.LockToken))
                headers.Add(new KeyValuePair<string, string>("If", IfHeaderHelper.GetHeaderValue(parameters.LockToken)));

            var requestParams = new RequestParameters { Headers = headers, Content = new StreamContent(stream), ContentType = parameters.ContentType };

            var response = await _dispatcher.Send(requestUri, HttpMethod.Put, requestParams, parameters.CancellationToken);

            return new WebDavResponse(response.StatusCode, response.Description);
        }

        /// <summary>
        /// Creates a duplicate of the source resource identified by the source URI in the destination resource identified by the destination URI.
        /// </summary>
        /// <param name="sourceUri">A string that represents the source <see cref="T:System.Uri"/>.</param>
        /// <param name="destUri">A string that represents the destination <see cref="T:System.Uri"/>.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Copy(string sourceUri, string destUri)
        {
            return Copy(CreateUri(sourceUri), CreateUri(destUri), new CopyParameters());
        }

        /// <summary>
        /// Creates a duplicate of the source resource identified by the source URI in the destination resource identified by the destination URI.
        /// </summary>
        /// <param name="sourceUri">The source <see cref="T:System.Uri"/>.</param>
        /// <param name="destUri">The destination <see cref="T:System.Uri"/>.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Copy(Uri sourceUri, Uri destUri)
        {
            return Copy(sourceUri, destUri, new CopyParameters());
        }

        /// <summary>
        /// Creates a duplicate of the source resource identified by the source URI in the destination resource identified by the destination URI.
        /// </summary>
        /// <param name="sourceUri">A string that represents the source <see cref="T:System.Uri"/>.</param>
        /// <param name="destUri">A string that represents the destination <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the COPY operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Copy(string sourceUri, string destUri, CopyParameters parameters)
        {
            return Copy(CreateUri(sourceUri), CreateUri(destUri), parameters);
        }

        /// <summary>
        /// Creates a duplicate of the source resource identified by the source URI in the destination resource identified by the destination URI.
        /// </summary>
        /// <param name="sourceUri">The source <see cref="T:System.Uri"/>.</param>
        /// <param name="destUri">The destination <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the COPY operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public async Task<WebDavResponse> Copy([DisallowNull] Uri sourceUri, [DisallowNull] Uri destUri, [DisallowNull] CopyParameters parameters)
		{
			if (sourceUri is null)
				throw new ArgumentNullException(nameof(sourceUri));
			if (destUri is null)
				throw new ArgumentNullException(nameof(destUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

			var applyTo = parameters.ApplyTo ?? ApplyTo.Copy.ResourceAndAllDescendants;
            var headers = new RequestHeaders
            {
                new KeyValuePair<string, string>("Destination", GetAbsoluteUri(destUri).ToString()),
                new KeyValuePair<string, string>("Depth", DepthHeaderHelper.GetValueForCopy(applyTo)),
                new KeyValuePair<string, string>("Overwrite", parameters.Overwrite ? "T" : "F")
            };

            if (!string.IsNullOrEmpty(parameters.DestLockToken))
                headers.Add(new KeyValuePair<string, string>("If", IfHeaderHelper.GetHeaderValue(parameters.DestLockToken)));

            var requestParams = new RequestParameters { Headers = headers };

            var response = await _dispatcher.Send(sourceUri, WebDavMethod.Copy, requestParams, parameters.CancellationToken);

            return new WebDavResponse(response.StatusCode, response.Description);
        }

        /// <summary>
        /// Moves the resource identified by the source URI to the destination identified by the destination URI.
        /// </summary>
        /// <param name="sourceUri">A string that represents the source <see cref="T:System.Uri"/>.</param>
        /// <param name="destUri">A string that represents the destination <see cref="T:System.Uri"/>.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Move(string sourceUri, string destUri)
        {
            return Move(CreateUri(sourceUri), CreateUri(destUri), new MoveParameters());
        }

        /// <summary>
        /// Moves the resource identified by the source URI to the destination identified by the destination URI.
        /// </summary>
        /// <param name="sourceUri">The source <see cref="T:System.Uri"/>.</param>
        /// <param name="destUri">The destination <see cref="T:System.Uri"/>.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Move(Uri sourceUri, Uri destUri)
        {
            return Move(sourceUri, destUri, new MoveParameters());
        }

        /// <summary>
        /// Moves the resource identified by the source URI to the destination identified by the destination URI.
        /// </summary>
        /// <param name="sourceUri">A string that represents the source <see cref="T:System.Uri"/>.</param>
        /// <param name="destUri">A string that represents the destination <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the MOVE operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Move(string sourceUri, string destUri, MoveParameters parameters)
        {
            return Move(CreateUri(sourceUri), CreateUri(destUri), parameters);
        }

        /// <summary>
        /// Moves the resource identified by the source URI to the destination identified by the destination URI.
        /// </summary>
        /// <param name="sourceUri">The source <see cref="T:System.Uri"/>.</param>
        /// <param name="destUri">The destination <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the MOVE operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public async Task<WebDavResponse> Move([DisallowNull] Uri sourceUri, [DisallowNull] Uri destUri, [DisallowNull] MoveParameters parameters)
		{
			if (sourceUri is null)
				throw new ArgumentNullException(nameof(sourceUri));
			if (destUri is null)
				throw new ArgumentNullException(nameof(destUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

            var headers = new RequestHeaders
            {
                new KeyValuePair<string, string>("Destination", GetAbsoluteUri(destUri).ToString()),
                new KeyValuePair<string, string>("Overwrite", parameters.Overwrite ? "T" : "F")
            };

            if (!string.IsNullOrEmpty(parameters.SourceLockToken))
                headers.Add(new KeyValuePair<string, string>("If", IfHeaderHelper.GetHeaderValue(parameters.SourceLockToken)));

            if (!string.IsNullOrEmpty(parameters.DestLockToken))
                headers.Add(new KeyValuePair<string, string>("If", IfHeaderHelper.GetHeaderValue(parameters.DestLockToken)));

            var requestParams = new RequestParameters { Headers = headers };

            var response = await _dispatcher.Send(sourceUri, WebDavMethod.Move, requestParams, parameters.CancellationToken);

            return new WebDavResponse(response.StatusCode, response.Description);
        }

        /// <summary>
        /// Takes out a shared lock or refreshes an existing lock of the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <returns>An instance of <see cref="LockResponse" /></returns>
        public Task<LockResponse> Lock(string requestUri)
        {
            return Lock(CreateUri(requestUri), new LockParameters());
        }

        /// <summary>
        /// Takes out a shared lock or refreshes an existing lock of the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <returns>An instance of <see cref="LockResponse" /></returns>
        public Task<LockResponse> Lock(Uri requestUri)
        {
            return Lock(requestUri, new LockParameters());
        }

        /// <summary>
        /// Takes out a lock of any type or refreshes an existing lock of the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the LOCK operation.</param>
        /// <returns>An instance of <see cref="LockResponse" /></returns>
        public Task<LockResponse> Lock(string requestUri, LockParameters parameters)
        {
            return Lock(CreateUri(requestUri), parameters);
        }

        /// <summary>
        /// Takes out a lock of any type or refreshes an existing lock of the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="parameters">Parameters of the LOCK operation.</param>
        /// <returns>An instance of <see cref="LockResponse" /></returns>
        public async Task<LockResponse> Lock([DisallowNull] Uri requestUri, [DisallowNull] LockParameters parameters)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

            var headers = new RequestHeaders();
            if (parameters.ApplyTo.HasValue)
                headers.Add(new KeyValuePair<string, string>("Depth", DepthHeaderHelper.GetValueForLock(parameters.ApplyTo.Value)));

            if (parameters.Timeout.HasValue)
                headers.Add(new KeyValuePair<string, string>("Timeout", $"Second-{parameters.Timeout.Value.TotalSeconds}"));

            string requestBody = LockRequestBuilder.BuildRequestBody(parameters);

            var requestParams = new RequestParameters { Headers = headers, Content = new StringContent(requestBody, DefaultEncoding, MediaTypeXml) };

            var response = await _dispatcher.Send(requestUri, WebDavMethod.Lock, requestParams, parameters.CancellationToken);

            if (!response.IsSuccessful)
                return new LockResponse(response.StatusCode, response.Description);

            var responseContent = await ReadContentAsString(response.Content).ConfigureAwait(false);

            return _lockResponseParser.Parse(responseContent, response.StatusCode, response.Description);
        }

        /// <summary>
        /// Removes the lock identified by the lock token from the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="lockToken">The resource lock token.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Unlock(string requestUri, string lockToken)
        {
            return Unlock(CreateUri(requestUri), new UnlockParameters(lockToken));
        }

        /// <summary>
        /// Removes the lock identified by the lock token from the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="lockToken">The resource lock token.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Unlock(Uri requestUri, string lockToken)
        {
            return Unlock(requestUri, new UnlockParameters(lockToken));
        }

        /// <summary>
        /// Removes the lock identified by the lock token from the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="T:System.Uri"/>.</param>
        /// <param name="parameters">Parameters of the UNLOCK operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public Task<WebDavResponse> Unlock(string requestUri, UnlockParameters parameters)
        {
            return Unlock(CreateUri(requestUri), parameters);
        }

        /// <summary>
        /// Removes the lock identified by the lock token from the resource identified by the request URI.
        /// </summary>
        /// <param name="requestUri">The <see cref="System.Uri"/> to request.</param>
        /// <param name="parameters">Parameters of the UNLOCK operation.</param>
        /// <returns>An instance of <see cref="WebDavResponse" /></returns>
        public async Task<WebDavResponse> Unlock([DisallowNull] Uri requestUri, [DisallowNull] UnlockParameters parameters)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (parameters is null)
				throw new ArgumentNullException(nameof(parameters));

            var headers = new RequestHeaders
            {
                new KeyValuePair<string, string>("Lock-Token", $"<{parameters.LockToken}>")
            };

            var requestParams = new RequestParameters { Headers = headers };

            var response = await _dispatcher.Send(requestUri, WebDavMethod.Unlock, requestParams, parameters.CancellationToken);

            return new WebDavResponse(response.StatusCode, response.Description);
        }

        /// <summary>
        /// Sets the dispatcher of WebDAV requests.
        /// </summary>
        /// <param name="dispatcher">The dispatcher of WebDAV http requests.</param>
        /// <returns>This instance of <see cref="WebDavClient" /> to support chain calls.</returns>
        internal WebDavClient SetWebDavDispatcher([DisallowNull] IWebDavDispatcher dispatcher)
		{
			if (dispatcher is null)
				throw new ArgumentNullException(nameof(dispatcher));
            _dispatcher = dispatcher;
            return this;
        }

        internal WebDavClient SetMkCalendarResponseParser([DisallowNull] IResponseParser<MkCalendarResponse> responseParser)
        {
	        if (responseParser is null)
		        throw new ArgumentNullException(nameof(responseParser));
	        _mkcalendarResponseParser = responseParser;
	        return this;
        }

		internal WebDavClient SetMkColExtendedResponseParser([DisallowNull] IResponseParser<MkColExtendedResponse> responseParser)
		{
			if (responseParser is null)
				throw new ArgumentNullException(nameof(responseParser));
			_mkcolExtendedResponseParser = responseParser;
			return this;
		}

        /// <summary>
        /// Sets the parser of PROPFIND responses.
        /// </summary>
        /// <param name="responseParser">The parser of WebDAV PROPFIND responses.</param>
        /// <returns>This instance of <see cref="WebDavClient" /> to support chain calls.</returns>
        internal WebDavClient SetPropfindResponseParser([DisallowNull] IResponseParser<PropfindResponse> responseParser)
		{
			if (responseParser is null)
				throw new ArgumentNullException(nameof(responseParser));
			_propfindResponseParser = responseParser;
            return this;
        }

        /// <summary>
        /// Sets the parser of PROPPATCH responses.
        /// </summary>
        /// <param name="responseParser">The parser of WebDAV PROPPATCH responses.</param>
        /// <returns>This instance of <see cref="WebDavClient" /> to support chain calls.</returns>
        internal WebDavClient SetProppatchResponseParser([DisallowNull] IResponseParser<ProppatchResponse> responseParser)
		{
			if (responseParser is null)
				throw new ArgumentNullException(nameof(responseParser));
            _proppatchResponseParser = responseParser;
            return this;
        }

        /// <summary>
        /// Sets the parser of PROPFIND responses.
        /// </summary>
        /// <param name="responseParser">The parser of WebDAV PROPFIND responses.</param>
        /// <returns>This instance of <see cref="WebDavClient" /> to support chain calls.</returns>
        internal WebDavClient SetReportResponseParser([DisallowNull] IResponseParser<ReportResponse> responseParser)
        {
	        if (responseParser is null)
		        throw new ArgumentNullException(nameof(responseParser));
	        _reportResponseParser = responseParser;
	        return this;
        }

		/// <summary>
		/// Sets the parser of LOCK responses.
		/// </summary>
		/// <param name="responseParser">The parser of WebDAV LOCK responses.</param>
		/// <returns>This instance of <see cref="WebDavClient" /> to support chain calls.</returns>
		internal WebDavClient SetLockResponseParser([DisallowNull] IResponseParser<LockResponse> responseParser)
		{
			if (responseParser is null)
				throw new ArgumentNullException(nameof(responseParser));
            _lockResponseParser = responseParser;
            return this;
        }

        private static HttpClient ConfigureHttpClient(WebDavClientParams @params)
        {
            HttpMessageHandler httpMessageHandler = @params.HttpMessageHandler;
            if (httpMessageHandler == null)
            {
                var httpHandler = new HttpClientHandler();

                // Fixes for Blazor WASM
                if (!RuntimeUtils.IsBlazorWASM)
                {
                    httpHandler.UseDefaultCredentials = @params.UseDefaultCredentials;
                    httpHandler.PreAuthenticate = @params.PreAuthenticate;
                    httpHandler.UseProxy = @params.UseProxy;

                    if (@params.Credentials != null)
                    {
                        httpHandler.Credentials = @params.Credentials;
                    }

                    if (@params.Proxy != null)
                    {
                        httpHandler.Proxy = @params.Proxy;
                    }
                }

                // Fix for Blazor WASM
                if (httpHandler.SupportsAutomaticDecompression)
                {
                    httpHandler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
				}

                if (@params.IgnoreSslCertificateErrors)
	                httpHandler.ServerCertificateCustomValidationCallback += IgnoreServerCertificateErrors;

				httpMessageHandler = httpHandler;
            }

            HttpClient httpClient;
            if (RuntimeUtils.IsBlazorWASM)
            {
                httpClient = new HttpClient
                {
                    BaseAddress = @params.BaseAddress,
                    Timeout = @params.Timeout
                };
            }
            else
            {
                httpClient = new HttpClient(httpMessageHandler, true)
                {
                    BaseAddress = @params.BaseAddress,
                    Timeout = @params.Timeout
                };
            }

            foreach (var header in @params.DefaultRequestHeaders)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            return httpClient;
        }

        private static bool IgnoreServerCertificateErrors(HttpRequestMessage arg1, X509Certificate2 arg2, X509Chain arg3, SslPolicyErrors arg4)
        {
	        return true;
        }

        private static Uri CreateUri(string requestUri)
        {
            return !string.IsNullOrEmpty(requestUri) ? new Uri(requestUri, UriKind.RelativeOrAbsolute) : null;
        }

        private static Exception CreateInvalidUriException()
        {
            return new InvalidOperationException("An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");
        }

        private static Encoding GetResponseEncoding(HttpContent content)
        {
            if (content.Headers.ContentType?.CharSet == null)
            {
                return FallbackEncoding;
            }

            try
            {
                return Encoding.GetEncoding(content.Headers.ContentType.CharSet);
            }
            catch (ArgumentException)
            {
                return FallbackEncoding;
            }
        }
        
		private static async Task<string> ReadContentAsString(HttpContent content)
        {
            byte[] bytes = await content.ReadAsByteArrayAsync();
            Encoding encoding = GetResponseEncoding(content);

#if NETSTANDARD1_1 || NETSTANDARD1_2 || PORTABLE
            return encoding.GetString(bytes, 0, bytes.Length);
#else
            return encoding.GetString(bytes);
#endif
        }

        private Uri GetAbsoluteUri(Uri uri)
        {
            if (uri == null && _dispatcher.BaseAddress == null)
            {
                throw CreateInvalidUriException();
            }

            if (uri == null)
            {
                return _dispatcher.BaseAddress;
            }

            if (uri.IsAbsoluteUri)
            {
                return uri;
            }

            if (_dispatcher.BaseAddress == null)
            {
                throw CreateInvalidUriException();
            }

            return new Uri(_dispatcher.BaseAddress, uri);
        }

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed/unmanaged resources.
        /// Disposes the underlying HttpClient.
        /// </summary>
        public void Dispose()
        {
            DisposeManagedResources();
        }

        /// <summary>
        /// Disposes the managed resources.
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
            var disposableDispatcher = _dispatcher as IDisposable;
            if (disposableDispatcher != null)
            {
                disposableDispatcher.Dispose();
            }
        }

        #endregion
    }
}
