using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public interface ISymbolReader : IDisposable
{
	ISymbolWriterProvider GetWriterProvider();

	bool ProcessDebugHeader(ImageDebugHeader header);

	MethodDebugInformation Read(MethodDefinition method);
}
