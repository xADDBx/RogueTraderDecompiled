using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

internal struct BuildJob<TGeometry> where TGeometry : unmanaged, IGeometry
{
	[BurstCompile]
	private struct StackFrame
	{
		public int depth;

		public int nodeIndex;

		public int geoOffset;

		public int geoSize;

		public ABox buildBounds;
	}

	[BurstCompile]
	private struct Job : IJob
	{
		private const int kAxisX = 0;

		private const int kAxisY = 1;

		private const int kAxisZ = 2;

		public int maxDepthLimit;

		public int maxLeafSize;

		public ABox sceneBounds;

		[WriteOnly]
		public NativeList<Node> nodeList;

		public NativeArray<TGeometry> geometryArray;

		public NativeArray<TGeometry> geometryStack;

		public NativeArray<StackFrame> frameStack;

		public NativeArray<BihTreeInfo> treeInfoArray;

		public NativeArray<BuildJobDebugInfo> debugInfoArray;

		public void Execute()
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			BuildJobDebugInfo value = debugInfoArray[0];
			ref NativeList<Node> reference = ref nodeList;
			Node value2 = default(Node);
			reference.Add(in value2);
			num++;
			frameStack[num2++] = new StackFrame
			{
				depth = 0,
				nodeIndex = 0,
				geoOffset = 0,
				geoSize = geometryArray.Length,
				buildBounds = sceneBounds
			};
			value.maxFrameStackSize = num2;
			value.minLeafSize = int.MaxValue;
			value.maxLeafSize = int.MinValue;
			do
			{
				StackFrame stackFrame = frameStack[--num2];
				int depth = stackFrame.depth;
				int nodeIndex = stackFrame.nodeIndex;
				int geoOffset = stackFrame.geoOffset;
				int geoSize = stackFrame.geoSize;
				ABox buildBounds = stackFrame.buildBounds;
				num3 = math.max(num3, depth);
				if (depth >= maxDepthLimit || geoSize <= maxLeafSize)
				{
					value.minLeafSize = math.min(value.minLeafSize, geoSize);
					value.maxLeafSize = math.max(value.maxLeafSize, geoSize);
					nodeList[nodeIndex] = Node.MakeLeaf(geoOffset, geoSize);
					continue;
				}
				if (!Sort(in buildBounds, geoOffset, geoSize, out var leftSize, out var leftPlane, out var rightPlane, out var axis))
				{
					value.minLeafSize = math.min(value.minLeafSize, geoSize);
					value.maxLeafSize = math.max(value.maxLeafSize, geoSize);
					nodeList[nodeIndex] = Node.MakeLeaf(geoOffset, geoSize);
					continue;
				}
				int num4 = num;
				ref NativeList<Node> reference2 = ref nodeList;
				value2 = default(Node);
				reference2.Add(in value2);
				ref NativeList<Node> reference3 = ref nodeList;
				value2 = default(Node);
				reference3.Add(in value2);
				num += 2;
				nodeList[nodeIndex] = Node.MakeInner(axis, num4, leftPlane, rightPlane);
				ABox buildBounds2 = buildBounds;
				buildBounds2.SetMax(leftPlane, axis);
				ABox buildBounds3 = buildBounds;
				buildBounds3.SetMin(rightPlane, axis);
				frameStack[num2++] = new StackFrame
				{
					depth = depth + 1,
					nodeIndex = num4,
					geoOffset = geoOffset,
					geoSize = leftSize,
					buildBounds = buildBounds2
				};
				frameStack[num2++] = new StackFrame
				{
					depth = depth + 1,
					nodeIndex = num4 + 1,
					geoOffset = geoOffset + leftSize,
					geoSize = geoSize - leftSize,
					buildBounds = buildBounds3
				};
				value.maxFrameStackSize = math.max(value.maxFrameStackSize, num2);
			}
			while (num2 > 0);
			treeInfoArray[0] = new BihTreeInfo
			{
				depth = num3
			};
			debugInfoArray[0] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Sort(in ABox buildBounds, int geoOffset, int geoSize, out int leftSize, out float leftPlane, out float rightPlane, out int axis)
		{
			float3 size = buildBounds.max - buildBounds.min;
			int num = SelectLongestAxis(in size);
			for (int i = 0; i < 3; i++)
			{
				axis = (num + i) % 3;
				float pivot = math.lerp(buildBounds.min[axis], buildBounds.max[axis], 0.5f);
				Sort(geoOffset, geoSize, pivot, axis, out leftSize, out leftPlane, out rightPlane);
				if (leftSize > 0 && leftSize < geoSize)
				{
					return true;
				}
				pivot = GetAverageCenter(geoOffset, geoSize, axis);
				Sort(geoOffset, geoSize, pivot, axis, out leftSize, out leftPlane, out rightPlane);
				if (leftSize > 0 && leftSize < geoSize)
				{
					return true;
				}
			}
			axis = 0;
			leftSize = 0;
			leftPlane = 0f;
			rightPlane = 0f;
			return false;
		}

		private void Sort(int geoOffset, int geoSize, float pivot, int axis, out int leftSize, out float leftPlane, out float rightPlane)
		{
			int num = geoOffset;
			int num2 = geoOffset + geoSize - 1;
			int num3 = num2;
			leftPlane = float.MinValue;
			rightPlane = float.MaxValue;
			int num4 = 0;
			while (num <= num3)
			{
				TGeometry value = geometryArray[num];
				ABox bounds = value.GetBounds();
				if (bounds.max[axis] <= pivot)
				{
					leftPlane = math.max(leftPlane, bounds.max[axis]);
					num++;
				}
				else if (bounds.min[axis] >= pivot)
				{
					ref NativeArray<TGeometry> reference = ref geometryArray;
					int index = num;
					ref NativeArray<TGeometry> reference2 = ref geometryArray;
					int index2 = num2;
					TGeometry val = geometryArray[num3];
					TGeometry val2 = geometryArray[num];
					TGeometry val4 = (reference[index] = val);
					val4 = (reference2[index2] = val2);
					rightPlane = math.min(rightPlane, bounds.min[axis]);
					num3--;
					num2--;
				}
				else
				{
					geometryStack[num4++] = value;
					geometryArray[num] = geometryArray[num3];
					num3--;
				}
			}
			for (int i = 0; i < num4; i++)
			{
				TGeometry value2 = geometryStack[i];
				ABox bounds2 = value2.GetBounds();
				if (math.lerp(bounds2.min[axis], bounds2.max[axis], 0.5f) < pivot)
				{
					geometryArray[num] = value2;
					leftPlane = math.max(leftPlane, bounds2.max[axis]);
					num++;
				}
				else
				{
					geometryArray[num2] = value2;
					rightPlane = math.min(rightPlane, bounds2.min[axis]);
					num2--;
				}
			}
			leftSize = num - geoOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int SelectLongestAxis(in float3 size)
		{
			if (size.x >= size.y)
			{
				return math.select(2, 0, size.x >= size.z);
			}
			return math.select(2, 1, size.y >= size.z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private float GetAverageCenter(int boundsOffset, int boundsSize, int axis)
		{
			float num = 0f;
			int i = boundsOffset;
			for (int num2 = boundsOffset + boundsSize; i < num2; i++)
			{
				ABox bounds = geometryArray[i].GetBounds();
				num += math.lerp(bounds.min[axis], bounds.max[axis], 0.5f) / (float)boundsSize;
			}
			return num;
		}
	}

	private readonly int m_MaxDepth;

	private readonly int m_MaxGeometryPerLeaf;

	private readonly ABox m_SceneBounds;

	private NativeList<Node> m_NodeList;

	private readonly NativeArray<TGeometry> m_GeometryArray;

	private readonly NativeArray<BihTreeInfo> m_TreeInfoArray;

	private NativeArray<BuildJobDebugInfo> m_DebugInfoArray;

	public BuildJob(int maxDepth, int maxGeometryPerLeaf, ABox sceneBounds, NativeList<Node> nodeList, NativeArray<TGeometry> geometryArray, NativeArray<BihTreeInfo> treeInfoArray, NativeArray<BuildJobDebugInfo> debugInfoArray)
	{
		m_MaxDepth = maxDepth;
		m_MaxGeometryPerLeaf = maxGeometryPerLeaf;
		m_SceneBounds = sceneBounds;
		m_NodeList = nodeList;
		m_GeometryArray = geometryArray;
		m_TreeInfoArray = treeInfoArray;
		m_DebugInfoArray = debugInfoArray;
	}

	public JobHandle Schedule(JobHandle dependsOn = default(JobHandle))
	{
		int num = EstimateNodesCount();
		m_NodeList.Clear();
		m_NodeList.SetCapacity(num);
		BuildJobDebugInfo value = m_DebugInfoArray[0];
		value.estimatedNodesCount = num;
		m_DebugInfoArray[0] = value;
		NativeArray<TGeometry> geometryStack = new NativeArray<TGeometry>(m_GeometryArray.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<StackFrame> frameStack = new NativeArray<StackFrame>(m_MaxDepth + 1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		Job jobData = default(Job);
		jobData.maxDepthLimit = m_MaxDepth;
		jobData.maxLeafSize = m_MaxGeometryPerLeaf;
		jobData.sceneBounds = m_SceneBounds;
		jobData.nodeList = m_NodeList;
		jobData.geometryArray = m_GeometryArray;
		jobData.geometryStack = geometryStack;
		jobData.frameStack = frameStack;
		jobData.treeInfoArray = m_TreeInfoArray;
		jobData.debugInfoArray = m_DebugInfoArray;
		JobHandle inputDeps = jobData.Schedule(dependsOn);
		inputDeps = geometryStack.Dispose(inputDeps);
		return frameStack.Dispose(inputDeps);
	}

	private int EstimateNodesCount()
	{
		uint num = math.ceilpow2((uint)(m_GeometryArray.Length / m_MaxGeometryPerLeaf));
		uint num2 = num;
		while (num != 0)
		{
			num >>= 1;
			num2 += num;
		}
		return (int)num2;
	}
}
