using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public class MarshalInfo
{
	internal NativeType native;

	public NativeType NativeType
	{
		get
		{
			return native;
		}
		set
		{
			native = value;
		}
	}

	public MarshalInfo(NativeType native)
	{
		this.native = native;
	}
}
