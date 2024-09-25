using Unity.Collections;
using UnityEngine.Jobs;

namespace Kingmaker.Visual.CharacterSystem;

public struct BoneUpdateJob : IJobParallelForTransform
{
	[ReadOnly]
	public NativeArray<Skeleton.BoneData> Scales;

	public void Execute(int index, TransformAccess ta)
	{
		if (ta.isValid)
		{
			ta.localScale = Scales[index].Scale;
			if (Scales[index].ApplyOffset)
			{
				ta.localPosition = Scales[index].Offset;
			}
		}
	}
}
