using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public interface ISymbolWriter : IDisposable
{
	ISymbolReaderProvider GetReaderProvider();

	ImageDebugHeader GetDebugHeader();

	void Write(MethodDebugInformation info);

	void Write();
}
