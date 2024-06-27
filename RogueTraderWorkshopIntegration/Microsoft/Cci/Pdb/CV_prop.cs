using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum CV_prop : ushort
{
	packed = 1,
	ctor = 2,
	ovlops = 4,
	isnested = 8,
	cnested = 0x10,
	opassign = 0x20,
	opcast = 0x40,
	fwdref = 0x80,
	scoped = 0x100
}
