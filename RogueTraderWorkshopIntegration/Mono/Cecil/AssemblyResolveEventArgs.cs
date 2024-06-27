using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class AssemblyResolveEventArgs : EventArgs
{
	private readonly AssemblyNameReference reference;

	public AssemblyNameReference AssemblyReference => reference;

	public AssemblyResolveEventArgs(AssemblyNameReference reference)
	{
		this.reference = reference;
	}
}
