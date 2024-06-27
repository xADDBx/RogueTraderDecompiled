using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Rocks;

[ComVisible(false)]
public static class MethodDefinitionRocks
{
	public static MethodDefinition GetBaseMethod(this MethodDefinition self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		if (!self.IsVirtual)
		{
			return self;
		}
		if (self.IsNewSlot)
		{
			return self;
		}
		for (TypeDefinition typeDefinition = ResolveBaseType(self.DeclaringType); typeDefinition != null; typeDefinition = ResolveBaseType(typeDefinition))
		{
			MethodDefinition matchingMethod = GetMatchingMethod(typeDefinition, self);
			if (matchingMethod != null)
			{
				return matchingMethod;
			}
		}
		return self;
	}

	public static MethodDefinition GetOriginalBaseMethod(this MethodDefinition self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		while (true)
		{
			MethodDefinition baseMethod = self.GetBaseMethod();
			if (baseMethod == self)
			{
				break;
			}
			self = baseMethod;
		}
		return self;
	}

	private static TypeDefinition ResolveBaseType(TypeDefinition type)
	{
		return type?.BaseType?.Resolve();
	}

	private static MethodDefinition GetMatchingMethod(TypeDefinition type, MethodDefinition method)
	{
		return MetadataResolver.GetMethod(type.Methods, method);
	}
}
