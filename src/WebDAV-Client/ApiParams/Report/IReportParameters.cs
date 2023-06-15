using System.Threading;

namespace WebDav.Report
{
	/// <summary>
	/// Represents parameters for the REPORT WebDAV method
	/// </summary>
	public interface IReportParameters
	{
		CancellationToken CancellationToken { get; set; }
	}
}
