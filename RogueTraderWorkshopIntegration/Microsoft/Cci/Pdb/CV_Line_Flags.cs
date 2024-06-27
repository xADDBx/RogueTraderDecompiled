using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum CV_Line_Flags : uint
{
	linenumStart = 0xFFFFFFu,
	deltaLineEnd = 0x7F000000u,
	fStatement = 0x80000000u
}
