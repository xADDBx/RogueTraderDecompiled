using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public interface IMethodDef
{
	string Name { get; }

	int Token { get; }
}
