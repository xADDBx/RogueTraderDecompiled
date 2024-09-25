using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Visual.GPUHashtable;
using Owlcat.Runtime.Visual.GPUParallelSort;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.PositionBasedDynamics.Broadphase;

public class GPUMultilevelGridBroadphase : GPUBroadphaseBase
{
	private MultilevelGridSettings m_Settings;

	private RadixSorter m_RadixSorter;

	private LinearProbingHashtable m_Hashtable;

	public override BroadphaseType Type => BroadphaseType.MultilevelGrid;

	public GPUMultilevelGridBroadphase(MultilevelGridSettings settings, ComputeShader radixSorterShader, ComputeShader hashtableShader)
	{
		m_Settings = settings;
		m_RadixSorter = new RadixSorter(radixSorterShader);
		m_Hashtable = new LinearProbingHashtable(4000, hashtableShader);
	}

	public override void Dispose()
	{
		base.Dispose();
		m_RadixSorter.Dispose();
	}

	public override void Update(CommandBuffer cmd)
	{
	}
}
