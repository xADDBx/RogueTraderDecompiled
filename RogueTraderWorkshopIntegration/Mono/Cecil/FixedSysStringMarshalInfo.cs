using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class FixedSysStringMarshalInfo : MarshalInfo
{
	internal int size;

	public int Size
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
		}
	}

	public FixedSysStringMarshalInfo()
		: base(NativeType.FixedSysString)
	{
		size = -1;
	}
}
