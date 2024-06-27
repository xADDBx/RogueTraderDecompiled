using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mono.Collections.Generic;

namespace Mono.Cecil;

[ComVisible(false)]
public sealed class GenericInstanceMethod : MethodSpecification, IGenericInstance, IMetadataTokenProvider, IGenericContext
{
	private Collection<TypeReference> arguments;

	public bool HasGenericArguments => !arguments.IsNullOrEmpty();

	public Collection<TypeReference> GenericArguments
	{
		get
		{
			if (arguments == null)
			{
				Interlocked.CompareExchange(ref arguments, new Collection<TypeReference>(), null);
			}
			return arguments;
		}
	}

	public override bool IsGenericInstance => true;

	IGenericParameterProvider IGenericContext.Method => base.ElementMethod;

	IGenericParameterProvider IGenericContext.Type => base.ElementMethod.DeclaringType;

	public override bool ContainsGenericParameter
	{
		get
		{
			if (!this.ContainsGenericParameter())
			{
				return base.ContainsGenericParameter;
			}
			return true;
		}
	}

	public override string FullName
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			MethodReference elementMethod = base.ElementMethod;
			stringBuilder.Append(elementMethod.ReturnType.FullName).Append(" ").Append(elementMethod.DeclaringType.FullName)
				.Append("::")
				.Append(elementMethod.Name);
			this.GenericInstanceFullName(stringBuilder);
			this.MethodSignatureFullName(stringBuilder);
			return stringBuilder.ToString();
		}
	}

	public GenericInstanceMethod(MethodReference method)
		: base(method)
	{
	}

	internal GenericInstanceMethod(MethodReference method, int arity)
		: this(method)
	{
		arguments = new Collection<TypeReference>(arity);
	}
}
