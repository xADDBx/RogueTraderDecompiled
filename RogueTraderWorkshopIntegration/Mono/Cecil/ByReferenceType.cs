using System;
using System.Runtime.InteropServices;
using Mono.Cecil.Metadata;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class ByReferenceType : TypeSpecification
{
	public override string Name => base.Name + "&";

	public override string FullName => base.FullName + "&";

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

	public override bool IsByReference => true;

	public ByReferenceType(TypeReference type)
		: base(type)
	{
		Mixin.CheckType(type);
		etype = Mono.Cecil.Metadata.ElementType.ByRef;
	}
}
