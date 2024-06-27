using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[Flags]
[ComVisible(false)]
public enum PropertyAttributes : ushort
{
	None = 0,
	SpecialName = 0x200,
	RTSpecialName = 0x400,
	HasDefault = 0x1000,
	Unused = 0xE9FF
}
