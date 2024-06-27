using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public interface ICompileUnit
{
	CompileUnitEntry Entry { get; }
}
