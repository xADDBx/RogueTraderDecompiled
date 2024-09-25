using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia.Blocks;
using Kingmaker.ResourceLinks;

namespace Kingmaker.Blueprints.Encyclopedia;

public interface IPage : INode
{
	BlueprintEncyclopediaNode Parent { get; }

	List<INode> GetRootBranch();

	List<IBlock> GetBlocks();

	List<SpriteLink> GetImages();
}
