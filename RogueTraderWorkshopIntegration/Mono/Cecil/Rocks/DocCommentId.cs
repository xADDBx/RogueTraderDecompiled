using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Cecil.Rocks;

[ComVisible(false)]
public class DocCommentId
{
	private class GenericTypeOptions
	{
		public bool IsArgument { get; set; }

		public bool IsNestedType { get; set; }

		public IList<TypeReference> Arguments { get; set; }

		public int ArgumentIndex { get; set; }

		public static GenericTypeOptions Empty()
		{
			return new GenericTypeOptions();
		}
	}

	private IMemberDefinition commentMember;

	private StringBuilder id;

	private DocCommentId(IMemberDefinition member)
	{
		commentMember = member;
		id = new StringBuilder();
	}

	private void WriteField(FieldDefinition field)
	{
		WriteDefinition('F', field);
	}

	private void WriteEvent(EventDefinition @event)
	{
		WriteDefinition('E', @event);
	}

	private void WriteType(TypeDefinition type)
	{
		id.Append('T').Append(':');
		WriteTypeFullName(type);
	}

	private void WriteMethod(MethodDefinition method)
	{
		WriteDefinition('M', method);
		if (method.HasGenericParameters)
		{
			id.Append('`').Append('`');
			id.Append(method.GenericParameters.Count);
		}
		if (method.HasParameters)
		{
			WriteParameters(method.Parameters);
		}
		if (IsConversionOperator(method))
		{
			WriteReturnType(method);
		}
	}

	private static bool IsConversionOperator(MethodDefinition self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		if (self.IsSpecialName)
		{
			if (!(self.Name == "op_Explicit"))
			{
				return self.Name == "op_Implicit";
			}
			return true;
		}
		return false;
	}

	private void WriteReturnType(MethodDefinition method)
	{
		id.Append('~');
		WriteTypeSignature(method.ReturnType);
	}

	private void WriteProperty(PropertyDefinition property)
	{
		WriteDefinition('P', property);
		if (property.HasParameters)
		{
			WriteParameters(property.Parameters);
		}
	}

	private void WriteParameters(IList<ParameterDefinition> parameters)
	{
		id.Append('(');
		WriteList(parameters, delegate(ParameterDefinition p)
		{
			WriteTypeSignature(p.ParameterType);
		});
		id.Append(')');
	}

	private void WriteTypeSignature(TypeReference type)
	{
		switch (type.MetadataType)
		{
		case MetadataType.Array:
			WriteArrayTypeSignature((ArrayType)type);
			break;
		case MetadataType.ByReference:
			WriteTypeSignature(((ByReferenceType)type).ElementType);
			id.Append('@');
			break;
		case MetadataType.FunctionPointer:
			WriteFunctionPointerTypeSignature((FunctionPointerType)type);
			break;
		case MetadataType.GenericInstance:
			WriteGenericInstanceTypeSignature((GenericInstanceType)type);
			break;
		case MetadataType.Var:
			if (IsGenericMethodTypeParameter(type))
			{
				id.Append('`');
			}
			id.Append('`');
			id.Append(((GenericParameter)type).Position);
			break;
		case MetadataType.MVar:
			id.Append('`').Append('`');
			id.Append(((GenericParameter)type).Position);
			break;
		case MetadataType.OptionalModifier:
			WriteModiferTypeSignature((OptionalModifierType)type, '!');
			break;
		case MetadataType.RequiredModifier:
			WriteModiferTypeSignature((RequiredModifierType)type, '|');
			break;
		case MetadataType.Pointer:
			WriteTypeSignature(((PointerType)type).ElementType);
			id.Append('*');
			break;
		default:
			WriteTypeFullName(type);
			break;
		}
	}

	private bool IsGenericMethodTypeParameter(TypeReference type)
	{
		if (commentMember is MethodDefinition methodDefinition)
		{
			GenericParameter genericParameter = type as GenericParameter;
			if (genericParameter != null)
			{
				return methodDefinition.GenericParameters.Any((GenericParameter i) => i.Name == genericParameter.Name);
			}
		}
		return false;
	}

	private void WriteGenericInstanceTypeSignature(GenericInstanceType type)
	{
		if (type.ElementType.IsTypeSpecification())
		{
			throw new NotSupportedException();
		}
		GenericTypeOptions options = new GenericTypeOptions
		{
			IsArgument = true,
			IsNestedType = type.IsNested,
			Arguments = type.GenericArguments
		};
		WriteTypeFullName(type.ElementType, options);
	}

	private void WriteList<T>(IList<T> list, Action<T> action)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				id.Append(',');
			}
			action(list[i]);
		}
	}

	private void WriteModiferTypeSignature(IModifierType type, char id)
	{
		WriteTypeSignature(type.ElementType);
		this.id.Append(id);
		WriteTypeSignature(type.ModifierType);
	}

	private void WriteFunctionPointerTypeSignature(FunctionPointerType type)
	{
		id.Append("=FUNC:");
		WriteTypeSignature(type.ReturnType);
		if (type.HasParameters)
		{
			WriteParameters(type.Parameters);
		}
	}

	private void WriteArrayTypeSignature(ArrayType type)
	{
		WriteTypeSignature(type.ElementType);
		if (type.IsVector)
		{
			id.Append("[]");
			return;
		}
		id.Append("[");
		WriteList(type.Dimensions, delegate(ArrayDimension dimension)
		{
			if (dimension.LowerBound.HasValue)
			{
				id.Append(dimension.LowerBound.Value);
			}
			id.Append(':');
			if (dimension.UpperBound.HasValue)
			{
				id.Append(dimension.UpperBound.Value - (dimension.LowerBound.GetValueOrDefault() + 1));
			}
		});
		id.Append("]");
	}

	private void WriteDefinition(char id, IMemberDefinition member)
	{
		this.id.Append(id).Append(':');
		WriteTypeFullName(member.DeclaringType);
		this.id.Append('.');
		WriteItemName(member.Name);
	}

	private void WriteTypeFullName(TypeReference type)
	{
		WriteTypeFullName(type, GenericTypeOptions.Empty());
	}

	private void WriteTypeFullName(TypeReference type, GenericTypeOptions options)
	{
		if (type.DeclaringType != null)
		{
			WriteTypeFullName(type.DeclaringType, options);
			id.Append('.');
		}
		if (!string.IsNullOrEmpty(type.Namespace))
		{
			id.Append(type.Namespace);
			id.Append('.');
		}
		string text = type.Name;
		if (options.IsArgument)
		{
			int num = text.LastIndexOf('`');
			if (num > 0)
			{
				text = text.Substring(0, num);
			}
		}
		id.Append(text);
		WriteGenericTypeParameters(type, options);
	}

	private void WriteGenericTypeParameters(TypeReference type, GenericTypeOptions options)
	{
		if (options.IsArgument && IsGenericType(type))
		{
			id.Append('{');
			WriteList(GetGenericTypeArguments(type, options), WriteTypeSignature);
			id.Append('}');
		}
	}

	private static bool IsGenericType(TypeReference type)
	{
		if (type.HasGenericParameters)
		{
			string text = string.Empty;
			int num = type.Name.LastIndexOf('`');
			if (num >= 0)
			{
				text = type.Name.Substring(0, num);
			}
			return type.Name.LastIndexOf('`') == text.Length;
		}
		return false;
	}

	private IList<TypeReference> GetGenericTypeArguments(TypeReference type, GenericTypeOptions options)
	{
		if (options.IsNestedType)
		{
			int count = type.GenericParameters.Count;
			List<TypeReference> result = options.Arguments.Skip(options.ArgumentIndex).Take(count).ToList();
			options.ArgumentIndex += count;
			return result;
		}
		return options.Arguments;
	}

	private void WriteItemName(string name)
	{
		id.Append(name.Replace('.', '#').Replace('<', '{').Replace('>', '}'));
	}

	public override string ToString()
	{
		return id.ToString();
	}

	public static string GetDocCommentId(IMemberDefinition member)
	{
		if (member == null)
		{
			throw new ArgumentNullException("member");
		}
		DocCommentId docCommentId = new DocCommentId(member);
		switch (member.MetadataToken.TokenType)
		{
		case TokenType.Field:
			docCommentId.WriteField((FieldDefinition)member);
			break;
		case TokenType.Method:
			docCommentId.WriteMethod((MethodDefinition)member);
			break;
		case TokenType.TypeDef:
			docCommentId.WriteType((TypeDefinition)member);
			break;
		case TokenType.Event:
			docCommentId.WriteEvent((EventDefinition)member);
			break;
		case TokenType.Property:
			docCommentId.WriteProperty((PropertyDefinition)member);
			break;
		default:
			throw new NotSupportedException(member.FullName);
		}
		return docCommentId.ToString();
	}
}
