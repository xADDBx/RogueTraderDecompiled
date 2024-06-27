using System.Runtime.InteropServices;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public interface ICustomAttributeProvider : IMetadataTokenProvider
{
	Collection<CustomAttribute> CustomAttributes { get; }

	bool HasCustomAttributes { get; }
}
