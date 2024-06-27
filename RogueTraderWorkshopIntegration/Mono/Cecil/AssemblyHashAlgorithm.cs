using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public enum AssemblyHashAlgorithm : uint
{
	None = 0u,
	MD5 = 32771u,
	SHA1 = 32772u,
	SHA256 = 32780u,
	SHA384 = 32781u,
	SHA512 = 32782u,
	Reserved = 32771u
}
