using System;
using System.Runtime.InteropServices;
using Mono.Cecil.Metadata;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class PointerType : TypeSpecification
{
	public override string Name => base.Name + "*";

	public override string FullName => base.FullName + "*";

	public override bool IsValueType
	{
		get
		{
			return false;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public override bool IsPointer => true;

	public PointerType(TypeReference type)
		: base(type)
	{
		Mixin.CheckType(type);
		etype = Mono.Cecil.Metadata.ElementType.Ptr;
	}
}
