using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public enum MetadataType : byte
{
	Void = 1,
	Boolean = 2,
	Char = 3,
	SByte = 4,
	Byte = 5,
	Int16 = 6,
	UInt16 = 7,
	Int32 = 8,
	UInt32 = 9,
	Int64 = 10,
	UInt64 = 11,
	Single = 12,
	Double = 13,
	String = 14,
	Pointer = 15,
	ByReference = 16,
	ValueType = 17,
	Class = 18,
	Var = 19,
	Array = 20,
	GenericInstance = 21,
	TypedByReference = 22,
	IntPtr = 24,
	UIntPtr = 25,
	FunctionPointer = 27,
	Object = 28,
	MVar = 30,
	RequiredModifier = 31,
	OptionalModifier = 32,
	Sentinel = 65,
	Pinned = 69
}
