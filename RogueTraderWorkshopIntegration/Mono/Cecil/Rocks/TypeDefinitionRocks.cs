using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Rocks;

[ComVisible(false)]
public static class TypeDefinitionRocks
{
	public static IEnumerable<MethodDefinition> GetConstructors(this TypeDefinition self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		if (!self.HasMethods)
		{
			return Empty<MethodDefinition>.Array;
		}
		return self.Methods.Where((MethodDefinition method) => method.IsConstructor);
	}

	public static MethodDefinition GetStaticConstructor(this TypeDefinition self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		if (!self.HasMethods)
		{
			return null;
		}
		return self.GetConstructors().FirstOrDefault((MethodDefinition ctor) => ctor.IsStatic);
	}

	public static IEnumerable<MethodDefinition> GetMethods(this TypeDefinition self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		if (!self.HasMethods)
		{
			return Empty<MethodDefinition>.Array;
		}
		return self.Methods.Where((MethodDefinition method) => !method.IsConstructor);
	}

	public static TypeReference GetEnumUnderlyingType(this TypeDefinition self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		if (!self.IsEnum)
		{
			throw new ArgumentException();
		}
		return Mixin.GetEnumUnderlyingType(self);
	}
}
