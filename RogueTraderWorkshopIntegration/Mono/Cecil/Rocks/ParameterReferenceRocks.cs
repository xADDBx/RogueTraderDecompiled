using System.Runtime.InteropServices;

namespace Mono.Cecil.Rocks;

[ComVisible(false)]
public static class ParameterReferenceRocks
{
	public static int GetSequence(this ParameterReference self)
	{
		return self.Index + 1;
	}
}
