using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Mono.Cecil;

[Serializable]
[ComVisible(false)]
public sealed class ResolutionException : Exception
{
	private readonly MemberReference member;

	public MemberReference Member => member;

	public IMetadataScope Scope
	{
		get
		{
			if (member is TypeReference typeReference)
			{
				return typeReference.Scope;
			}
			TypeReference declaringType = member.DeclaringType;
			if (declaringType != null)
			{
				return declaringType.Scope;
			}
			throw new NotSupportedException();
		}
	}

	public ResolutionException(MemberReference member)
		: base("Failed to resolve " + member.FullName)
	{
		if (member == null)
		{
			throw new ArgumentNullException("member");
		}
		this.member = member;
	}

	public ResolutionException(MemberReference member, Exception innerException)
		: base("Failed to resolve " + member.FullName, innerException)
	{
		if (member == null)
		{
			throw new ArgumentNullException("member");
		}
		this.member = member;
	}

	private ResolutionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
