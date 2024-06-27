using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public class MethodReference : MemberReference, IMethodSignature, IMetadataTokenProvider, IGenericParameterProvider, IGenericContext
{
	internal ParameterDefinitionCollection parameters;

	private MethodReturnType return_type;

	private bool has_this;

	private bool explicit_this;

	private MethodCallingConvention calling_convention;

	internal Collection<GenericParameter> generic_parameters;

	public virtual bool HasThis
	{
		get
		{
			return has_this;
		}
		set
		{
			has_this = value;
		}
	}

	public virtual bool ExplicitThis
	{
		get
		{
			return explicit_this;
		}
		set
		{
			explicit_this = value;
		}
	}

	public virtual MethodCallingConvention CallingConvention
	{
		get
		{
			return calling_convention;
		}
		set
		{
			calling_convention = value;
		}
	}

	public virtual bool HasParameters => !parameters.IsNullOrEmpty();

	public virtual Collection<ParameterDefinition> Parameters
	{
		get
		{
			if (parameters == null)
			{
				Interlocked.CompareExchange(ref parameters, new ParameterDefinitionCollection(this), null);
			}
			return parameters;
		}
	}

	IGenericParameterProvider IGenericContext.Type
	{
		get
		{
			TypeReference declaringType = DeclaringType;
			if (declaringType is GenericInstanceType genericInstanceType)
			{
				return genericInstanceType.ElementType;
			}
			return declaringType;
		}
	}

	IGenericParameterProvider IGenericContext.Method => this;

	GenericParameterType IGenericParameterProvider.GenericParameterType => GenericParameterType.Method;

	public virtual bool HasGenericParameters => !generic_parameters.IsNullOrEmpty();

	public virtual Collection<GenericParameter> GenericParameters
	{
		get
		{
			if (generic_parameters == null)
			{
				Interlocked.CompareExchange(ref generic_parameters, new GenericParameterCollection(this), null);
			}
			return generic_parameters;
		}
	}

	public TypeReference ReturnType
	{
		get
		{
			return MethodReturnType?.ReturnType;
		}
		set
		{
			MethodReturnType methodReturnType = MethodReturnType;
			if (methodReturnType != null)
			{
				methodReturnType.ReturnType = value;
			}
		}
	}

	public virtual MethodReturnType MethodReturnType
	{
		get
		{
			return return_type;
		}
		set
		{
			return_type = value;
		}
	}

	public override string FullName
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(ReturnType.FullName).Append(" ").Append(MemberFullName());
			this.MethodSignatureFullName(stringBuilder);
			return stringBuilder.ToString();
		}
	}

	public virtual bool IsGenericInstance => false;

	public override bool ContainsGenericParameter
	{
		get
		{
			if (ReturnType.ContainsGenericParameter || base.ContainsGenericParameter)
			{
				return true;
			}
			if (!HasParameters)
			{
				return false;
			}
			Collection<ParameterDefinition> collection = Parameters;
			for (int i = 0; i < collection.Count; i++)
			{
				if (collection[i].ParameterType.ContainsGenericParameter)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal MethodReference()
	{
		return_type = new MethodReturnType(this);
		token = new MetadataToken(TokenType.MemberRef);
	}

	public MethodReference(string name, TypeReference returnType)
		: base(name)
	{
		Mixin.CheckType(returnType, Mixin.Argument.returnType);
		return_type = new MethodReturnType(this);
		return_type.ReturnType = returnType;
		token = new MetadataToken(TokenType.MemberRef);
	}

	public MethodReference(string name, TypeReference returnType, TypeReference declaringType)
		: this(name, returnType)
	{
		Mixin.CheckType(declaringType, Mixin.Argument.declaringType);
		DeclaringType = declaringType;
	}

	public virtual MethodReference GetElementMethod()
	{
		return this;
	}

	protected override IMemberDefinition ResolveDefinition()
	{
		return Resolve();
	}

	public new virtual MethodDefinition Resolve()
	{
		return (Module ?? throw new NotSupportedException()).Resolve(this);
	}
}
