﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WebDav
{
    /// <summary>
    /// Represents a response of the PROPPATCH operation.
    /// </summary>
    public class ProppatchResponse : WebDavResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProppatchResponse"/> class.
        /// </summary>
        /// <param name="statusCode">The status code of the operation.</param>
        public ProppatchResponse(int statusCode)
            : this(statusCode, null, new List<WebDavPropertyStatus>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProppatchResponse"/> class.
        /// </summary>
        /// <param name="statusCode">The status code of the response.</param>
        /// <param name="propertyStatuses">The collection of property statuses.</param>
        public ProppatchResponse(int statusCode, IEnumerable<WebDavPropertyStatus> propertyStatuses)
            : this(statusCode, null, propertyStatuses)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProppatchResponse"/> class.
        /// </summary>
        /// <param name="statusCode">The status code of the response.</param>
        /// <param name="description">The description of the response.</param>
        public ProppatchResponse(int statusCode, string description)
            : this(statusCode, description, new List<WebDavPropertyStatus>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProppatchResponse"/> class.
        /// </summary>
        /// <param name="statusCode">The status code of the response.</param>
        /// <param name="description">The description of the response.</param>
        /// <param name="propertyStatuses">The collection of property statuses.</param>
        public ProppatchResponse(int statusCode, string description, [DisallowNull] IEnumerable<WebDavPropertyStatus> propertyStatuses)
            : base(statusCode, description)
		{
			if (propertyStatuses is null)
				throw new ArgumentNullException(nameof(propertyStatuses));
            PropertyStatuses = new List<WebDavPropertyStatus>(propertyStatuses);
        }

        /// <summary>
        /// Gets the collection statuses of set/delete operation on the resource's properties.
        /// </summary>
        public IReadOnlyCollection<WebDavPropertyStatus> PropertyStatuses { get; private set; }

        public override string ToString()
        {
            return string.Format("PROPPATCH WebDAV response - StatusCode: {0}, Description: {1}", StatusCode, Description);
        }
    }
}
