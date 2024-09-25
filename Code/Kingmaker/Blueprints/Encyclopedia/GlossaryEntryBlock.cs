using Kingmaker.Blueprints.Encyclopedia.Blocks;

namespace Kingmaker.Blueprints.Encyclopedia;

public class GlossaryEntryBlock : IBlock
{
	public readonly BlueprintEncyclopediaGlossaryEntry Entry;

	public GlossaryEntryBlock(BlueprintEncyclopediaGlossaryEntry glossaryEntry)
	{
		Entry = glossaryEntry;
	}
}
