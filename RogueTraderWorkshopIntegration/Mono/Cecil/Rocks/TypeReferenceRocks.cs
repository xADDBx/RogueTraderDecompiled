using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Rocks;

[ComVisible(false)]
public static class TypeReferenceRocks
{
	public static ArrayType MakeArrayType(this TypeReference self)
	{
		return new ArrayType(self);
	}

	public static ArrayType MakeArrayType(this TypeReference self, int rank)
	{
		if (rank == 0)
		{
			throw new ArgumentOutOfRangeException("rank");
		}
		ArrayType arrayType = new ArrayType(self);
		for (int i = 1; i < rank; i++)
		{
			arrayType.Dimensions.Add(default(ArrayDimension));
		}
		return arrayType;
	}

	public static PointerType MakePointerType(this TypeReference self)
	{
		return new PointerType(self);
	}

	public static ByReferenceType MakeByReferenceType(this TypeReference self)
	{
		return new ByReferenceType(self);
	}

	public static OptionalModifierType MakeOptionalModifierType(this TypeReference self, TypeReference modifierType)
	{
		return new OptionalModifierType(modifierType, self);
	}

	public static RequiredModifierType MakeRequiredModifierType(this TypeReference self, TypeReference modifierType)
	{
		return new RequiredModifierType(modifierType, self);
	}

	public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, params TypeReference[] arguments)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		if (arguments == null)
		{
			throw new ArgumentNullException("arguments");
		}
		if (arguments.Length == 0)
		{
			throw new ArgumentException();
		}
		if (self.GenericParameters.Count != arguments.Length)
		{
			throw new ArgumentException();
		}
		GenericInstanceType genericInstanceType = new GenericInstanceType(self, arguments.Length);
		foreach (TypeReference item in arguments)
		{
			genericInstanceType.GenericArguments.Add(item);
		}
		return genericInstanceType;
	}

	public static PinnedType MakePinnedType(this TypeReference self)
	{
		return new PinnedType(self);
	}

	public static SentinelType MakeSentinelType(this TypeReference self)
	{
		return new SentinelType(self);
	}
}
