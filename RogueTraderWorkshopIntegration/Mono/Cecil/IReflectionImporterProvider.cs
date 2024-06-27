using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IReflectionImporterProvider
{
	IReflectionImporter GetReflectionImporter(ModuleDefinition module);
}
