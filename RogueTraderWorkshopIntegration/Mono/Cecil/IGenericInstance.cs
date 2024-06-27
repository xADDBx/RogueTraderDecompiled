using System.Runtime.InteropServices;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IGenericInstance : IMetadataTokenProvider
{
	bool HasGenericArguments { get; }

	Collection<TypeReference> GenericArguments { get; }
}
