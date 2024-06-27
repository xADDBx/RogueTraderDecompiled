using System.Runtime.InteropServices;

namespace Mono.Cecil;

[ComVisible(false)]
public interface IMetadataImporterProvider
{
	IMetadataImporter GetMetadataImporter(ModuleDefinition module);
}
