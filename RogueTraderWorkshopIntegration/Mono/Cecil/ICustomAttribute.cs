using System.Runtime.InteropServices;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public interface ICustomAttribute
{
	TypeReference AttributeType { get; }

	bool HasFields { get; }

	bool HasProperties { get; }

	bool HasConstructorArguments { get; }

	Collection<CustomAttributeNamedArgument> Fields { get; }

	Collection<CustomAttributeNamedArgument> Properties { get; }

	Collection<CustomAttributeArgument> ConstructorArguments { get; }
}
