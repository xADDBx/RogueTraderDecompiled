using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public enum ModuleKind
{
	Dll,
	Console,
	Windows,
	NetModule
}
