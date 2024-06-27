using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IMarshalInfoProvider : IMetadataTokenProvider
{
	bool HasMarshalInfo { get; }

	MarshalInfo MarshalInfo { get; set; }
}
