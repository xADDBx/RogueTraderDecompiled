using Kingmaker.Pathfinding;
using MemoryPack;
using Pathfinding;

namespace Kingmaker.MemoryPack.Formatters;

[MemoryPackUnionFormatter(typeof(Path))]
[MemoryPackUnion(0, typeof(ForcedPath))]
[MemoryPackUnion(1, typeof(ShipPath))]
[MemoryPackUnion(2, typeof(WarhammerPathPlayer))]
[MemoryPackUnion(3, typeof(WarhammerPathAi))]
[MemoryPackUnion(4, typeof(WarhammerXPath))]
[MemoryPackUnion(5, typeof(WarhammerABPath))]
public class PathFormatter
{
}
