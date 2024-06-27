using System;

namespace Mono.Cecil;

[Flags]
internal enum MethodDefinitionTreatment
{
	None = 0,
	Abstract = 2,
	Private = 4,
	Public = 8,
	Runtime = 0x10,
	InternalCall = 0x20
}
