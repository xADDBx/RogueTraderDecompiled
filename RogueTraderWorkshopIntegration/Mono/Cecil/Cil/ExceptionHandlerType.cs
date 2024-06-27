using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public enum ExceptionHandlerType
{
	Catch = 0,
	Filter = 1,
	Finally = 2,
	Fault = 4
}
