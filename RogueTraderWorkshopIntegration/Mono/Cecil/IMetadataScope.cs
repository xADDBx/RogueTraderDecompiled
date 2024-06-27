using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IMetadataScope : IMetadataTokenProvider
{
	MetadataScopeType MetadataScopeType { get; }

	string Name { get; set; }
}
