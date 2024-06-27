using Mono.Cecil.PE;

namespace Mono.Cecil;

internal sealed class DeferredModuleReader : ModuleReader
{
	public DeferredModuleReader(Image image)
		: base(image, ReadingMode.Deferred)
	{
	}

	protected override void ReadModule()
	{
		module.Read(module, delegate(ModuleDefinition _, MetadataReader reader)
		{
			ReadModuleManifest(reader);
		});
	}

	public override void ReadSymbols(ModuleDefinition module)
	{
	}
}
