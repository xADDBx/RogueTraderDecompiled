using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum LeafPointerAttr : uint
{
	ptrtype = 0x1Fu,
	ptrmode = 0xE0u,
	isflat32 = 0x100u,
	isvolatile = 0x200u,
	isconst = 0x400u,
	isunaligned = 0x800u,
	isrestrict = 0x1000u
}
