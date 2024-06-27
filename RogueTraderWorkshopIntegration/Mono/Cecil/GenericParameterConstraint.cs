using System.Runtime.InteropServices;
using System.Threading;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class GenericParameterConstraint : ICustomAttributeProvider, IMetadataTokenProvider
{
	internal GenericParameter generic_parameter;

	internal MetadataToken token;

	private TypeReference constraint_type;

	private Collection<CustomAttribute> custom_attributes;

	public TypeReference ConstraintType
	{
		get
		{
			return constraint_type;
		}
		set
		{
			constraint_type = value;
		}
	}

	public bool HasCustomAttributes
	{
		get
		{
			if (custom_attributes != null)
			{
				return custom_attributes.Count > 0;
			}
			if (generic_parameter == null)
			{
				return false;
			}
			return this.GetHasCustomAttributes(generic_parameter.Module);
		}
	}

	public Collection<CustomAttribute> CustomAttributes
	{
		get
		{
			if (generic_parameter == null)
			{
				if (custom_attributes == null)
				{
					Interlocked.CompareExchange(ref custom_attributes, new Collection<CustomAttribute>(), null);
				}
				return custom_attributes;
			}
			return custom_attributes ?? this.GetCustomAttributes(ref custom_attributes, generic_parameter.Module);
		}
	}

	public MetadataToken MetadataToken
	{
		get
		{
			return token;
		}
		set
		{
			token = value;
		}
	}

	internal GenericParameterConstraint(TypeReference constraintType, MetadataToken token)
	{
		constraint_type = constraintType;
		this.token = token;
	}

	public GenericParameterConstraint(TypeReference constraintType)
	{
		Mixin.CheckType(constraintType, Mixin.Argument.constraintType);
		constraint_type = constraintType;
		token = new MetadataToken(TokenType.GenericParamConstraint);
	}
}
