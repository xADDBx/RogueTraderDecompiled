using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Mono.Cecil;

[Serializable]
[ComVisible(false)]
public sealed class AssemblyResolutionException : FileNotFoundException
{
	private readonly AssemblyNameReference reference;

	public AssemblyNameReference AssemblyReference => reference;

	public AssemblyResolutionException(AssemblyNameReference reference)
		: this(reference, null)
	{
	}

	public AssemblyResolutionException(AssemblyNameReference reference, Exception innerException)
		: base($"Failed to resolve assembly: '{reference}'", innerException)
	{
		this.reference = reference;
	}

	private AssemblyResolutionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
