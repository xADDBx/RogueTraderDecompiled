using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct ComposeOutlineMeshCommandData
{
	public OutlineType lineType;

	public bool overwrite;

	public float3 meshOffset;

	public OutlineCellFilterData shape;

	public OutlineCellFilterData mask;
}
