using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IConstantProvider : IMetadataTokenProvider
{
	bool HasConstant { get; set; }

	object Constant { get; set; }
}
