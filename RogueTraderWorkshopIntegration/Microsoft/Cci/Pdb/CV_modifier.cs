using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum CV_modifier : ushort
{
	MOD_const = 1,
	MOD_volatile = 2,
	MOD_unaligned = 4
}
