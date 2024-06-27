using System;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;
using Mono.Collections.Generic;

namespace Mono.Cecil;

internal sealed class SignatureWriter : ByteBuffer
{
	private readonly MetadataBuilder metadata;

	public SignatureWriter(MetadataBuilder metadata)
		: base(6)
	{
		this.metadata = metadata;
	}

	public void WriteElementType(ElementType element_type)
	{
		WriteByte((byte)element_type);
	}

	public void WriteUTF8String(string @string)
	{
		if (@string == null)
		{
			WriteByte(byte.MaxValue);
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(@string);
		WriteCompressedUInt32((uint)bytes.Length);
		WriteBytes(bytes);
	}

	public void WriteMethodSignature(IMethodSignature method)
	{
		byte b = (byte)method.CallingConvention;
		if (method.HasThis)
		{
			b = (byte)(b | 0x20u);
		}
		if (method.ExplicitThis)
		{
			b = (byte)(b | 0x40u);
		}
		int num = ((method is IGenericParameterProvider { HasGenericParameters: not false } genericParameterProvider) ? genericParameterProvider.GenericParameters.Count : 0);
		if (num > 0)
		{
			b = (byte)(b | 0x10u);
		}
		int num2 = (method.HasParameters ? method.Parameters.Count : 0);
		WriteByte(b);
		if (num > 0)
		{
			WriteCompressedUInt32((uint)num);
		}
		WriteCompressedUInt32((uint)num2);
		WriteTypeSignature(method.ReturnType);
		if (num2 != 0)
		{
			Collection<ParameterDefinition> parameters = method.Parameters;
			for (int i = 0; i < num2; i++)
			{
				WriteTypeSignature(parameters[i].ParameterType);
			}
		}
	}

	private uint MakeTypeDefOrRefCodedRID(TypeReference type)
	{
		return CodedIndex.TypeDefOrRef.CompressMetadataToken(metadata.LookupToken(type));
	}

	public void WriteTypeToken(TypeReference type)
	{
		WriteCompressedUInt32(MakeTypeDefOrRefCodedRID(type));
	}

	public void WriteTypeSignature(TypeReference type)
	{
		if (type == null)
		{
			throw new ArgumentNullException();
		}
		ElementType etype = type.etype;
		switch (etype)
		{
		case ElementType.Var:
		case ElementType.MVar:
		{
			GenericParameter obj = (GenericParameter)type;
			WriteElementType(etype);
			int num = obj.Position;
			if (num == -1)
			{
				throw new NotSupportedException();
			}
			WriteCompressedUInt32((uint)num);
			break;
		}
		case ElementType.GenericInst:
		{
			GenericInstanceType genericInstanceType = (GenericInstanceType)type;
			WriteElementType(ElementType.GenericInst);
			WriteElementType(genericInstanceType.IsValueType ? ElementType.ValueType : ElementType.Class);
			WriteCompressedUInt32(MakeTypeDefOrRefCodedRID(genericInstanceType.ElementType));
			WriteGenericInstanceSignature(genericInstanceType);
			break;
		}
		case ElementType.Ptr:
		case ElementType.ByRef:
		case ElementType.Sentinel:
		case ElementType.Pinned:
		{
			TypeSpecification typeSpecification = (TypeSpecification)type;
			WriteElementType(etype);
			WriteTypeSignature(typeSpecification.ElementType);
			break;
		}
		case ElementType.FnPtr:
		{
			FunctionPointerType method = (FunctionPointerType)type;
			WriteElementType(ElementType.FnPtr);
			WriteMethodSignature(method);
			break;
		}
		case ElementType.CModReqD:
		case ElementType.CModOpt:
		{
			IModifierType type2 = (IModifierType)type;
			WriteModifierSignature(etype, type2);
			break;
		}
		case ElementType.Array:
		{
			ArrayType arrayType = (ArrayType)type;
			if (!arrayType.IsVector)
			{
				WriteArrayTypeSignature(arrayType);
				break;
			}
			WriteElementType(ElementType.SzArray);
			WriteTypeSignature(arrayType.ElementType);
			break;
		}
		case ElementType.None:
			WriteElementType(type.IsValueType ? ElementType.ValueType : ElementType.Class);
			WriteCompressedUInt32(MakeTypeDefOrRefCodedRID(type));
			break;
		default:
			if (!TryWriteElementType(type))
			{
				throw new NotSupportedException();
			}
			break;
		}
	}

	private void WriteArrayTypeSignature(ArrayType array)
	{
		WriteElementType(ElementType.Array);
		WriteTypeSignature(array.ElementType);
		Collection<ArrayDimension> dimensions = array.Dimensions;
		int count = dimensions.Count;
		WriteCompressedUInt32((uint)count);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			ArrayDimension arrayDimension = dimensions[i];
			if (arrayDimension.UpperBound.HasValue)
			{
				num++;
				num2++;
			}
			else if (arrayDimension.LowerBound.HasValue)
			{
				num2++;
			}
		}
		int[] array2 = new int[num];
		int[] array3 = new int[num2];
		for (int j = 0; j < num2; j++)
		{
			ArrayDimension arrayDimension2 = dimensions[j];
			array3[j] = arrayDimension2.LowerBound.GetValueOrDefault();
			if (arrayDimension2.UpperBound.HasValue)
			{
				array2[j] = arrayDimension2.UpperBound.Value - array3[j] + 1;
			}
		}
		WriteCompressedUInt32((uint)num);
		for (int k = 0; k < num; k++)
		{
			WriteCompressedUInt32((uint)array2[k]);
		}
		WriteCompressedUInt32((uint)num2);
		for (int l = 0; l < num2; l++)
		{
			WriteCompressedInt32(array3[l]);
		}
	}

	public void WriteGenericInstanceSignature(IGenericInstance instance)
	{
		Collection<TypeReference> genericArguments = instance.GenericArguments;
		int count = genericArguments.Count;
		WriteCompressedUInt32((uint)count);
		for (int i = 0; i < count; i++)
		{
			WriteTypeSignature(genericArguments[i]);
		}
	}

	private void WriteModifierSignature(ElementType element_type, IModifierType type)
	{
		WriteElementType(element_type);
		WriteCompressedUInt32(MakeTypeDefOrRefCodedRID(type.ModifierType));
		WriteTypeSignature(type.ElementType);
	}

	private bool TryWriteElementType(TypeReference type)
	{
		ElementType etype = type.etype;
		if (etype == ElementType.None)
		{
			return false;
		}
		WriteElementType(etype);
		return true;
	}

	public void WriteConstantString(string value)
	{
		if (value != null)
		{
			WriteBytes(Encoding.Unicode.GetBytes(value));
		}
		else
		{
			WriteByte(byte.MaxValue);
		}
	}

	public void WriteConstantPrimitive(object value)
	{
		WritePrimitiveValue(value);
	}

	public void WriteCustomAttributeConstructorArguments(CustomAttribute attribute)
	{
		if (attribute.HasConstructorArguments)
		{
			Collection<CustomAttributeArgument> constructorArguments = attribute.ConstructorArguments;
			Collection<ParameterDefinition> parameters = attribute.Constructor.Parameters;
			if (parameters.Count != constructorArguments.Count)
			{
				throw new InvalidOperationException();
			}
			for (int i = 0; i < constructorArguments.Count; i++)
			{
				TypeReference type = GenericParameterResolver.ResolveParameterTypeIfNeeded(attribute.Constructor, parameters[i]);
				WriteCustomAttributeFixedArgument(type, constructorArguments[i]);
			}
		}
	}

	private void WriteCustomAttributeFixedArgument(TypeReference type, CustomAttributeArgument argument)
	{
		if (type.IsArray)
		{
			WriteCustomAttributeFixedArrayArgument((ArrayType)type, argument);
		}
		else
		{
			WriteCustomAttributeElement(type, argument);
		}
	}

	private void WriteCustomAttributeFixedArrayArgument(ArrayType type, CustomAttributeArgument argument)
	{
		if (!(argument.Value is CustomAttributeArgument[] array))
		{
			WriteUInt32(uint.MaxValue);
			return;
		}
		WriteInt32(array.Length);
		if (array.Length != 0)
		{
			TypeReference elementType = type.ElementType;
			for (int i = 0; i < array.Length; i++)
			{
				WriteCustomAttributeElement(elementType, array[i]);
			}
		}
	}

	private void WriteCustomAttributeElement(TypeReference type, CustomAttributeArgument argument)
	{
		if (type.IsArray)
		{
			WriteCustomAttributeFixedArrayArgument((ArrayType)type, argument);
		}
		else if (type.etype == ElementType.Object)
		{
			argument = (CustomAttributeArgument)argument.Value;
			type = argument.Type;
			WriteCustomAttributeFieldOrPropType(type);
			WriteCustomAttributeElement(type, argument);
		}
		else
		{
			WriteCustomAttributeValue(type, argument.Value);
		}
	}

	private void WriteCustomAttributeValue(TypeReference type, object value)
	{
		switch (type.etype)
		{
		case ElementType.String:
		{
			string text = (string)value;
			if (text == null)
			{
				WriteByte(byte.MaxValue);
			}
			else
			{
				WriteUTF8String(text);
			}
			break;
		}
		case ElementType.None:
			if (type.IsTypeOf("System", "Type"))
			{
				WriteCustomAttributeTypeValue((TypeReference)value);
			}
			else
			{
				WriteCustomAttributeEnumValue(type, value);
			}
			break;
		case ElementType.GenericInst:
			WriteCustomAttributeEnumValue(type, value);
			break;
		default:
			WritePrimitiveValue(value);
			break;
		}
	}

	private void WriteCustomAttributeTypeValue(TypeReference value)
	{
		if (value is TypeDefinition typeDefinition)
		{
			TypeDefinition typeDefinition2 = typeDefinition;
			while (typeDefinition2.DeclaringType != null)
			{
				typeDefinition2 = typeDefinition2.DeclaringType;
			}
			if (WindowsRuntimeProjections.IsClrImplementationType(typeDefinition2))
			{
				WindowsRuntimeProjections.Project(typeDefinition2);
				WriteTypeReference(value);
				WindowsRuntimeProjections.RemoveProjection(typeDefinition2);
				return;
			}
		}
		WriteTypeReference(value);
	}

	private void WritePrimitiveValue(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException();
		}
		switch (Type.GetTypeCode(value.GetType()))
		{
		case TypeCode.Boolean:
			WriteByte((byte)(((bool)value) ? 1u : 0u));
			break;
		case TypeCode.Byte:
			WriteByte((byte)value);
			break;
		case TypeCode.SByte:
			WriteSByte((sbyte)value);
			break;
		case TypeCode.Int16:
			WriteInt16((short)value);
			break;
		case TypeCode.UInt16:
			WriteUInt16((ushort)value);
			break;
		case TypeCode.Char:
			WriteInt16((short)(char)value);
			break;
		case TypeCode.Int32:
			WriteInt32((int)value);
			break;
		case TypeCode.UInt32:
			WriteUInt32((uint)value);
			break;
		case TypeCode.Single:
			WriteSingle((float)value);
			break;
		case TypeCode.Int64:
			WriteInt64((long)value);
			break;
		case TypeCode.UInt64:
			WriteUInt64((ulong)value);
			break;
		case TypeCode.Double:
			WriteDouble((double)value);
			break;
		default:
			throw new NotSupportedException(value.GetType().FullName);
		}
	}

	private void WriteCustomAttributeEnumValue(TypeReference enum_type, object value)
	{
		TypeDefinition typeDefinition = enum_type.CheckedResolve();
		if (!typeDefinition.IsEnum)
		{
			throw new ArgumentException();
		}
		WriteCustomAttributeValue(typeDefinition.GetEnumUnderlyingType(), value);
	}

	private void WriteCustomAttributeFieldOrPropType(TypeReference type)
	{
		if (type.IsArray)
		{
			ArrayType arrayType = (ArrayType)type;
			WriteElementType(ElementType.SzArray);
			WriteCustomAttributeFieldOrPropType(arrayType.ElementType);
			return;
		}
		ElementType etype = type.etype;
		switch (etype)
		{
		case ElementType.Object:
			WriteElementType(ElementType.Boxed);
			break;
		case ElementType.None:
			if (type.IsTypeOf("System", "Type"))
			{
				WriteElementType(ElementType.Type);
				break;
			}
			WriteElementType(ElementType.Enum);
			WriteTypeReference(type);
			break;
		case ElementType.GenericInst:
			WriteElementType(ElementType.Enum);
			WriteTypeReference(type);
			break;
		default:
			WriteElementType(etype);
			break;
		}
	}

	public void WriteCustomAttributeNamedArguments(CustomAttribute attribute)
	{
		int namedArgumentCount = GetNamedArgumentCount(attribute);
		WriteUInt16((ushort)namedArgumentCount);
		if (namedArgumentCount != 0)
		{
			WriteICustomAttributeNamedArguments(attribute);
		}
	}

	private static int GetNamedArgumentCount(ICustomAttribute attribute)
	{
		int num = 0;
		if (attribute.HasFields)
		{
			num += attribute.Fields.Count;
		}
		if (attribute.HasProperties)
		{
			num += attribute.Properties.Count;
		}
		return num;
	}

	private void WriteICustomAttributeNamedArguments(ICustomAttribute attribute)
	{
		if (attribute.HasFields)
		{
			WriteCustomAttributeNamedArguments(83, attribute.Fields);
		}
		if (attribute.HasProperties)
		{
			WriteCustomAttributeNamedArguments(84, attribute.Properties);
		}
	}

	private void WriteCustomAttributeNamedArguments(byte kind, Collection<CustomAttributeNamedArgument> named_arguments)
	{
		for (int i = 0; i < named_arguments.Count; i++)
		{
			WriteCustomAttributeNamedArgument(kind, named_arguments[i]);
		}
	}

	private void WriteCustomAttributeNamedArgument(byte kind, CustomAttributeNamedArgument named_argument)
	{
		CustomAttributeArgument argument = named_argument.Argument;
		WriteByte(kind);
		WriteCustomAttributeFieldOrPropType(argument.Type);
		WriteUTF8String(named_argument.Name);
		WriteCustomAttributeFixedArgument(argument.Type, argument);
	}

	private void WriteSecurityAttribute(SecurityAttribute attribute)
	{
		WriteTypeReference(attribute.AttributeType);
		int namedArgumentCount = GetNamedArgumentCount(attribute);
		if (namedArgumentCount == 0)
		{
			WriteCompressedUInt32(1u);
			WriteCompressedUInt32(0u);
			return;
		}
		SignatureWriter signatureWriter = new SignatureWriter(metadata);
		signatureWriter.WriteCompressedUInt32((uint)namedArgumentCount);
		signatureWriter.WriteICustomAttributeNamedArguments(attribute);
		WriteCompressedUInt32((uint)signatureWriter.length);
		WriteBytes(signatureWriter);
	}

	public void WriteSecurityDeclaration(SecurityDeclaration declaration)
	{
		WriteByte(46);
		Collection<SecurityAttribute> security_attributes = declaration.security_attributes;
		if (security_attributes == null)
		{
			throw new NotSupportedException();
		}
		WriteCompressedUInt32((uint)security_attributes.Count);
		for (int i = 0; i < security_attributes.Count; i++)
		{
			WriteSecurityAttribute(security_attributes[i]);
		}
	}

	public void WriteXmlSecurityDeclaration(SecurityDeclaration declaration)
	{
		string xmlSecurityDeclaration = GetXmlSecurityDeclaration(declaration);
		if (xmlSecurityDeclaration == null)
		{
			throw new NotSupportedException();
		}
		WriteBytes(Encoding.Unicode.GetBytes(xmlSecurityDeclaration));
	}

	private static string GetXmlSecurityDeclaration(SecurityDeclaration declaration)
	{
		if (declaration.security_attributes == null || declaration.security_attributes.Count != 1)
		{
			return null;
		}
		SecurityAttribute securityAttribute = declaration.security_attributes[0];
		if (!securityAttribute.AttributeType.IsTypeOf("System.Security.Permissions", "PermissionSetAttribute"))
		{
			return null;
		}
		if (securityAttribute.properties == null || securityAttribute.properties.Count != 1)
		{
			return null;
		}
		CustomAttributeNamedArgument customAttributeNamedArgument = securityAttribute.properties[0];
		if (customAttributeNamedArgument.Name != "XML")
		{
			return null;
		}
		return (string)customAttributeNamedArgument.Argument.Value;
	}

	private void WriteTypeReference(TypeReference type)
	{
		WriteUTF8String(TypeParser.ToParseable(type, top_level: false));
	}

	public void WriteMarshalInfo(MarshalInfo marshal_info)
	{
		WriteNativeType(marshal_info.native);
		switch (marshal_info.native)
		{
		case NativeType.Array:
		{
			ArrayMarshalInfo arrayMarshalInfo = (ArrayMarshalInfo)marshal_info;
			if (arrayMarshalInfo.element_type != NativeType.None)
			{
				WriteNativeType(arrayMarshalInfo.element_type);
			}
			if (arrayMarshalInfo.size_parameter_index > -1)
			{
				WriteCompressedUInt32((uint)arrayMarshalInfo.size_parameter_index);
			}
			if (arrayMarshalInfo.size > -1)
			{
				WriteCompressedUInt32((uint)arrayMarshalInfo.size);
			}
			if (arrayMarshalInfo.size_parameter_multiplier > -1)
			{
				WriteCompressedUInt32((uint)arrayMarshalInfo.size_parameter_multiplier);
			}
			break;
		}
		case NativeType.SafeArray:
		{
			SafeArrayMarshalInfo safeArrayMarshalInfo = (SafeArrayMarshalInfo)marshal_info;
			if (safeArrayMarshalInfo.element_type != 0)
			{
				WriteVariantType(safeArrayMarshalInfo.element_type);
			}
			break;
		}
		case NativeType.FixedArray:
		{
			FixedArrayMarshalInfo fixedArrayMarshalInfo = (FixedArrayMarshalInfo)marshal_info;
			if (fixedArrayMarshalInfo.size > -1)
			{
				WriteCompressedUInt32((uint)fixedArrayMarshalInfo.size);
			}
			if (fixedArrayMarshalInfo.element_type != NativeType.None)
			{
				WriteNativeType(fixedArrayMarshalInfo.element_type);
			}
			break;
		}
		case NativeType.FixedSysString:
		{
			FixedSysStringMarshalInfo fixedSysStringMarshalInfo = (FixedSysStringMarshalInfo)marshal_info;
			if (fixedSysStringMarshalInfo.size > -1)
			{
				WriteCompressedUInt32((uint)fixedSysStringMarshalInfo.size);
			}
			break;
		}
		case NativeType.CustomMarshaler:
		{
			CustomMarshalInfo customMarshalInfo = (CustomMarshalInfo)marshal_info;
			WriteUTF8String((customMarshalInfo.guid != Guid.Empty) ? customMarshalInfo.guid.ToString() : string.Empty);
			WriteUTF8String(customMarshalInfo.unmanaged_type);
			WriteTypeReference(customMarshalInfo.managed_type);
			WriteUTF8String(customMarshalInfo.cookie);
			break;
		}
		}
	}

	private void WriteNativeType(NativeType native)
	{
		WriteByte((byte)native);
	}

	private void WriteVariantType(VariantType variant)
	{
		WriteByte((byte)variant);
	}

	public void WriteSequencePoints(MethodDebugInformation info)
	{
		int num = -1;
		int num2 = -1;
		WriteCompressedUInt32(info.local_var_token.RID);
		if (!info.TryGetUniqueDocument(out var document))
		{
			document = null;
		}
		for (int i = 0; i < info.SequencePoints.Count; i++)
		{
			SequencePoint sequencePoint = info.SequencePoints[i];
			Document document2 = sequencePoint.Document;
			if (document != document2)
			{
				MetadataToken documentToken = metadata.GetDocumentToken(document2);
				if (document != null)
				{
					WriteCompressedUInt32(0u);
				}
				WriteCompressedUInt32(documentToken.RID);
				document = document2;
			}
			if (i > 0)
			{
				WriteCompressedUInt32((uint)(sequencePoint.Offset - info.SequencePoints[i - 1].Offset));
			}
			else
			{
				WriteCompressedUInt32((uint)sequencePoint.Offset);
			}
			if (sequencePoint.IsHidden)
			{
				WriteInt16(0);
				continue;
			}
			int num3 = sequencePoint.EndLine - sequencePoint.StartLine;
			int value = sequencePoint.EndColumn - sequencePoint.StartColumn;
			WriteCompressedUInt32((uint)num3);
			if (num3 == 0)
			{
				WriteCompressedUInt32((uint)value);
			}
			else
			{
				WriteCompressedInt32(value);
			}
			if (num < 0)
			{
				WriteCompressedUInt32((uint)sequencePoint.StartLine);
				WriteCompressedUInt32((uint)sequencePoint.StartColumn);
			}
			else
			{
				WriteCompressedInt32(sequencePoint.StartLine - num);
				WriteCompressedInt32(sequencePoint.StartColumn - num2);
			}
			num = sequencePoint.StartLine;
			num2 = sequencePoint.StartColumn;
		}
	}
}
