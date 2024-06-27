using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IMetadataTokenProvider
{
	MetadataToken MetadataToken { get; set; }
}
