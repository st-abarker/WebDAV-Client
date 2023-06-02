namespace WebDav
{
    /// <summary>
    /// Contains classes to specify AppltTo for concrete methods.
    /// </summary>
    public static class ApplyTo
    {
        /// <summary>
        /// Specifies whether the PROPFIND method is to be applied only to the resource, to the resource and its internal members only, or the resource and all its members.
        /// It corresponds to the WebDAV Depth header.
        /// </summary>
        public enum Propfind
        {
            ResourceOnly,
            ResourceAndChildren,
            ResourceAndAllDescendants
        }

        /// <summary>
        /// Specifies whether the COPY method is to be applied only to the resource or the resource and all its members.
        /// It corresponds to the WebDAV Depth header.
        /// </summary>
        public enum Copy
        {
            ResourceOnly,
            ResourceAndAllDescendants
        }

        /// <summary>
        /// Specifies whether the LOCK method is to be applied only to the resource or the resource and all its members.
        /// It corresponds to the WebDAV Depth header.
        /// </summary>
        public enum Lock
        {
            ResourceOnly,
            ResourceAndAllDescendants
        }
    }
}
