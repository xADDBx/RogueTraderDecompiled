using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum CV_GENERIC_FLAG : ushort
{
	cstyle = 1,
	rsclean = 2
}
