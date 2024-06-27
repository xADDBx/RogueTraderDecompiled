using System;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Cecil.Metadata;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class GenericParameter : TypeReference, ICustomAttributeProvider, IMetadataTokenProvider
{
	internal int position;

	internal GenericParameterType type;

	internal IGenericParameterProvider owner;

	private ushort attributes;

	private GenericParameterConstraintCollection constraints;

	private Collection<CustomAttribute> custom_attributes;

	public GenericParameterAttributes Attributes
	{
		get
		{
			return (GenericParameterAttributes)attributes;
		}
		set
		{
			attributes = (ushort)value;
		}
	}

	public int Position => position;

	public GenericParameterType Type => type;

	public IGenericParameterProvider Owner => owner;

	public bool HasConstraints
	{
		get
		{
			if (constraints != null)
			{
				return constraints.Count > 0;
			}
			if (base.HasImage)
			{
				return Module.Read(this, (GenericParameter generic_parameter, MetadataReader reader) => reader.HasGenericConstraints(generic_parameter));
			}
			return false;
		}
	}

	public Collection<GenericParameterConstraint> Constraints
	{
		get
		{
			if (constraints != null)
			{
				return constraints;
			}
			if (base.HasImage)
			{
				return Module.Read(ref constraints, this, (GenericParameter generic_parameter, MetadataReader reader) => reader.ReadGenericConstraints(generic_parameter));
			}
			Interlocked.CompareExchange(ref constraints, new GenericParameterConstraintCollection(this), null);
			return constraints;
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
			return this.GetHasCustomAttributes(Module);
		}
	}

	public Collection<CustomAttribute> CustomAttributes => custom_attributes ?? this.GetCustomAttributes(ref custom_attributes, Module);

	public override IMetadataScope Scope
	{
		get
		{
			if (owner == null)
			{
				return null;
			}
			if (owner.GenericParameterType != GenericParameterType.Method)
			{
				return ((TypeReference)owner).Scope;
			}
			return ((MethodReference)owner).DeclaringType.Scope;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public override TypeReference DeclaringType
	{
		get
		{
			return owner as TypeReference;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public MethodReference DeclaringMethod => owner as MethodReference;

	public override ModuleDefinition Module => module ?? owner.Module;

	public override string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(base.Name))
			{
				return base.Name;
			}
			return base.Name = ((type == GenericParameterType.Method) ? "!!" : "!") + position;
		}
	}

	public override string Namespace
	{
		get
		{
			return string.Empty;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public override string FullName => Name;

	public override bool IsGenericParameter => true;

	public override bool ContainsGenericParameter => true;

	public override MetadataType MetadataType => (MetadataType)etype;

	public bool IsNonVariant
	{
		get
		{
			return attributes.GetMaskedAttributes(3, 0u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(3, 0u, value);
		}
	}

	public bool IsCovariant
	{
		get
		{
			return attributes.GetMaskedAttributes(3, 1u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(3, 1u, value);
		}
	}

	public bool IsContravariant
	{
		get
		{
			return attributes.GetMaskedAttributes(3, 2u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(3, 2u, value);
		}
	}

	public bool HasReferenceTypeConstraint
	{
		get
		{
			return attributes.GetAttributes(4);
		}
		set
		{
			attributes = attributes.SetAttributes(4, value);
		}
	}

	public bool HasNotNullableValueTypeConstraint
	{
		get
		{
			return attributes.GetAttributes(8);
		}
		set
		{
			attributes = attributes.SetAttributes(8, value);
		}
	}

	public bool HasDefaultConstructorConstraint
	{
		get
		{
			return attributes.GetAttributes(16);
		}
		set
		{
			attributes = attributes.SetAttributes(16, value);
		}
	}

	public GenericParameter(IGenericParameterProvider owner)
		: this(string.Empty, owner)
	{
	}

	public GenericParameter(string name, IGenericParameterProvider owner)
		: base(string.Empty, name)
	{
		if (owner == null)
		{
			throw new ArgumentNullException();
		}
		position = -1;
		this.owner = owner;
		type = owner.GenericParameterType;
		etype = ConvertGenericParameterType(type);
		token = new MetadataToken(TokenType.GenericParam);
	}

	internal GenericParameter(int position, GenericParameterType type, ModuleDefinition module)
		: base(string.Empty, string.Empty)
	{
		Mixin.CheckModule(module);
		this.position = position;
		this.type = type;
		etype = ConvertGenericParameterType(type);
		base.module = module;
		token = new MetadataToken(TokenType.GenericParam);
	}

	private static ElementType ConvertGenericParameterType(GenericParameterType type)
	{
		return type switch
		{
			GenericParameterType.Type => ElementType.Var, 
			GenericParameterType.Method => ElementType.MVar, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public override TypeDefinition Resolve()
	{
		return null;
	}
}
