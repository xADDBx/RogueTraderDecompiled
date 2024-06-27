using System;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;
using Mono.Collections.Generic;

namespace Mono.Cecil;

internal sealed class SignatureReader : ByteBuffer
{
	private readonly MetadataReader reader;

	internal readonly uint start;

	internal readonly uint sig_length;

	private TypeSystem TypeSystem => reader.module.TypeSystem;

	public SignatureReader(uint blob, MetadataReader reader)
		: base(reader.image.BlobHeap.data)
	{
		this.reader = reader;
		position = (int)blob;
		sig_length = ReadCompressedUInt32();
		start = (uint)position;
	}

	private MetadataToken ReadTypeTokenSignature()
	{
		return CodedIndex.TypeDefOrRef.GetMetadataToken(ReadCompressedUInt32());
	}

	private GenericParameter GetGenericParameter(GenericParameterType type, uint var)
	{
		IGenericContext context = reader.context;
		if (context == null)
		{
			return GetUnboundGenericParameter(type, (int)var);
		}
		IGenericParameterProvider genericParameterProvider = type switch
		{
			GenericParameterType.Type => context.Type, 
			GenericParameterType.Method => context.Method, 
			_ => throw new NotSupportedException(), 
		};
		if (!context.IsDefinition)
		{
			CheckGenericContext(genericParameterProvider, (int)var);
		}
		if ((int)var >= genericParameterProvider.GenericParameters.Count)
		{
			return GetUnboundGenericParameter(type, (int)var);
		}
		return genericParameterProvider.GenericParameters[(int)var];
	}

	private GenericParameter GetUnboundGenericParameter(GenericParameterType type, int index)
	{
		return new GenericParameter(index, type, reader.module);
	}

	private static void CheckGenericContext(IGenericParameterProvider owner, int index)
	{
		Collection<GenericParameter> genericParameters = owner.GenericParameters;
		for (int i = genericParameters.Count; i <= index; i++)
		{
			genericParameters.Add(new GenericParameter(owner));
		}
	}

	public void ReadGenericInstanceSignature(IGenericParameterProvider provider, IGenericInstance instance, uint arity)
	{
		if (!provider.IsDefinition)
		{
			CheckGenericContext(provider, (int)(arity - 1));
		}
		Collection<TypeReference> genericArguments = instance.GenericArguments;
		for (int i = 0; i < arity; i++)
		{
			genericArguments.Add(ReadTypeSignature());
		}
	}

	private ArrayType ReadArrayTypeSignature()
	{
		ArrayType arrayType = new ArrayType(ReadTypeSignature());
		uint num = ReadCompressedUInt32();
		uint[] array = new uint[ReadCompressedUInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ReadCompressedUInt32();
		}
		int[] array2 = new int[ReadCompressedUInt32()];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = ReadCompressedInt32();
		}
		arrayType.Dimensions.Clear();
		for (int k = 0; k < num; k++)
		{
			int? num2 = null;
			int? upperBound = null;
			if (k < array2.Length)
			{
				num2 = array2[k];
			}
			if (k < array.Length)
			{
				upperBound = num2 + (int)array[k] - 1;
			}
			arrayType.Dimensions.Add(new ArrayDimension(num2, upperBound));
		}
		return arrayType;
	}

	private TypeReference GetTypeDefOrRef(MetadataToken token)
	{
		return reader.GetTypeDefOrRef(token);
	}

	public TypeReference ReadTypeSignature()
	{
		return ReadTypeSignature((ElementType)ReadByte());
	}

	public TypeReference ReadTypeToken()
	{
		return GetTypeDefOrRef(ReadTypeTokenSignature());
	}

	private TypeReference ReadTypeSignature(ElementType etype)
	{
		switch (etype)
		{
		case ElementType.ValueType:
		{
			TypeReference typeDefOrRef2 = GetTypeDefOrRef(ReadTypeTokenSignature());
			typeDefOrRef2.KnownValueType();
			return typeDefOrRef2;
		}
		case ElementType.Class:
			return GetTypeDefOrRef(ReadTypeTokenSignature());
		case ElementType.Ptr:
			return new PointerType(ReadTypeSignature());
		case ElementType.FnPtr:
		{
			FunctionPointerType functionPointerType = new FunctionPointerType();
			ReadMethodSignature(functionPointerType);
			return functionPointerType;
		}
		case ElementType.ByRef:
			return new ByReferenceType(ReadTypeSignature());
		case ElementType.Pinned:
			return new PinnedType(ReadTypeSignature());
		case ElementType.SzArray:
			return new ArrayType(ReadTypeSignature());
		case ElementType.Array:
			return ReadArrayTypeSignature();
		case ElementType.CModOpt:
			return new OptionalModifierType(GetTypeDefOrRef(ReadTypeTokenSignature()), ReadTypeSignature());
		case ElementType.CModReqD:
			return new RequiredModifierType(GetTypeDefOrRef(ReadTypeTokenSignature()), ReadTypeSignature());
		case ElementType.Sentinel:
			return new SentinelType(ReadTypeSignature());
		case ElementType.Var:
			return GetGenericParameter(GenericParameterType.Type, ReadCompressedUInt32());
		case ElementType.MVar:
			return GetGenericParameter(GenericParameterType.Method, ReadCompressedUInt32());
		case ElementType.GenericInst:
		{
			bool num = ReadByte() == 17;
			TypeReference typeDefOrRef = GetTypeDefOrRef(ReadTypeTokenSignature());
			uint arity = ReadCompressedUInt32();
			GenericInstanceType genericInstanceType = new GenericInstanceType(typeDefOrRef, (int)arity);
			ReadGenericInstanceSignature(typeDefOrRef, genericInstanceType, arity);
			if (num)
			{
				genericInstanceType.KnownValueType();
				typeDefOrRef.GetElementType().KnownValueType();
			}
			return genericInstanceType;
		}
		case ElementType.Object:
			return TypeSystem.Object;
		case ElementType.Void:
			return TypeSystem.Void;
		case ElementType.TypedByRef:
			return TypeSystem.TypedReference;
		case ElementType.I:
			return TypeSystem.IntPtr;
		case ElementType.U:
			return TypeSystem.UIntPtr;
		default:
			return GetPrimitiveType(etype);
		}
	}

	public void ReadMethodSignature(IMethodSignature method)
	{
		byte b = ReadByte();
		if ((b & 0x20u) != 0)
		{
			method.HasThis = true;
			b = (byte)(b & 0xFFFFFFDFu);
		}
		if ((b & 0x40u) != 0)
		{
			method.ExplicitThis = true;
			b = (byte)(b & 0xFFFFFFBFu);
		}
		method.CallingConvention = (MethodCallingConvention)b;
		MethodReference methodReference = method as MethodReference;
		if (methodReference != null && !methodReference.DeclaringType.IsArray)
		{
			reader.context = methodReference;
		}
		if ((b & 0x10u) != 0)
		{
			uint num = ReadCompressedUInt32();
			if (methodReference != null && !methodReference.IsDefinition)
			{
				CheckGenericContext(methodReference, (int)(num - 1));
			}
		}
		uint num2 = ReadCompressedUInt32();
		method.MethodReturnType.ReturnType = ReadTypeSignature();
		if (num2 != 0)
		{
			Collection<ParameterDefinition> collection = ((!(method is MethodReference methodReference2)) ? method.Parameters : (methodReference2.parameters = new ParameterDefinitionCollection(method, (int)num2)));
			for (int i = 0; i < num2; i++)
			{
				collection.Add(new ParameterDefinition(ReadTypeSignature()));
			}
		}
	}

	public object ReadConstantSignature(ElementType type)
	{
		return ReadPrimitiveValue(type);
	}

	public void ReadCustomAttributeConstructorArguments(CustomAttribute attribute, Collection<ParameterDefinition> parameters)
	{
		int count = parameters.Count;
		if (count != 0)
		{
			attribute.arguments = new Collection<CustomAttributeArgument>(count);
			for (int i = 0; i < count; i++)
			{
				TypeReference type = GenericParameterResolver.ResolveParameterTypeIfNeeded(attribute.Constructor, parameters[i]);
				attribute.arguments.Add(ReadCustomAttributeFixedArgument(type));
			}
		}
	}

	private CustomAttributeArgument ReadCustomAttributeFixedArgument(TypeReference type)
	{
		if (type.IsArray)
		{
			return ReadCustomAttributeFixedArrayArgument((ArrayType)type);
		}
		return ReadCustomAttributeElement(type);
	}

	public void ReadCustomAttributeNamedArguments(ushort count, ref Collection<CustomAttributeNamedArgument> fields, ref Collection<CustomAttributeNamedArgument> properties)
	{
		for (int i = 0; i < count; i++)
		{
			if (!CanReadMore())
			{
				break;
			}
			ReadCustomAttributeNamedArgument(ref fields, ref properties);
		}
	}

	private void ReadCustomAttributeNamedArgument(ref Collection<CustomAttributeNamedArgument> fields, ref Collection<CustomAttributeNamedArgument> properties)
	{
		byte b = ReadByte();
		TypeReference type = ReadCustomAttributeFieldOrPropType();
		string name = ReadUTF8String();
		(b switch
		{
			83 => GetCustomAttributeNamedArgumentCollection(ref fields), 
			84 => GetCustomAttributeNamedArgumentCollection(ref properties), 
			_ => throw new NotSupportedException(), 
		}).Add(new CustomAttributeNamedArgument(name, ReadCustomAttributeFixedArgument(type)));
	}

	private static Collection<CustomAttributeNamedArgument> GetCustomAttributeNamedArgumentCollection(ref Collection<CustomAttributeNamedArgument> collection)
	{
		if (collection != null)
		{
			return collection;
		}
		return collection = new Collection<CustomAttributeNamedArgument>();
	}

	private CustomAttributeArgument ReadCustomAttributeFixedArrayArgument(ArrayType type)
	{
		uint num = ReadUInt32();
		switch (num)
		{
		case uint.MaxValue:
			return new CustomAttributeArgument(type, null);
		case 0u:
			return new CustomAttributeArgument(type, Empty<CustomAttributeArgument>.Array);
		default:
		{
			CustomAttributeArgument[] array = new CustomAttributeArgument[num];
			TypeReference elementType = type.ElementType;
			for (int i = 0; i < num; i++)
			{
				array[i] = ReadCustomAttributeElement(elementType);
			}
			return new CustomAttributeArgument(type, array);
		}
		}
	}

	private CustomAttributeArgument ReadCustomAttributeElement(TypeReference type)
	{
		if (type.IsArray)
		{
			return ReadCustomAttributeFixedArrayArgument((ArrayType)type);
		}
		return new CustomAttributeArgument(type, (type.etype == ElementType.Object) ? ((object)ReadCustomAttributeElement(ReadCustomAttributeFieldOrPropType())) : ReadCustomAttributeElementValue(type));
	}

	private object ReadCustomAttributeElementValue(TypeReference type)
	{
		ElementType etype = type.etype;
		if (etype == ElementType.GenericInst)
		{
			type = type.GetElementType();
			etype = type.etype;
		}
		switch (etype)
		{
		case ElementType.String:
			return ReadUTF8String();
		case ElementType.None:
			if (type.IsTypeOf("System", "Type"))
			{
				return ReadTypeReference();
			}
			return ReadCustomAttributeEnum(type);
		default:
			return ReadPrimitiveValue(etype);
		}
	}

	private object ReadPrimitiveValue(ElementType type)
	{
		return type switch
		{
			ElementType.Boolean => ReadByte() == 1, 
			ElementType.I1 => (sbyte)ReadByte(), 
			ElementType.U1 => ReadByte(), 
			ElementType.Char => (char)ReadUInt16(), 
			ElementType.I2 => ReadInt16(), 
			ElementType.U2 => ReadUInt16(), 
			ElementType.I4 => ReadInt32(), 
			ElementType.U4 => ReadUInt32(), 
			ElementType.I8 => ReadInt64(), 
			ElementType.U8 => ReadUInt64(), 
			ElementType.R4 => ReadSingle(), 
			ElementType.R8 => ReadDouble(), 
			_ => throw new NotImplementedException(type.ToString()), 
		};
	}

	private TypeReference GetPrimitiveType(ElementType etype)
	{
		return etype switch
		{
			ElementType.Boolean => TypeSystem.Boolean, 
			ElementType.Char => TypeSystem.Char, 
			ElementType.I1 => TypeSystem.SByte, 
			ElementType.U1 => TypeSystem.Byte, 
			ElementType.I2 => TypeSystem.Int16, 
			ElementType.U2 => TypeSystem.UInt16, 
			ElementType.I4 => TypeSystem.Int32, 
			ElementType.U4 => TypeSystem.UInt32, 
			ElementType.I8 => TypeSystem.Int64, 
			ElementType.U8 => TypeSystem.UInt64, 
			ElementType.R4 => TypeSystem.Single, 
			ElementType.R8 => TypeSystem.Double, 
			ElementType.String => TypeSystem.String, 
			_ => throw new NotImplementedException(etype.ToString()), 
		};
	}

	private TypeReference ReadCustomAttributeFieldOrPropType()
	{
		ElementType elementType = (ElementType)ReadByte();
		return elementType switch
		{
			ElementType.Boxed => TypeSystem.Object, 
			ElementType.SzArray => new ArrayType(ReadCustomAttributeFieldOrPropType()), 
			ElementType.Enum => ReadTypeReference(), 
			ElementType.Type => TypeSystem.LookupType("System", "Type"), 
			_ => GetPrimitiveType(elementType), 
		};
	}

	public TypeReference ReadTypeReference()
	{
		return TypeParser.ParseType(reader.module, ReadUTF8String());
	}

	private object ReadCustomAttributeEnum(TypeReference enum_type)
	{
		TypeDefinition typeDefinition = enum_type.CheckedResolve();
		if (!typeDefinition.IsEnum)
		{
			throw new ArgumentException();
		}
		return ReadCustomAttributeElementValue(typeDefinition.GetEnumUnderlyingType());
	}

	public SecurityAttribute ReadSecurityAttribute()
	{
		SecurityAttribute securityAttribute = new SecurityAttribute(ReadTypeReference());
		ReadCompressedUInt32();
		ReadCustomAttributeNamedArguments((ushort)ReadCompressedUInt32(), ref securityAttribute.fields, ref securityAttribute.properties);
		return securityAttribute;
	}

	public MarshalInfo ReadMarshalInfo()
	{
		NativeType nativeType = ReadNativeType();
		switch (nativeType)
		{
		case NativeType.Array:
		{
			ArrayMarshalInfo arrayMarshalInfo = new ArrayMarshalInfo();
			if (CanReadMore())
			{
				arrayMarshalInfo.element_type = ReadNativeType();
			}
			if (CanReadMore())
			{
				arrayMarshalInfo.size_parameter_index = (int)ReadCompressedUInt32();
			}
			if (CanReadMore())
			{
				arrayMarshalInfo.size = (int)ReadCompressedUInt32();
			}
			if (CanReadMore())
			{
				arrayMarshalInfo.size_parameter_multiplier = (int)ReadCompressedUInt32();
			}
			return arrayMarshalInfo;
		}
		case NativeType.SafeArray:
		{
			SafeArrayMarshalInfo safeArrayMarshalInfo = new SafeArrayMarshalInfo();
			if (CanReadMore())
			{
				safeArrayMarshalInfo.element_type = ReadVariantType();
			}
			return safeArrayMarshalInfo;
		}
		case NativeType.FixedArray:
		{
			FixedArrayMarshalInfo fixedArrayMarshalInfo = new FixedArrayMarshalInfo();
			if (CanReadMore())
			{
				fixedArrayMarshalInfo.size = (int)ReadCompressedUInt32();
			}
			if (CanReadMore())
			{
				fixedArrayMarshalInfo.element_type = ReadNativeType();
			}
			return fixedArrayMarshalInfo;
		}
		case NativeType.FixedSysString:
		{
			FixedSysStringMarshalInfo fixedSysStringMarshalInfo = new FixedSysStringMarshalInfo();
			if (CanReadMore())
			{
				fixedSysStringMarshalInfo.size = (int)ReadCompressedUInt32();
			}
			return fixedSysStringMarshalInfo;
		}
		case NativeType.CustomMarshaler:
		{
			CustomMarshalInfo customMarshalInfo = new CustomMarshalInfo();
			string text = ReadUTF8String();
			customMarshalInfo.guid = ((!string.IsNullOrEmpty(text)) ? new Guid(text) : Guid.Empty);
			customMarshalInfo.unmanaged_type = ReadUTF8String();
			customMarshalInfo.managed_type = ReadTypeReference();
			customMarshalInfo.cookie = ReadUTF8String();
			return customMarshalInfo;
		}
		default:
			return new MarshalInfo(nativeType);
		}
	}

	private NativeType ReadNativeType()
	{
		return (NativeType)ReadByte();
	}

	private VariantType ReadVariantType()
	{
		return (VariantType)ReadByte();
	}

	private string ReadUTF8String()
	{
		if (buffer[position] == byte.MaxValue)
		{
			position++;
			return null;
		}
		int num = (int)ReadCompressedUInt32();
		if (num == 0)
		{
			return string.Empty;
		}
		if (position + num > buffer.Length)
		{
			return string.Empty;
		}
		string @string = Encoding.UTF8.GetString(buffer, position, num);
		position += num;
		return @string;
	}

	public string ReadDocumentName()
	{
		char c = (char)buffer[position];
		position++;
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		while (CanReadMore())
		{
			if (num > 0 && c != 0)
			{
				stringBuilder.Append(c);
			}
			uint num2 = ReadCompressedUInt32();
			if (num2 != 0)
			{
				stringBuilder.Append(reader.ReadUTF8StringBlob(num2));
			}
			num++;
		}
		return stringBuilder.ToString();
	}

	public Collection<SequencePoint> ReadSequencePoints(Document document)
	{
		ReadCompressedUInt32();
		if (document == null)
		{
			document = reader.GetDocument(ReadCompressedUInt32());
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool flag = true;
		Collection<SequencePoint> collection = new Collection<SequencePoint>((int)(sig_length - (position - start)) / 5);
		int num4 = 0;
		while (CanReadMore())
		{
			int num5 = (int)ReadCompressedUInt32();
			if (num4 > 0 && num5 == 0)
			{
				document = reader.GetDocument(ReadCompressedUInt32());
			}
			else
			{
				num += num5;
				int num6 = (int)ReadCompressedUInt32();
				int num7 = ((num6 == 0) ? ((int)ReadCompressedUInt32()) : ReadCompressedInt32());
				if (num6 == 0 && num7 == 0)
				{
					collection.Add(new SequencePoint(num, document)
					{
						StartLine = 16707566,
						EndLine = 16707566,
						StartColumn = 0,
						EndColumn = 0
					});
				}
				else
				{
					if (flag)
					{
						num2 = (int)ReadCompressedUInt32();
						num3 = (int)ReadCompressedUInt32();
					}
					else
					{
						num2 += ReadCompressedInt32();
						num3 += ReadCompressedInt32();
					}
					collection.Add(new SequencePoint(num, document)
					{
						StartLine = num2,
						StartColumn = num3,
						EndLine = num2 + num6,
						EndColumn = num3 + num7
					});
					flag = false;
				}
			}
			num4++;
		}
		return collection;
	}

	public bool CanReadMore()
	{
		return position - start < sig_length;
	}
}
