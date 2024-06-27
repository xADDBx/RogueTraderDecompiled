using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public enum FlowControl
{
	Branch,
	Break,
	Call,
	Cond_Branch,
	Meta,
	Next,
	Phi,
	Return,
	Throw
}
