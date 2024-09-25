using Unity.Burst;
using Unity.Collections;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct StratagemBuffer
{
	public NativeList<StratagemDescriptor> descriptorList;

	public NativeList<int> cellIndexList;

	public NativeSlice<int> GetCellIndices(int shapeId)
	{
		StratagemDescriptor stratagemDescriptor = descriptorList[shapeId];
		return cellIndexList.AsArray().Slice(stratagemDescriptor.start, stratagemDescriptor.length);
	}
}
