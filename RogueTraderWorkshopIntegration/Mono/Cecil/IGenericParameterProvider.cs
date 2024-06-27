using System.Runtime.InteropServices;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IGenericParameterProvider : IMetadataTokenProvider
{
	bool HasGenericParameters { get; }

	bool IsDefinition { get; }

	ModuleDefinition Module { get; }

	Collection<GenericParameter> GenericParameters { get; }

	GenericParameterType GenericParameterType { get; }
}
