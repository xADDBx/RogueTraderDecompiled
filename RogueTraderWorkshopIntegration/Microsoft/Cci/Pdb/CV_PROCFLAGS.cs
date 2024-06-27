using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum CV_PROCFLAGS : byte
{
	CV_PFLAG_NOFPO = 1,
	CV_PFLAG_INT = 2,
	CV_PFLAG_FAR = 4,
	CV_PFLAG_NEVER = 8,
	CV_PFLAG_NOTREACHED = 0x10,
	CV_PFLAG_CUST_CALL = 0x20,
	CV_PFLAG_NOINLINE = 0x40,
	CV_PFLAG_OPTDBGINFO = 0x80
}
