using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum CV_LVARFLAGS : ushort
{
	fIsParam = 1,
	fAddrTaken = 2,
	fCompGenx = 4,
	fIsAggregate = 8,
	fIsAggregated = 0x10,
	fIsAliased = 0x20,
	fIsAlias = 0x40
}
