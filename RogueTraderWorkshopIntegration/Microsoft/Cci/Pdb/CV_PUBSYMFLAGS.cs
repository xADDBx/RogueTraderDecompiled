using System;

namespace Microsoft.Cci.Pdb;

[Flags]
internal enum CV_PUBSYMFLAGS : uint
{
	fNone = 0u,
	fCode = 1u,
	fFunction = 2u,
	fManaged = 4u,
	fMSIL = 8u
}
