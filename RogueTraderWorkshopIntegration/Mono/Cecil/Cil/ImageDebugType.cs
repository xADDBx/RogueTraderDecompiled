using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public enum ImageDebugType
{
	CodeView = 2,
	Deterministic = 16,
	EmbeddedPortablePdb = 17,
	PdbChecksum = 19
}
