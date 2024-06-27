using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public delegate AssemblyDefinition AssemblyResolveEventHandler(object sender, AssemblyNameReference reference);
