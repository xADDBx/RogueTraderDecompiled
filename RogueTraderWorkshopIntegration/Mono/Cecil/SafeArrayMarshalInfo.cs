using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class SafeArrayMarshalInfo : MarshalInfo
{
	internal VariantType element_type;

	public VariantType ElementType
	{
		get
		{
			return element_type;
		}
		set
		{
			element_type = value;
		}
	}

	public SafeArrayMarshalInfo()
		: base(NativeType.SafeArray)
	{
		element_type = VariantType.None;
	}
}
