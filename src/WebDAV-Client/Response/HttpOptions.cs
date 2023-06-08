using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDav.Client.Response
{
	[Flags]
	public enum HttpMethods
	{
		None = 0,

		ACL = 1 << 1,
		BDelete = 1 << 2,
		BPropPatch = 1 << 3,
		Connect = 1 << 4,
		Copy = 1 << 5,
		Delete = 1 << 6,
		Get = 1 << 7,
		Head = 1 << 8,
		Lock = 1 << 9,
		MkCalendar = 1 << 10,
		MkCol = 1 << 11,
		MkColExtended = 1 << 12,
		Move = 1 << 13,
		Options = 1 << 14,
		Patch = 1 << 15,
		Poll = 1 << 16,
		Post = 1 << 17,
		PropFind = 1 << 18,
		PropPatch = 1 << 19,
		Put = 1 << 20,
		Report = 1 << 21,
		Search = 1 << 22,
		Subscribe = 1 << 23,
		Trace = 1 << 24,
		Unlock = 1 << 25,
		Unsubscribe = 1 << 26,
	}
}
