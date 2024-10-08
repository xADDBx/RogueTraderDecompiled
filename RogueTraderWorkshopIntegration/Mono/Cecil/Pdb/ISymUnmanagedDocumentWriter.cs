using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Pdb;

[ComImport]
[Guid("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISymUnmanagedDocumentWriter
{
	void SetSource(uint sourceSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] source);

	void SetCheckSum(Guid algorithmId, uint checkSumSize, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] checkSum);
}
