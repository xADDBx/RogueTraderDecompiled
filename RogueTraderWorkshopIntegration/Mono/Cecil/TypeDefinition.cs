using System;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Cecil.Metadata;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class TypeDefinition : TypeReference, IMemberDefinition, ICustomAttributeProvider, IMetadataTokenProvider, ISecurityDeclarationProvider
{
	private uint attributes;

	private TypeReference base_type;

	internal Range fields_range;

	internal Range methods_range;

	private short packing_size = -2;

	private int class_size = -2;

	private InterfaceImplementationCollection interfaces;

	private Collection<TypeDefinition> nested_types;

	private Collection<MethodDefinition> methods;

	private Collection<FieldDefinition> fields;

	private Collection<EventDefinition> events;

	private Collection<PropertyDefinition> properties;

	private Collection<CustomAttribute> custom_attributes;

	private Collection<SecurityDeclaration> security_declarations;

	public TypeAttributes Attributes
	{
		get
		{
			return (TypeAttributes)attributes;
		}
		set
		{
			if (base.IsWindowsRuntimeProjection && (ushort)value != attributes)
			{
				throw new InvalidOperationException();
			}
			attributes = (uint)value;
		}
	}

	public TypeReference BaseType
	{
		get
		{
			return base_type;
		}
		set
		{
			base_type = value;
		}
	}

	public override string Name
	{
		get
		{
			return base.Name;
		}
		set
		{
			if (base.IsWindowsRuntimeProjection && value != base.Name)
			{
				throw new InvalidOperationException();
			}
			base.Name = value;
		}
	}

	public bool HasLayoutInfo
	{
		get
		{
			if (packing_size >= 0 || class_size >= 0)
			{
				return true;
			}
			ResolveLayout();
			if (packing_size < 0)
			{
				return class_size >= 0;
			}
			return true;
		}
	}

	public short PackingSize
	{
		get
		{
			if (packing_size >= 0)
			{
				return packing_size;
			}
			ResolveLayout();
			if (packing_size < 0)
			{
				return -1;
			}
			return packing_size;
		}
		set
		{
			packing_size = value;
		}
	}

	public int ClassSize
	{
		get
		{
			if (class_size >= 0)
			{
				return class_size;
			}
			ResolveLayout();
			if (class_size < 0)
			{
				return -1;
			}
			return class_size;
		}
		set
		{
			class_size = value;
		}
	}

	public bool HasInterfaces
	{
		get
		{
			if (interfaces != null)
			{
				return interfaces.Count > 0;
			}
			if (base.HasImage)
			{
				return Module.Read(this, (TypeDefinition type, MetadataReader reader) => reader.HasInterfaces(type));
			}
			return false;
		}
	}

	public Collection<InterfaceImplementation> Interfaces
	{
		get
		{
			if (interfaces != null)
			{
				return interfaces;
			}
			if (base.HasImage)
			{
				return Module.Read(ref interfaces, this, (TypeDefinition type, MetadataReader reader) => reader.ReadInterfaces(type));
			}
			Interlocked.CompareExchange(ref interfaces, new InterfaceImplementationCollection(this), null);
			return interfaces;
		}
	}

	public bool HasNestedTypes
	{
		get
		{
			if (nested_types != null)
			{
				return nested_types.Count > 0;
			}
			if (base.HasImage)
			{
				return Module.Read(this, (TypeDefinition type, MetadataReader reader) => reader.HasNestedTypes(type));
			}
			return false;
		}
	}

	public Collection<TypeDefinition> NestedTypes
	{
		get
		{
			if (nested_types != null)
			{
				return nested_types;
			}
			if (base.HasImage)
			{
				return Module.Read(ref nested_types, this, (TypeDefinition type, MetadataReader reader) => reader.ReadNestedTypes(type));
			}
			Interlocked.CompareExchange(ref nested_types, new MemberDefinitionCollection<TypeDefinition>(this), null);
			return nested_types;
		}
	}

	public bool HasMethods
	{
		get
		{
			if (methods != null)
			{
				return methods.Count > 0;
			}
			if (base.HasImage)
			{
				return methods_range.Length != 0;
			}
			return false;
		}
	}

	public Collection<MethodDefinition> Methods
	{
		get
		{
			if (methods != null)
			{
				return methods;
			}
			if (base.HasImage)
			{
				return Module.Read(ref methods, this, (TypeDefinition type, MetadataReader reader) => reader.ReadMethods(type));
			}
			Interlocked.CompareExchange(ref methods, new MemberDefinitionCollection<MethodDefinition>(this), null);
			return methods;
		}
	}

	public bool HasFields
	{
		get
		{
			if (fields != null)
			{
				return fields.Count > 0;
			}
			if (base.HasImage)
			{
				return fields_range.Length != 0;
			}
			return false;
		}
	}

	public Collection<FieldDefinition> Fields
	{
		get
		{
			if (fields != null)
			{
				return fields;
			}
			if (base.HasImage)
			{
				return Module.Read(ref fields, this, (TypeDefinition type, MetadataReader reader) => reader.ReadFields(type));
			}
			Interlocked.CompareExchange(ref fields, new MemberDefinitionCollection<FieldDefinition>(this), null);
			return fields;
		}
	}

	public bool HasEvents
	{
		get
		{
			if (events != null)
			{
				return events.Count > 0;
			}
			if (base.HasImage)
			{
				return Module.Read(this, (TypeDefinition type, MetadataReader reader) => reader.HasEvents(type));
			}
			return false;
		}
	}

	public Collection<EventDefinition> Events
	{
		get
		{
			if (events != null)
			{
				return events;
			}
			if (base.HasImage)
			{
				return Module.Read(ref events, this, (TypeDefinition type, MetadataReader reader) => reader.ReadEvents(type));
			}
			Interlocked.CompareExchange(ref events, new MemberDefinitionCollection<EventDefinition>(this), null);
			return events;
		}
	}

	public bool HasProperties
	{
		get
		{
			if (properties != null)
			{
				return properties.Count > 0;
			}
			if (base.HasImage)
			{
				return Module.Read(this, (TypeDefinition type, MetadataReader reader) => reader.HasProperties(type));
			}
			return false;
		}
	}

	public Collection<PropertyDefinition> Properties
	{
		get
		{
			if (properties != null)
			{
				return properties;
			}
			if (base.HasImage)
			{
				return Module.Read(ref properties, this, (TypeDefinition type, MetadataReader reader) => reader.ReadProperties(type));
			}
			Interlocked.CompareExchange(ref properties, new MemberDefinitionCollection<PropertyDefinition>(this), null);
			return properties;
		}
	}

	public bool HasSecurityDeclarations
	{
		get
		{
			if (security_declarations != null)
			{
				return security_declarations.Count > 0;
			}
			return this.GetHasSecurityDeclarations(Module);
		}
	}

	public Collection<SecurityDeclaration> SecurityDeclarations => security_declarations ?? this.GetSecurityDeclarations(ref security_declarations, Module);

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

	public override bool HasGenericParameters
	{
		get
		{
			if (generic_parameters != null)
			{
				return generic_parameters.Count > 0;
			}
			return this.GetHasGenericParameters(Module);
		}
	}

	public override Collection<GenericParameter> GenericParameters => generic_parameters ?? this.GetGenericParameters(ref generic_parameters, Module);

	public bool IsNotPublic
	{
		get
		{
			return attributes.GetMaskedAttributes(7u, 0u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(7u, 0u, value);
		}
	}

	public bool IsPublic
	{
		get
		{
			return attributes.GetMaskedAttributes(7u, 1u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(7u, 1u, value);
		}
	}

	public bool IsNestedPublic
	{
		get
		{
			return attributes.GetMaskedAttributes(7u, 2u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(7u, 2u, value);
		}
	}

	public bool IsNestedPrivate
	{
		get
		{
			return attributes.GetMaskedAttributes(7u, 3u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(7u, 3u, value);
		}
	}

	public bool IsNestedFamily
	{
		get
		{
			return attributes.GetMaskedAttributes(7u, 4u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(7u, 4u, value);
		}
	}

	public bool IsNestedAssembly
	{
		get
		{
			return attributes.GetMaskedAttributes(7u, 5u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(7u, 5u, value);
		}
	}

	public bool IsNestedFamilyAndAssembly
	{
		get
		{
			return attributes.GetMaskedAttributes(7u, 6u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(7u, 6u, value);
		}
	}

	public bool IsNestedFamilyOrAssembly
	{
		get
		{
			return attributes.GetMaskedAttributes(7u, 7u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(7u, 7u, value);
		}
	}

	public bool IsAutoLayout
	{
		get
		{
			return attributes.GetMaskedAttributes(24u, 0u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(24u, 0u, value);
		}
	}

	public bool IsSequentialLayout
	{
		get
		{
			return attributes.GetMaskedAttributes(24u, 8u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(24u, 8u, value);
		}
	}

	public bool IsExplicitLayout
	{
		get
		{
			return attributes.GetMaskedAttributes(24u, 16u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(24u, 16u, value);
		}
	}

	public bool IsClass
	{
		get
		{
			return attributes.GetMaskedAttributes(32u, 0u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(32u, 0u, value);
		}
	}

	public bool IsInterface
	{
		get
		{
			return attributes.GetMaskedAttributes(32u, 32u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(32u, 32u, value);
		}
	}

	public bool IsAbstract
	{
		get
		{
			return attributes.GetAttributes(128u);
		}
		set
		{
			attributes = attributes.SetAttributes(128u, value);
		}
	}

	public bool IsSealed
	{
		get
		{
			return attributes.GetAttributes(256u);
		}
		set
		{
			attributes = attributes.SetAttributes(256u, value);
		}
	}

	public bool IsSpecialName
	{
		get
		{
			return attributes.GetAttributes(1024u);
		}
		set
		{
			attributes = attributes.SetAttributes(1024u, value);
		}
	}

	public bool IsImport
	{
		get
		{
			return attributes.GetAttributes(4096u);
		}
		set
		{
			attributes = attributes.SetAttributes(4096u, value);
		}
	}

	public bool IsSerializable
	{
		get
		{
			return attributes.GetAttributes(8192u);
		}
		set
		{
			attributes = attributes.SetAttributes(8192u, value);
		}
	}

	public bool IsWindowsRuntime
	{
		get
		{
			return attributes.GetAttributes(16384u);
		}
		set
		{
			attributes = attributes.SetAttributes(16384u, value);
		}
	}

	public bool IsAnsiClass
	{
		get
		{
			return attributes.GetMaskedAttributes(196608u, 0u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(196608u, 0u, value);
		}
	}

	public bool IsUnicodeClass
	{
		get
		{
			return attributes.GetMaskedAttributes(196608u, 65536u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(196608u, 65536u, value);
		}
	}

	public bool IsAutoClass
	{
		get
		{
			return attributes.GetMaskedAttributes(196608u, 131072u);
		}
		set
		{
			attributes = attributes.SetMaskedAttributes(196608u, 131072u, value);
		}
	}

	public bool IsBeforeFieldInit
	{
		get
		{
			return attributes.GetAttributes(1048576u);
		}
		set
		{
			attributes = attributes.SetAttributes(1048576u, value);
		}
	}

	public bool IsRuntimeSpecialName
	{
		get
		{
			return attributes.GetAttributes(2048u);
		}
		set
		{
			attributes = attributes.SetAttributes(2048u, value);
		}
	}

	public bool HasSecurity
	{
		get
		{
			return attributes.GetAttributes(262144u);
		}
		set
		{
			attributes = attributes.SetAttributes(262144u, value);
		}
	}

	public bool IsEnum
	{
		get
		{
			if (base_type != null)
			{
				return base_type.IsTypeOf("System", "Enum");
			}
			return false;
		}
	}

	public override bool IsValueType
	{
		get
		{
			if (base_type == null)
			{
				return false;
			}
			if (!base_type.IsTypeOf("System", "Enum"))
			{
				if (base_type.IsTypeOf("System", "ValueType"))
				{
					return !this.IsTypeOf("System", "Enum");
				}
				return false;
			}
			return true;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override bool IsPrimitive
	{
		get
		{
			if (MetadataSystem.TryGetPrimitiveElementType(this, out var self))
			{
				return self.IsPrimitive();
			}
			return false;
		}
	}

	public override MetadataType MetadataType
	{
		get
		{
			if (MetadataSystem.TryGetPrimitiveElementType(this, out var result))
			{
				return (MetadataType)result;
			}
			return base.MetadataType;
		}
	}

	public override bool IsDefinition => true;

	public new TypeDefinition DeclaringType
	{
		get
		{
			return (TypeDefinition)base.DeclaringType;
		}
		set
		{
			base.DeclaringType = value;
		}
	}

	internal new TypeDefinitionProjection WindowsRuntimeProjection
	{
		get
		{
			return (TypeDefinitionProjection)projection;
		}
		set
		{
			projection = value;
		}
	}

	private void ResolveLayout()
	{
		if (!base.HasImage)
		{
			packing_size = -1;
			class_size = -1;
			return;
		}
		lock (Module.SyncRoot)
		{
			if (packing_size == -2 && class_size == -2)
			{
				Row<short, int> row = Module.Read(this, (TypeDefinition type, MetadataReader reader) => reader.ReadTypeLayout(type));
				packing_size = row.Col1;
				class_size = row.Col2;
			}
		}
	}

	public TypeDefinition(string @namespace, string name, TypeAttributes attributes)
		: base(@namespace, name)
	{
		this.attributes = (uint)attributes;
		token = new MetadataToken(TokenType.TypeDef);
	}

	public TypeDefinition(string @namespace, string name, TypeAttributes attributes, TypeReference baseType)
		: this(@namespace, name, attributes)
	{
		BaseType = baseType;
	}

	protected override void ClearFullName()
	{
		base.ClearFullName();
		if (HasNestedTypes)
		{
			Collection<TypeDefinition> nestedTypes = NestedTypes;
			for (int i = 0; i < nestedTypes.Count; i++)
			{
				nestedTypes[i].ClearFullName();
			}
		}
	}

	public override TypeDefinition Resolve()
	{
		return this;
	}
}
