using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public enum DocumentHashAlgorithm
{
	None,
	MD5,
	SHA1,
	SHA256
}
