using System;

namespace Mono.Cecil;

[Flags]
internal enum TypeDefinitionTreatment
{
	None = 0,
	KindMask = 0xF,
	NormalType = 1,
	NormalAttribute = 2,
	UnmangleWindowsRuntimeName = 3,
	PrefixWindowsRuntimeName = 4,
	RedirectToClrType = 5,
	RedirectToClrAttribute = 6,
	RedirectImplementedMethods = 7,
	Abstract = 0x10,
	Internal = 0x20
}
