using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public class FieldReference : MemberReference
{
	private TypeReference field_type;

	public TypeReference FieldType
	{
		get
		{
			return field_type;
		}
		set
		{
			field_type = value;
		}
	}

	public override string FullName => field_type.FullName + " " + MemberFullName();

	public override bool ContainsGenericParameter
	{
		get
		{
			if (!field_type.ContainsGenericParameter)
			{
				return base.ContainsGenericParameter;
			}
			return true;
		}
	}

	internal FieldReference()
	{
		token = new MetadataToken(TokenType.MemberRef);
	}

	public FieldReference(string name, TypeReference fieldType)
		: base(name)
	{
		Mixin.CheckType(fieldType, Mixin.Argument.fieldType);
		field_type = fieldType;
		token = new MetadataToken(TokenType.MemberRef);
	}

	public FieldReference(string name, TypeReference fieldType, TypeReference declaringType)
		: this(name, fieldType)
	{
		Mixin.CheckType(declaringType, Mixin.Argument.declaringType);
		DeclaringType = declaringType;
	}

	protected override IMemberDefinition ResolveDefinition()
	{
		return Resolve();
	}

	public new virtual FieldDefinition Resolve()
	{
		return (Module ?? throw new NotSupportedException()).Resolve(this);
	}
}
