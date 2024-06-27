using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[Flags]
[ComVisible(false)]
public enum EventAttributes : ushort
{
	None = 0,
	SpecialName = 0x200,
	RTSpecialName = 0x400
}
