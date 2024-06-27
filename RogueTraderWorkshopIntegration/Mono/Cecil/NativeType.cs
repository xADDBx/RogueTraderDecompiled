using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public enum NativeType
{
	None = 102,
	Boolean = 2,
	I1 = 3,
	U1 = 4,
	I2 = 5,
	U2 = 6,
	I4 = 7,
	U4 = 8,
	I8 = 9,
	U8 = 10,
	R4 = 11,
	R8 = 12,
	LPStr = 20,
	Int = 31,
	UInt = 32,
	Func = 38,
	Array = 42,
	Currency = 15,
	BStr = 19,
	LPWStr = 21,
	LPTStr = 22,
	FixedSysString = 23,
	IUnknown = 25,
	IDispatch = 26,
	Struct = 27,
	IntF = 28,
	SafeArray = 29,
	FixedArray = 30,
	ByValStr = 34,
	ANSIBStr = 35,
	TBStr = 36,
	VariantBool = 37,
	ASAny = 40,
	LPStruct = 43,
	CustomMarshaler = 44,
	Error = 45,
	Max = 80
}
