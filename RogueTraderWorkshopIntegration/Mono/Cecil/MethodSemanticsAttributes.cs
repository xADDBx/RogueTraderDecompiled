using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[Flags]
[ComVisible(false)]
public enum MethodSemanticsAttributes : ushort
{
	None = 0,
	Setter = 1,
	Getter = 2,
	Other = 4,
	AddOn = 8,
	RemoveOn = 0x10,
	Fire = 0x20
}
