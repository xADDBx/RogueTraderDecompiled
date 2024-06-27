using System;
using Mono.Cecil.Cil;

namespace Mono.Cecil;

internal sealed class GenericParameterResolver
{
	internal static TypeReference ResolveReturnTypeIfNeeded(MethodReference methodReference)
	{
		if (methodReference.DeclaringType.IsArray && methodReference.Name == "Get")
		{
			return methodReference.ReturnType;
		}
		GenericInstanceMethod genericInstanceMethod = methodReference as GenericInstanceMethod;
		GenericInstanceType genericInstanceType = methodReference.DeclaringType as GenericInstanceType;
		if (genericInstanceMethod == null && genericInstanceType == null)
		{
			return methodReference.ReturnType;
		}
		return ResolveIfNeeded(genericInstanceMethod, genericInstanceType, methodReference.ReturnType);
	}

	internal static TypeReference ResolveFieldTypeIfNeeded(FieldReference fieldReference)
	{
		return ResolveIfNeeded(null, fieldReference.DeclaringType as GenericInstanceType, fieldReference.FieldType);
	}

	internal static TypeReference ResolveParameterTypeIfNeeded(MethodReference method, ParameterReference parameter)
	{
		GenericInstanceMethod genericInstanceMethod = method as GenericInstanceMethod;
		GenericInstanceType genericInstanceType = method.DeclaringType as GenericInstanceType;
		if (genericInstanceMethod == null && genericInstanceType == null)
		{
			return parameter.ParameterType;
		}
		return ResolveIfNeeded(genericInstanceMethod, genericInstanceType, parameter.ParameterType);
	}

	internal static TypeReference ResolveVariableTypeIfNeeded(MethodReference method, VariableReference variable)
	{
		GenericInstanceMethod genericInstanceMethod = method as GenericInstanceMethod;
		GenericInstanceType genericInstanceType = method.DeclaringType as GenericInstanceType;
		if (genericInstanceMethod == null && genericInstanceType == null)
		{
			return variable.VariableType;
		}
		return ResolveIfNeeded(genericInstanceMethod, genericInstanceType, variable.VariableType);
	}

	private static TypeReference ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance declaringGenericInstanceType, TypeReference parameterType)
	{
		if (parameterType is ByReferenceType byReferenceType)
		{
			return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, byReferenceType);
		}
		if (parameterType is ArrayType arrayType)
		{
			return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, arrayType);
		}
		if (parameterType is GenericInstanceType genericInstanceType)
		{
			return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, genericInstanceType);
		}
		if (parameterType is GenericParameter genericParameterElement)
		{
			return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, genericParameterElement);
		}
		if (parameterType is RequiredModifierType requiredModifierType && ContainsGenericParameters(requiredModifierType))
		{
			return ResolveIfNeeded(genericInstanceMethod, declaringGenericInstanceType, requiredModifierType.ElementType);
		}
		if (ContainsGenericParameters(parameterType))
		{
			throw new Exception("Unexpected generic parameter.");
		}
		return parameterType;
	}

	private static TypeReference ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, GenericParameter genericParameterElement)
	{
		if (genericParameterElement.MetadataType != MetadataType.MVar)
		{
			return genericInstanceType.GenericArguments[genericParameterElement.Position];
		}
		if (genericInstanceMethod == null)
		{
			return genericParameterElement;
		}
		return genericInstanceMethod.GenericArguments[genericParameterElement.Position];
	}

	private static ArrayType ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, ArrayType arrayType)
	{
		return new ArrayType(ResolveIfNeeded(genericInstanceMethod, genericInstanceType, arrayType.ElementType), arrayType.Rank);
	}

	private static ByReferenceType ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, ByReferenceType byReferenceType)
	{
		return new ByReferenceType(ResolveIfNeeded(genericInstanceMethod, genericInstanceType, byReferenceType.ElementType));
	}

	private static GenericInstanceType ResolveIfNeeded(IGenericInstance genericInstanceMethod, IGenericInstance genericInstanceType, GenericInstanceType genericInstanceType1)
	{
		if (!ContainsGenericParameters(genericInstanceType1))
		{
			return genericInstanceType1;
		}
		GenericInstanceType genericInstanceType2 = new GenericInstanceType(genericInstanceType1.ElementType);
		foreach (TypeReference genericArgument in genericInstanceType1.GenericArguments)
		{
			if (!genericArgument.IsGenericParameter)
			{
				genericInstanceType2.GenericArguments.Add(ResolveIfNeeded(genericInstanceMethod, genericInstanceType, genericArgument));
				continue;
			}
			GenericParameter genericParameter = (GenericParameter)genericArgument;
			switch (genericParameter.Type)
			{
			case GenericParameterType.Type:
				if (genericInstanceType == null)
				{
					throw new NotSupportedException();
				}
				genericInstanceType2.GenericArguments.Add(genericInstanceType.GenericArguments[genericParameter.Position]);
				break;
			case GenericParameterType.Method:
				if (genericInstanceMethod == null)
				{
					genericInstanceType2.GenericArguments.Add(genericParameter);
				}
				else
				{
					genericInstanceType2.GenericArguments.Add(genericInstanceMethod.GenericArguments[genericParameter.Position]);
				}
				break;
			}
		}
		return genericInstanceType2;
	}

	private static bool ContainsGenericParameters(TypeReference typeReference)
	{
		if (typeReference is GenericParameter)
		{
			return true;
		}
		if (typeReference is ArrayType arrayType)
		{
			return ContainsGenericParameters(arrayType.ElementType);
		}
		if (typeReference is PointerType pointerType)
		{
			return ContainsGenericParameters(pointerType.ElementType);
		}
		if (typeReference is ByReferenceType byReferenceType)
		{
			return ContainsGenericParameters(byReferenceType.ElementType);
		}
		if (typeReference is SentinelType sentinelType)
		{
			return ContainsGenericParameters(sentinelType.ElementType);
		}
		if (typeReference is PinnedType pinnedType)
		{
			return ContainsGenericParameters(pinnedType.ElementType);
		}
		if (typeReference is RequiredModifierType requiredModifierType)
		{
			return ContainsGenericParameters(requiredModifierType.ElementType);
		}
		if (typeReference is GenericInstanceType genericInstanceType)
		{
			foreach (TypeReference genericArgument in genericInstanceType.GenericArguments)
			{
				if (ContainsGenericParameters(genericArgument))
				{
					return true;
				}
			}
			return false;
		}
		if (typeReference is TypeSpecification)
		{
			throw new NotSupportedException();
		}
		return false;
	}
}
