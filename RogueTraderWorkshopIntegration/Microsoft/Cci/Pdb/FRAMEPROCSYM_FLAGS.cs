using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum FRAMEPROCSYM_FLAGS : uint
{
	fHasAlloca = 1u,
	fHasSetJmp = 2u,
	fHasLongJmp = 4u,
	fHasInlAsm = 8u,
	fHasEH = 0x10u,
	fInlSpec = 0x20u,
	fHasSEH = 0x40u,
	fNaked = 0x80u,
	fSecurityChecks = 0x100u,
	fAsyncEH = 0x200u,
	fGSNoStackOrdering = 0x400u,
	fWasInlined = 0x800u
}
