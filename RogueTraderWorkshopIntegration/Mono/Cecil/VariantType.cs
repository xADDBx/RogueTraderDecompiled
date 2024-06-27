using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public enum VariantType
{
	None = 0,
	I2 = 2,
	I4 = 3,
	R4 = 4,
	R8 = 5,
	CY = 6,
	Date = 7,
	BStr = 8,
	Dispatch = 9,
	Error = 10,
	Bool = 11,
	Variant = 12,
	Unknown = 13,
	Decimal = 14,
	I1 = 16,
	UI1 = 17,
	UI2 = 18,
	UI4 = 19,
	I8 = 20,
	UI8 = 21,
	Int = 22,
	UInt = 23
}
