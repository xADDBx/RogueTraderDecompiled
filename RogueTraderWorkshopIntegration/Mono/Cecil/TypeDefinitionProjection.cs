using System.Collections.Generic;
using Mono.Collections.Generic;

namespace Mono.Cecil;

internal sealed class TypeDefinitionProjection
{
	public readonly TypeAttributes Attributes;

	public readonly string Name;

	public readonly TypeDefinitionTreatment Treatment;

	public readonly Collection<MethodDefinition> RedirectedMethods;

	public readonly Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>> RedirectedInterfaces;

	public TypeDefinitionProjection(TypeDefinition type, TypeDefinitionTreatment treatment, Collection<MethodDefinition> redirectedMethods, Collection<KeyValuePair<InterfaceImplementation, InterfaceImplementation>> redirectedInterfaces)
	{
		Attributes = type.Attributes;
		Name = type.Name;
		Treatment = treatment;
		RedirectedMethods = redirectedMethods;
		RedirectedInterfaces = redirectedInterfaces;
	}
}
