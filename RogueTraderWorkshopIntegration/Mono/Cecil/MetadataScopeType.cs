using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public enum MetadataScopeType
{
	AssemblyNameReference,
	ModuleReference,
	ModuleDefinition
}
