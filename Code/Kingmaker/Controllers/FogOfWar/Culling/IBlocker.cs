using Unity.Collections;

namespace Kingmaker.Controllers.FogOfWar.Culling;

public interface IBlocker
{
	int CullingRegistryIndex { get; set; }

	void AppendCullingSegments(NativeList<BlockerSegment> results);
}
