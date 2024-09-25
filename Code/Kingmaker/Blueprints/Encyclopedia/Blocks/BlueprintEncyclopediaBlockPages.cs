using System.Collections.Generic;

namespace Kingmaker.Blueprints.Encyclopedia.Blocks;

public class BlueprintEncyclopediaBlockPages : BlueprintEncyclopediaBlock
{
	public enum SourcePages
	{
		ByChild,
		ByList
	}

	public SourcePages Source;

	public List<BlueprintEncyclopediaPageReference> Pages = new List<BlueprintEncyclopediaPageReference>();

	public override string ToString()
	{
		return "Child Pages";
	}
}
