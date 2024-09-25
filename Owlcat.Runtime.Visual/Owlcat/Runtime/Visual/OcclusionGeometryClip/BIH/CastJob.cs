using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

[BurstCompile]
internal struct CastJob<TGeometry, TCastGeometry, TFlag> where TGeometry : unmanaged, IGeometry where TCastGeometry : unmanaged, IHierarchyCastGeometry<TGeometry> where TFlag : unmanaged
{
	[BurstCompile]
	private struct Job : IJob
	{
		public ABox sceneBounds;

		[ReadOnly]
		public NativeArray<TCastGeometry> castGeometryArray;

		[ReadOnly]
		public NativeArray<int> castGeometryCountArray;

		public int castGeometryArrayIndex;

		public TFlag intersectionValue;

		[ReadOnly]
		public NativeList<Node> nodeList;

		[ReadOnly]
		public NativeArray<TGeometry> geometryArray;

		[ReadOnly]
		public NativeArray<uint> intersectionIndicesArray;

		[NativeDisableContainerSafetyRestriction]
		[WriteOnly]
		public NativeArray<TFlag> intersectionArray;

		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<CastJobStackFrame> framesStack;

		private IntersectJobDebugData debugData;

		public void Execute()
		{
			int num = castGeometryCountArray[0];
			if (castGeometryArrayIndex >= num)
			{
				return;
			}
			TCastGeometry val = castGeometryArray[castGeometryArrayIndex];
			ABox a = val.GetBounds();
			if (!Intersects(in a, in sceneBounds))
			{
				return;
			}
			int num2 = 0;
			framesStack[num2++] = new CastJobStackFrame
			{
				nodeIndex = 0,
				nodeBounds = sceneBounds,
				axis = 0
			};
			do
			{
				CastJobStackFrame castJobStackFrame = framesStack[--num2];
				int nodeIndex = castJobStackFrame.nodeIndex;
				ABox b = castJobStackFrame.nodeBounds;
				int axis = castJobStackFrame.axis;
				if (!IntersectsAxis(in a, in b, axis) || !val.IntersectsNode(b))
				{
					continue;
				}
				Node node = nodeList[nodeIndex];
				if (node.IsLeafNode)
				{
					int i = node.LeafOffset;
					for (int num3 = node.LeafOffset + node.LeafSize; i < num3; i++)
					{
						TGeometry bounds = geometryArray[i];
						if (val.IntersectsLeaf(bounds))
						{
							(int begin, int end) indexRange = bounds.GetIndexRange();
							int item = indexRange.begin;
							int item2 = indexRange.end;
							for (int j = item; j < item2; j++)
							{
								int index = (int)intersectionIndicesArray[j];
								intersectionArray[index] = intersectionValue;
							}
						}
					}
				}
				else
				{
					int innerSplitAxis = node.InnerSplitAxis;
					ABox nodeBounds = b;
					ABox nodeBounds2 = b;
					nodeBounds.SetMax(node.InnerLeftPlane, innerSplitAxis);
					nodeBounds2.SetMin(node.InnerRightPlane, innerSplitAxis);
					framesStack[num2++] = new CastJobStackFrame
					{
						nodeIndex = node.InnerChildIndex,
						nodeBounds = nodeBounds,
						axis = innerSplitAxis
					};
					framesStack[num2++] = new CastJobStackFrame
					{
						nodeIndex = node.InnerChildIndex + 1,
						nodeBounds = nodeBounds2,
						axis = innerSplitAxis
					};
				}
			}
			while (num2 > 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Intersects(in ABox a, in ABox b)
		{
			if (math.all(a.min < b.max))
			{
				return math.all(a.max > b.min);
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IntersectsAxis(in ABox a, in ABox b, int axis)
		{
			if (a.min[axis] < b.max[axis])
			{
				return a.max[axis] > b.min[axis];
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("OWLCAT_BIH_DEBUG")]
		public void IncrementInnerNodeAabbTestCount()
		{
			debugData.innerNodeAabbTestCount++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("OWLCAT_BIH_DEBUG")]
		public void IncrementInnerNodeGeometryTestCount()
		{
			debugData.innerNodeGeometryTestCount++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("OWLCAT_BIH_DEBUG")]
		public void IncrementLeafNodeTestCount()
		{
			debugData.leafNodeTestCount++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("OWLCAT_BIH_DEBUG")]
		public void UpdateMaxFrameStackSize(int value)
		{
			debugData.maxFrameStackSize = math.max(debugData.maxFrameStackSize, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("OWLCAT_BIH_DEBUG")]
		private void StoreDebugData(in IntersectJobDebugData data)
		{
		}
	}

	private readonly ABox m_SceneBounds;

	private readonly int m_HierarchyDepth;

	private readonly NativeList<Node> m_HierarchyNodeList;

	private readonly NativeArray<TGeometry> m_HierarchyGeometryArray;

	private NativeArray<TCastGeometry> m_CastGeometryArray;

	private readonly NativeArray<int> m_CastGeometryCountArray;

	private NativeArray<TFlag> m_IntersectionArray;

	private NativeArray<uint> m_IntersectionIndicesArray;

	private readonly TFlag m_IntersectionValue;

	private readonly NativeArray<CastJobStackFrame> m_FrameStackArray;

	private NativeArray<IntersectJobDebugData> m_DebugDataArray;

	public CastJob(ABox sceneBounds, int hierarchyDepth, NativeList<Node> hierarchyNodeList, NativeArray<TGeometry> hierarchyGeometryArray, NativeArray<TCastGeometry> castGeometryArray, NativeArray<int> castGeometryCountArray, NativeArray<TFlag> intersectionArray, NativeArray<uint> intersectionIndicesArray, TFlag intersectionValue, NativeArray<CastJobStackFrame> frameStackArray, NativeArray<IntersectJobDebugData> debugDataArray)
	{
		m_SceneBounds = sceneBounds;
		m_HierarchyDepth = hierarchyDepth;
		m_HierarchyNodeList = hierarchyNodeList;
		m_HierarchyGeometryArray = hierarchyGeometryArray;
		m_CastGeometryArray = castGeometryArray;
		m_CastGeometryCountArray = castGeometryCountArray;
		m_IntersectionArray = intersectionArray;
		m_IntersectionIndicesArray = intersectionIndicesArray;
		m_IntersectionValue = intersectionValue;
		m_FrameStackArray = frameStackArray;
		m_DebugDataArray = debugDataArray;
	}

	public JobHandle Schedule(JobHandle dependsOn = default(JobHandle))
	{
		if (m_HierarchyGeometryArray.Length == 0)
		{
			return dependsOn;
		}
		if (m_CastGeometryArray.Length == 0)
		{
			return dependsOn;
		}
		int num = CastJobUtility.EvaluateFrameStackSize(m_HierarchyDepth);
		NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(m_CastGeometryArray.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		int i = 0;
		for (int length = m_CastGeometryArray.Length; i < length; i++)
		{
			Job job = default(Job);
			job.sceneBounds = m_SceneBounds;
			job.castGeometryArray = m_CastGeometryArray;
			job.castGeometryCountArray = m_CastGeometryCountArray;
			job.castGeometryArrayIndex = i;
			job.nodeList = m_HierarchyNodeList;
			job.geometryArray = m_HierarchyGeometryArray;
			job.framesStack = new NativeSlice<CastJobStackFrame>(m_FrameStackArray, i * num, num);
			job.intersectionIndicesArray = m_IntersectionIndicesArray;
			job.intersectionArray = m_IntersectionArray;
			job.intersectionValue = m_IntersectionValue;
			Job jobData = job;
			jobs[i] = jobData.Schedule(dependsOn);
		}
		return jobs.Dispose(JobHandle.CombineDependencies(jobs));
	}
}
