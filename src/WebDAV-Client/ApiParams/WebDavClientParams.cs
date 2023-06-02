﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace WebDav
{
    /// <summary>
    /// Represents parameters for the <see cref="WebDavClient"/> class.
    /// </summary>
    public class WebDavClientParams
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavClientParams"/> class.
        /// </summary>
        public WebDavClientParams()
        {
            UseDefaultCredentials = true;
            DefaultRequestHeaders = new Dictionary<string, string>();
            Timeout = TimeSpan.FromMilliseconds(-1); // Infinite timespan is nothing but TimeSpan with milliseconds set to -1
            UseProxy = true;
            PreAuthenticate = true;
        }

        /// <summary>
        /// Gets or sets the HTTP message handler. Note that this overrides some other properties in this class like:
        /// - PreAuthenticate
        /// - UseDefaultCredentials
        /// - UseProxy
        /// - Credentials
        /// - Proxy
        /// </summary>
        public HttpMessageHandler HttpMessageHandler { get; set; }

        /// <summary>
        /// Gets or sets a value that controls whether default credentials are sent.
        /// </summary>
        /// <value>
        /// <c>true</c> if the default credentials are used; otherwise <c>false</c>. The default value is <c>true</c>.
        /// </value>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// Gets or sets the base address of the resource's URI used when sending requests.
        /// </summary>
        public Uri BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets authentication information used by the WebDavClient.
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the headers which should be sent with each request.
        /// </summary>
        public IDictionary<string, string> DefaultRequestHeaders { get; set; }

        /// <summary>
        ///  Gets or sets a flag indicating whether the client should ignore issues with the SSL security certificate.
        /// </summary>
        public bool IgnoreSslCertificateErrors { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether an Authorization header should be sent with the request.
        /// </summary>
        /// <value>
        ///   <c>true</c> if an HTTP Authorization header should be send with requests after authentication has taken place; otherwise, <c>false</c>. The default value is <c>true</c>.
        /// </value>
        public bool PreAuthenticate { get; set; }

        /// <summary>
        /// Gets or sets proxy information used by the WebDavClient.
        /// </summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>
        /// Gets or sets a timeout for WebDAV operations.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a proxy should be used for requests.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a proxy should be used for requests; otherwise, <c>false</c>. The default value is <c>true</c>.
        /// </value>
        public bool UseProxy { get; set; }
    }
}
