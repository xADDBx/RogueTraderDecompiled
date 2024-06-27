using System;
using Mono.Cecil.Cil;

namespace Mono.Cecil;

internal sealed class TypeResolver
{
	private readonly IGenericInstance _typeDefinitionContext;

	private readonly IGenericInstance _methodDefinitionContext;

	public static TypeResolver For(TypeReference typeReference)
	{
		if (!typeReference.IsGenericInstance)
		{
			return new TypeResolver();
		}
		return new TypeResolver((GenericInstanceType)typeReference);
	}

	public static TypeResolver For(TypeReference typeReference, MethodReference methodReference)
	{
		return new TypeResolver(typeReference as GenericInstanceType, methodReference as GenericInstanceMethod);
	}

	public TypeResolver()
	{
	}

	public TypeResolver(GenericInstanceType typeDefinitionContext)
	{
		_typeDefinitionContext = typeDefinitionContext;
	}

	public TypeResolver(GenericInstanceMethod methodDefinitionContext)
	{
		_methodDefinitionContext = methodDefinitionContext;
	}

	public TypeResolver(GenericInstanceType typeDefinitionContext, GenericInstanceMethod methodDefinitionContext)
	{
		_typeDefinitionContext = typeDefinitionContext;
		_methodDefinitionContext = methodDefinitionContext;
	}

	public MethodReference Resolve(MethodReference method)
	{
		MethodReference result = method;
		if (IsDummy())
		{
			return result;
		}
		TypeReference declaringType = Resolve(method.DeclaringType);
		if (method is GenericInstanceMethod genericInstanceMethod)
		{
			result = new MethodReference(method.Name, method.ReturnType, declaringType);
			foreach (ParameterDefinition parameter in method.Parameters)
			{
				result.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, parameter.ParameterType));
			}
			foreach (GenericParameter genericParameter in genericInstanceMethod.ElementMethod.GenericParameters)
			{
				result.GenericParameters.Add(new GenericParameter(genericParameter.Name, result));
			}
			result.HasThis = method.HasThis;
			GenericInstanceMethod genericInstanceMethod2 = new GenericInstanceMethod(result);
			foreach (TypeReference genericArgument in genericInstanceMethod.GenericArguments)
			{
				genericInstanceMethod2.GenericArguments.Add(Resolve(genericArgument));
			}
			result = genericInstanceMethod2;
		}
		else
		{
			result = new MethodReference(method.Name, method.ReturnType, declaringType);
			foreach (GenericParameter genericParameter2 in method.GenericParameters)
			{
				result.GenericParameters.Add(new GenericParameter(genericParameter2.Name, result));
			}
			foreach (ParameterDefinition parameter2 in method.Parameters)
			{
				result.Parameters.Add(new ParameterDefinition(parameter2.Name, parameter2.Attributes, parameter2.ParameterType));
			}
			result.HasThis = method.HasThis;
		}
		return result;
	}

	public FieldReference Resolve(FieldReference field)
	{
		TypeReference typeReference = Resolve(field.DeclaringType);
		if (typeReference == field.DeclaringType)
		{
			return field;
		}
		return new FieldReference(field.Name, field.FieldType, typeReference);
	}

	public TypeReference ResolveReturnType(MethodReference method)
	{
		return Resolve(GenericParameterResolver.ResolveReturnTypeIfNeeded(method));
	}

	public TypeReference ResolveParameterType(MethodReference method, ParameterReference parameter)
	{
		return Resolve(GenericParameterResolver.ResolveParameterTypeIfNeeded(method, parameter));
	}

	public TypeReference ResolveVariableType(MethodReference method, VariableReference variable)
	{
		return Resolve(GenericParameterResolver.ResolveVariableTypeIfNeeded(method, variable));
	}

	public TypeReference ResolveFieldType(FieldReference field)
	{
		return Resolve(GenericParameterResolver.ResolveFieldTypeIfNeeded(field));
	}

	public TypeReference Resolve(TypeReference typeReference)
	{
		return Resolve(typeReference, includeTypeDefinitions: true);
	}

	public TypeReference Resolve(TypeReference typeReference, bool includeTypeDefinitions)
	{
		if (IsDummy())
		{
			return typeReference;
		}
		if (_typeDefinitionContext != null && _typeDefinitionContext.GenericArguments.Contains(typeReference))
		{
			return typeReference;
		}
		if (_methodDefinitionContext != null && _methodDefinitionContext.GenericArguments.Contains(typeReference))
		{
			return typeReference;
		}
		if (typeReference is GenericParameter genericParameter)
		{
			if (_typeDefinitionContext != null && _typeDefinitionContext.GenericArguments.Contains(genericParameter))
			{
				return genericParameter;
			}
			if (_methodDefinitionContext != null && _methodDefinitionContext.GenericArguments.Contains(genericParameter))
			{
				return genericParameter;
			}
			return ResolveGenericParameter(genericParameter);
		}
		if (typeReference is ArrayType arrayType)
		{
			return new ArrayType(Resolve(arrayType.ElementType), arrayType.Rank);
		}
		if (typeReference is PointerType pointerType)
		{
			return new PointerType(Resolve(pointerType.ElementType));
		}
		if (typeReference is ByReferenceType byReferenceType)
		{
			return new ByReferenceType(Resolve(byReferenceType.ElementType));
		}
		if (typeReference is PinnedType pinnedType)
		{
			return new PinnedType(Resolve(pinnedType.ElementType));
		}
		if (typeReference is GenericInstanceType genericInstanceType)
		{
			GenericInstanceType genericInstanceType2 = new GenericInstanceType(genericInstanceType.ElementType);
			{
				foreach (TypeReference genericArgument in genericInstanceType.GenericArguments)
				{
					genericInstanceType2.GenericArguments.Add(Resolve(genericArgument));
				}
				return genericInstanceType2;
			}
		}
		if (typeReference is RequiredModifierType requiredModifierType)
		{
			return Resolve(requiredModifierType.ElementType, includeTypeDefinitions);
		}
		if (includeTypeDefinitions && typeReference is TypeDefinition { HasGenericParameters: not false } typeDefinition)
		{
			GenericInstanceType genericInstanceType3 = new GenericInstanceType(typeDefinition);
			{
				foreach (GenericParameter genericParameter2 in typeDefinition.GenericParameters)
				{
					genericInstanceType3.GenericArguments.Add(Resolve(genericParameter2));
				}
				return genericInstanceType3;
			}
		}
		if (typeReference is TypeSpecification)
		{
			throw new NotSupportedException($"The type {typeReference.FullName} cannot be resolved correctly.");
		}
		return typeReference;
	}

	internal TypeResolver Nested(GenericInstanceMethod genericInstanceMethod)
	{
		return new TypeResolver(_typeDefinitionContext as GenericInstanceType, genericInstanceMethod);
	}

	private TypeReference ResolveGenericParameter(GenericParameter genericParameter)
	{
		if (genericParameter.Owner == null)
		{
			return HandleOwnerlessInvalidILCode(genericParameter);
		}
		if (!(genericParameter.Owner is MemberReference))
		{
			throw new NotSupportedException();
		}
		if (genericParameter.Type != 0)
		{
			if (_methodDefinitionContext == null)
			{
				return genericParameter;
			}
			return _methodDefinitionContext.GenericArguments[genericParameter.Position];
		}
		return _typeDefinitionContext.GenericArguments[genericParameter.Position];
	}

	private TypeReference HandleOwnerlessInvalidILCode(GenericParameter genericParameter)
	{
		if (genericParameter.Type == GenericParameterType.Method && _typeDefinitionContext != null && genericParameter.Position < _typeDefinitionContext.GenericArguments.Count)
		{
			return _typeDefinitionContext.GenericArguments[genericParameter.Position];
		}
		return genericParameter.Module.TypeSystem.Object;
	}

	private bool IsDummy()
	{
		if (_typeDefinitionContext == null)
		{
			return _methodDefinitionContext == null;
		}
		return false;
	}
}
