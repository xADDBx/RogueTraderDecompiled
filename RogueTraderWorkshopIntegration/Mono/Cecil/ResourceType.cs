using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public enum ResourceType
{
	Linked,
	Embedded,
	AssemblyLinked
}
