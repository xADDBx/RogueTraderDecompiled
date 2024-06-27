using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IMetadataResolver
{
	TypeDefinition Resolve(TypeReference type);

	FieldDefinition Resolve(FieldReference field);

	MethodDefinition Resolve(MethodReference method);
}
