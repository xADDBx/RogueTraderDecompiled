using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IMemberDefinition : ICustomAttributeProvider, IMetadataTokenProvider
{
	string Name { get; set; }

	string FullName { get; }

	bool IsSpecialName { get; set; }

	bool IsRuntimeSpecialName { get; set; }

	TypeDefinition DeclaringType { get; set; }
}
