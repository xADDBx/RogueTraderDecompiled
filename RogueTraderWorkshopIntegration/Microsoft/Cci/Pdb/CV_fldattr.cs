using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum CV_fldattr
{
	access = 3,
	mprop = 0x1C,
	pseudo = 0x20,
	noinherit = 0x40,
	noconstruct = 0x80,
	compgenx = 0x100
}
