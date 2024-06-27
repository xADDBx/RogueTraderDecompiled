using System.IO;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public interface ISymbolWriterProvider
{
	ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName);

	ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream);
}
