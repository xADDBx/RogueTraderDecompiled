using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IAssemblyResolver : IDisposable
{
	AssemblyDefinition Resolve(AssemblyNameReference name);

	AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters);
}
