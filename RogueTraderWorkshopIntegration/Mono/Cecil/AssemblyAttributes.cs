using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[Flags]
[ComVisible(false)]
public enum AssemblyAttributes : uint
{
	PublicKey = 1u,
	SideBySideCompatible = 0u,
	Retargetable = 0x100u,
	WindowsRuntime = 0x200u,
	DisableJITCompileOptimizer = 0x4000u,
	EnableJITCompileTracking = 0x8000u
}
