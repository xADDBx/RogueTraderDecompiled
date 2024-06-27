using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum CFLAGSYM_FLAGS : ushort
{
	pcode = 1,
	floatprec = 6,
	floatpkg = 0x18,
	ambdata = 0xE0,
	ambcode = 0x700,
	mode32 = 0x800
}
