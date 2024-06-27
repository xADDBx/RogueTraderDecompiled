using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum COMPILESYM_FLAGS : uint
{
	iLanguage = 0xFFu,
	fEC = 0x100u,
	fNoDbgInfo = 0x200u,
	fLTCG = 0x400u,
	fNoDataAlign = 0x800u,
	fManagedPresent = 0x1000u,
	fSecurityChecks = 0x2000u,
	fHotPatch = 0x4000u,
	fCVTCIL = 0x8000u,
	fMSILModule = 0x10000u
}
