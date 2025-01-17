﻿using System;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace WebDav
{
    /// <summary>
    /// Represents parameters for the UNLOCK WebDAV method.
    /// </summary>
    public class UnlockParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnlockParameters"/> class.
        /// <param name="lockToken">The resource lock token.</param>
        /// </summary>
        public UnlockParameters([DisallowNull] string lockToken)
		{
			if (lockToken is null)
				throw new ArgumentNullException(nameof(lockToken));

			LockToken = lockToken;
            CancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// Gets the resource lock token.
        /// </summary>
        public string LockToken { get; private set; }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
    }
}
