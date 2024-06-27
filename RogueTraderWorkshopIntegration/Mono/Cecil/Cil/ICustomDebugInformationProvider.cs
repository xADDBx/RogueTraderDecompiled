using System.Runtime.InteropServices;
using Mono.Collections.Generic;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public interface ICustomDebugInformationProvider : IMetadataTokenProvider
{
	bool HasCustomDebugInformations { get; }

	Collection<CustomDebugInformation> CustomDebugInformations { get; }
}
