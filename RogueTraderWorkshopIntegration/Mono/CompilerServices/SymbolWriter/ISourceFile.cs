using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public interface ISourceFile
{
	SourceFileEntry Entry { get; }
}
