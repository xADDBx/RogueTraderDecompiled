using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;

[BurstCompile]
public struct GridFindCellStartJob : IJob
{
	public int Count;

	public NativeArray<KeyValuePairComparable<uint, uint>> Hash;

	public NativeHashMap<uint, uint> CellStart;

	public void Execute()
	{
		CellStart.Clear();
		uint num = Hash[0].Key;
		CellStart.Add(num, 0u);
		for (int i = 1; i < Count; i++)
		{
			uint key = Hash[i].Key;
			if (key != num)
			{
				if (key == uint.MaxValue)
				{
					break;
				}
				CellStart.Add(key, (uint)i);
				num = key;
			}
		}
	}
}
