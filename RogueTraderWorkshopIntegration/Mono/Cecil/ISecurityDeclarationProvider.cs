using System.Runtime.InteropServices;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public interface ISecurityDeclarationProvider : IMetadataTokenProvider
{
	bool HasSecurityDeclarations { get; }

	Collection<SecurityDeclaration> SecurityDeclarations { get; }
}
