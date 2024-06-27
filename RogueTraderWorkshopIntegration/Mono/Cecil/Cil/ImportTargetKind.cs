using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public enum ImportTargetKind : byte
{
	ImportNamespace = 1,
	ImportNamespaceInAssembly,
	ImportType,
	ImportXmlNamespaceWithAlias,
	ImportAlias,
	DefineAssemblyAlias,
	DefineNamespaceAlias,
	DefineNamespaceInAssemblyAlias,
	DefineTypeAlias
}
