using System.IO;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public interface ISymbolReaderProvider
{
	ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName);

	ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream);
}
