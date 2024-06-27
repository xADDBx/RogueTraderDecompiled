using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil;

[Flags]
[ComVisible(false)]
public enum GenericParameterAttributes : ushort
{
	VarianceMask = 3,
	NonVariant = 0,
	Covariant = 1,
	Contravariant = 2,
	SpecialConstraintMask = 0x1C,
	ReferenceTypeConstraint = 4,
	NotNullableValueTypeConstraint = 8,
	DefaultConstructorConstraint = 0x10
}
