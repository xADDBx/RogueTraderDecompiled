using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[Flags]
[ComVisible(false)]
public enum VariableAttributes : ushort
{
	None = 0,
	DebuggerHidden = 1
}
