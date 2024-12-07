using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Pathfinding.LosCaching;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[JsonOptIn]
[Preserve]
public class CustomGridGraph : NavGraph, IUpdatableGraph, ITransformedGraph, IRaycastableGraph
{
	public class TextureData
	{
		public enum ChannelUse
		{
			None,
			Penalty,
			Position,
			WalkablePenalty
		}

		public bool enabled;

		public Texture2D source;

		public float[] factors = new float[3];

		public ChannelUse[] channels = new ChannelUse[3];

		private Color32[] data;

		public void Initialize()
		{
			if (!enabled || !(source != null))
			{
				return;
			}
			for (int i = 0; i < channels.Length; i++)
			{
				if (channels[i] != 0)
				{
					try
					{
						data = source.GetPixels32();
						break;
					}
					catch (UnityException ex)
					{
						Debug.LogWarning(ex.ToString());
						data = null;
						break;
					}
				}
			}
		}

		public void Apply(CustomGridNode node, int x, int z)
		{
			if (enabled && data != null && x < source.width && z < source.height)
			{
				Color32 color = data[z * source.width + x];
				if (channels[0] != 0)
				{
					ApplyChannel(node, x, z, color.r, channels[0], factors[0]);
				}
				if (channels[1] != 0)
				{
					ApplyChannel(node, x, z, color.g, channels[1], factors[1]);
				}
				if (channels[2] != 0)
				{
					ApplyChannel(node, x, z, color.b, channels[2], factors[2]);
				}
				node.WalkableErosion = node.StaticWalkable;
			}
		}

		private void ApplyChannel(CustomGridNode node, int x, int z, int value, ChannelUse channelUse, float factor)
		{
			switch (channelUse)
			{
			case ChannelUse.Penalty:
				node.Penalty += (uint)Mathf.RoundToInt((float)value * factor);
				break;
			case ChannelUse.Position:
				node.position = CustomGridNode.GetGridGraph(node.GraphIndex).GraphPointToWorld(x, z, value);
				break;
			case ChannelUse.WalkablePenalty:
				if (value == 0)
				{
					node.StaticWalkable = false;
				}
				else
				{
					node.Penalty += (uint)Mathf.RoundToInt((float)(value - 1) * factor);
				}
				break;
			}
		}
	}

	public static int kDataFormatSignature = -12345;

	public static uint kDataFormatVersionLegacy = 0u;

	public static uint kDataFormatVersion = 2u;

	public static uint kBakedSurfaceDataMinVersion = 2u;

	public static int kInvalidNodesCount = -1;

	public CustomGridMeshNode[] meshNodes;

	[JsonMember]
	public InspectorGridMode inspectorGridMode;

	[JsonMember]
	public InspectorGridHexagonNodeSize inspectorHexagonSizeMode;

	public int width;

	public int depth;

	[JsonMember]
	public float aspectRatio = 1f;

	[JsonMember]
	public float isometricAngle;

	[JsonMember]
	public bool uniformEdgeCosts;

	[JsonMember]
	public Vector3 rotation;

	[JsonMember]
	public Vector3 center;

	[JsonMember]
	public bool snapCenterToWorldGrid;

	[JsonMember]
	public Vector2 unclampedSize;

	[JsonMember]
	public float nodeSize = 1f;

	[JsonMember]
	public GraphCollision collision;

	[JsonMember]
	public float maxClimb = 0.4f;

	[JsonMember]
	public float maxSlope = 90f;

	[JsonMember]
	public int erodeIterations;

	[JsonMember]
	public bool erosionUseTags;

	[JsonMember]
	public int erosionFirstTag = 1;

	[JsonMember]
	public NumNeighbours neighbours = NumNeighbours.Eight;

	[JsonMember]
	public bool cutCorners;

	[JsonMember]
	public float penaltyPositionOffset;

	[JsonMember]
	public bool penaltyPosition;

	[JsonMember]
	public float penaltyPositionFactor = 1f;

	[JsonMember]
	public bool penaltyAngle;

	[JsonMember]
	public float penaltyAngleFactor = 100f;

	[JsonMember]
	public float penaltyAnglePower = 1f;

	[JsonMember]
	public bool useJumpPointSearch;

	[JsonMember]
	public bool showMeshOutline = true;

	[JsonMember]
	public bool showNodeConnections;

	[JsonMember]
	public bool showMeshSurface = true;

	[JsonMember]
	public bool showNodeCoordinates;

	[JsonMember]
	public TextureData textureData = new TextureData();

	private NavmeshMasksGeneration m_NavmeshMasks;

	private GridCuts m_GridCuts;

	private GridNavmeshFences m_Fences;

	[NonSerialized]
	public readonly int[] neighbourOffsets = new int[8];

	[NonSerialized]
	public readonly uint[] neighbourCosts = new uint[8];

	[NonSerialized]
	public readonly int[] neighbourXOffsets = new int[8];

	[NonSerialized]
	public readonly int[] neighbourZOffsets = new int[8];

	internal static readonly int[] hexagonNeighbourIndices = new int[6] { 0, 1, 5, 2, 3, 7 };

	public const int getNearestForceOverlap = 2;

	public CustomGridNode[] nodes;

	private bool m_IsScanningNow;

	private LosCache losCache;

	private bool losCacheReady;

	private const float EdgeWalkableCheckOffset = 0.6f;

	private static readonly Vector3[] Offsets = new Vector3[4]
	{
		new Vector3(0.3f, 0f, 0.3f),
		new Vector3(0.3f, 0f, -0.3f),
		new Vector3(-0.3f, 0f, 0.3f),
		new Vector3(-0.3f, 0f, -0.3f)
	};

	private static readonly Vector3[] Cube = new Vector3[36]
	{
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f)
	};

	private static readonly Vector3[] Quad = new Vector3[6]
	{
		new Vector3(-0.5f, 0f, 0.5f),
		new Vector3(0.5f, 0f, 0.5f),
		new Vector3(-0.5f, 0f, -0.5f),
		new Vector3(-0.5f, 0f, -0.5f),
		new Vector3(0.5f, 0f, 0.5f),
		new Vector3(0.5f, 0f, -0.5f)
	};

	public bool MasksChanged = true;

	private Dictionary<int, Color> customColoring = new Dictionary<int, Color>();

	private int customColoringHash;

	public virtual bool uniformWidthDepthGrid => true;

	public virtual int LayerCount => 1;

	protected bool useRaycastNormal => Math.Abs(90f - maxSlope) > float.Epsilon;

	public Vector2 size { get; protected set; }

	public GraphTransform transform { get; private set; }

	public int Width
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	public int Depth
	{
		get
		{
			return depth;
		}
		set
		{
			depth = value;
		}
	}

	private bool showCustomColoring => customColoring.Count > 0;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveGridGraphFromStatic();
	}

	protected override void DestroyAllNodes()
	{
		GetNodes(delegate(GraphNode node)
		{
			(node as CustomGridNodeBase).ClearCustomConnections(alsoReverse: true);
			node.ClearConnections(alsoReverse: false);
			node.Destroy();
		});
	}

	private void RemoveGridGraphFromStatic()
	{
		CustomGridNode.SetGridGraph(AstarPath.active.data.GetGraphIndex(this), null);
	}

	public override int CountNodes()
	{
		if (nodes == null)
		{
			return 0;
		}
		return nodes.Length;
	}

	public override void GetNodes(Action<GraphNode> action)
	{
		if (nodes != null)
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				action(nodes[i]);
			}
		}
	}

	public CustomGridGraph()
	{
		unclampedSize = new Vector2(10f, 10f);
		nodeSize = 1f;
		collision = new GraphCollision();
		transform = new GraphTransform(Matrix4x4.identity);
	}

	private static float GetDistance(Vector3 p1, Vector3 p2, NNConstraint constraint)
	{
		if (!constraint.distanceXZ)
		{
			return (p1 - p2).magnitude;
		}
		return (p1.ToXZ() - p2.ToXZ()).magnitude;
	}

	private static float GetDistanceSqr(Vector3 p1, Vector3 p2, NNConstraint constraint)
	{
		if (!constraint.distanceXZ)
		{
			return (p1 - p2).sqrMagnitude;
		}
		return (p1.ToXZ() - p2.ToXZ()).sqrMagnitude;
	}

	public override void RelocateNodes(Matrix4x4 deltaMatrix)
	{
		throw new Exception("This method cannot be used for Grid Graphs. Please use the other overload of RelocateNodes instead");
	}

	public void RelocateNodes(Vector3 center, Quaternion rotation, float nodeSize, float aspectRatio = 1f, float isometricAngle = 0f)
	{
		GraphTransform previousTransform = transform;
		this.center = center;
		this.rotation = rotation.eulerAngles;
		this.aspectRatio = aspectRatio;
		this.isometricAngle = isometricAngle;
		SetDimensions(width, depth, nodeSize);
		GetNodes(delegate(GraphNode node)
		{
			CustomGridNodeBase customGridNodeBase = node as CustomGridNodeBase;
			float y = previousTransform.InverseTransform((Vector3)node.position).y;
			node.position = GraphPointToWorld(customGridNodeBase.XCoordinateInGrid, customGridNodeBase.ZCoordinateInGrid, y);
		});
	}

	public Int3 GraphPointToWorld(int x, int z, float height)
	{
		return (Int3)transform.Transform(new Vector3((float)x + 0.5f, height, (float)z + 0.5f));
	}

	public static float ConvertHexagonSizeToNodeSize(InspectorGridHexagonNodeSize mode, float value)
	{
		switch (mode)
		{
		case InspectorGridHexagonNodeSize.Diameter:
			value *= 1.5f / (float)Math.Sqrt(2.0);
			break;
		case InspectorGridHexagonNodeSize.Width:
			value *= (float)Math.Sqrt(1.5);
			break;
		}
		return value;
	}

	public static float ConvertNodeSizeToHexagonSize(InspectorGridHexagonNodeSize mode, float value)
	{
		switch (mode)
		{
		case InspectorGridHexagonNodeSize.Diameter:
			value *= (float)Math.Sqrt(2.0) / 1.5f;
			break;
		case InspectorGridHexagonNodeSize.Width:
			value *= (float)Math.Sqrt(0.6666666865348816);
			break;
		}
		return value;
	}

	public uint GetConnectionCost(int dir)
	{
		return neighbourCosts[dir];
	}

	public CustomGridNode GetNodeConnection(CustomGridNode node, int dir)
	{
		if (!node.HasConnectionInDirection(dir))
		{
			return null;
		}
		if (!node.EdgeNode)
		{
			return nodes[node.NodeInGridIndex + neighbourOffsets[dir]];
		}
		int nodeInGridIndex = node.NodeInGridIndex;
		int num = nodeInGridIndex / Width;
		int x = nodeInGridIndex - num * Width;
		return GetNodeConnection(nodeInGridIndex, x, num, dir);
	}

	public bool HasNodeConnection(CustomGridNode node, int dir)
	{
		if (!node.HasConnectionInDirection(dir))
		{
			return false;
		}
		if (!node.EdgeNode)
		{
			return true;
		}
		int nodeInGridIndex = node.NodeInGridIndex;
		int num = nodeInGridIndex / Width;
		int x = nodeInGridIndex - num * Width;
		return HasNodeConnection(nodeInGridIndex, x, num, dir);
	}

	public void SetNodeConnection(CustomGridNode node, int dir, bool value)
	{
		int nodeInGridIndex = node.NodeInGridIndex;
		int num = nodeInGridIndex / Width;
		int x = nodeInGridIndex - num * Width;
		SetNodeConnection(nodeInGridIndex, x, num, dir, value);
	}

	private CustomGridNode GetNodeConnection(int index, int x, int z, int dir)
	{
		if (!nodes[index].HasConnectionInDirection(dir))
		{
			return null;
		}
		int num = x + neighbourXOffsets[dir];
		if (num < 0 || num >= Width)
		{
			return null;
		}
		int num2 = z + neighbourZOffsets[dir];
		if (num2 < 0 || num2 >= Depth)
		{
			return null;
		}
		int num3 = index + neighbourOffsets[dir];
		return nodes[num3];
	}

	public void SetNodeConnection(int index, int x, int z, int dir, bool value)
	{
		nodes[index].SetConnectionInternal(dir, value);
	}

	public bool HasNodeConnection(int index, int x, int z, int dir)
	{
		if (!nodes[index].HasConnectionInDirection(dir))
		{
			return false;
		}
		int num = x + neighbourXOffsets[dir];
		if (num < 0 || num >= Width)
		{
			return false;
		}
		int num2 = z + neighbourZOffsets[dir];
		if (num2 < 0 || num2 >= Depth)
		{
			return false;
		}
		return true;
	}

	public void SetDimensions(int width, int depth, float nodeSize)
	{
		unclampedSize = new Vector2(width, depth) * nodeSize;
		this.nodeSize = nodeSize;
		UpdateTransform();
	}

	[Obsolete("Use SetDimensions instead")]
	public void UpdateSizeFromWidthDepth()
	{
		SetDimensions(width, depth, nodeSize);
	}

	[Obsolete("This method has been renamed to UpdateTransform")]
	public void GenerateMatrix()
	{
		UpdateTransform();
	}

	public void UpdateTransform()
	{
		CalculateDimensions(out width, out depth, out nodeSize);
		transform = CalculateTransform();
	}

	public GraphTransform CalculateTransform()
	{
		CalculateDimensions(out var num, out var num2, out var num3);
		Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 45f, 0f), Vector3.one);
		matrix4x = Matrix4x4.Scale(new Vector3(Mathf.Cos(MathF.PI / 180f * isometricAngle), 1f, 1f)) * matrix4x;
		matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, -45f, 0f), Vector3.one) * matrix4x;
		return new GraphTransform(Matrix4x4.TRS((Matrix4x4.TRS(center, Quaternion.Euler(rotation), new Vector3(aspectRatio, 1f, 1f)) * matrix4x).MultiplyPoint3x4(-new Vector3((float)num * num3, 0f, (float)num2 * num3) * 0.5f), Quaternion.Euler(rotation), new Vector3(num3 * aspectRatio, 1f, num3)) * matrix4x);
	}

	private void CalculateDimensions(out int width, out int depth, out float nodeSize)
	{
		Vector2 vector = unclampedSize;
		vector.x *= Mathf.Sign(vector.x);
		vector.y *= Mathf.Sign(vector.y);
		nodeSize = Mathf.Max(this.nodeSize, vector.x / 1024f);
		nodeSize = Mathf.Max(this.nodeSize, vector.y / 1024f);
		vector.x = ((vector.x < nodeSize) ? nodeSize : vector.x);
		vector.y = ((vector.y < nodeSize) ? nodeSize : vector.y);
		size = vector;
		width = Mathf.FloorToInt(size.x / nodeSize);
		depth = Mathf.FloorToInt(size.y / nodeSize);
		if (Mathf.Approximately(size.x / nodeSize, Mathf.CeilToInt(size.x / nodeSize)))
		{
			width = Mathf.CeilToInt(size.x / nodeSize);
		}
		if (Mathf.Approximately(size.y / nodeSize, Mathf.CeilToInt(size.y / nodeSize)))
		{
			depth = Mathf.CeilToInt(size.y / nodeSize);
		}
	}

	public Int2 GetNearestNodeCoords(Vector3 position)
	{
		position = transform.InverseTransform(position);
		return new Int2(Mathf.Clamp((int)position.x, 0, width - 1), Mathf.Clamp((int)position.z, 0, depth - 1));
	}

	public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
	{
		if (nodes == null || depth * width != nodes.Length)
		{
			return default(NNInfoInternal);
		}
		position = transform.InverseTransform(position);
		float x = position.x;
		float z = position.z;
		int num = Mathf.Clamp((int)x, 0, width - 1);
		int num2 = Mathf.Clamp((int)z, 0, depth - 1);
		NNInfoInternal result = new NNInfoInternal(nodes[num2 * width + num]);
		float y = transform.InverseTransform((Vector3)nodes[num2 * width + num].position).y;
		result.clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(x, num, (float)num + 1f), y, Mathf.Clamp(z, num2, (float)num2 + 1f)));
		return result;
	}

	public CustomGridNode GetNearestDirect(Vector3 position)
	{
		if (nodes == null || depth * width != nodes.Length)
		{
			return null;
		}
		position = transform.InverseTransform(position);
		float x = position.x;
		float z = position.z;
		int num = (int)x;
		int num2 = (int)z;
		if (num < 0 || num > width - 1)
		{
			return null;
		}
		if (num2 < 0 || num2 > depth - 1)
		{
			return null;
		}
		return nodes[num2 * width + num];
	}

	public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
	{
		if (nodes == null || depth * width != nodes.Length)
		{
			return default(NNInfoInternal);
		}
		Vector3 p = position;
		position = transform.InverseTransform(position);
		float x = position.x;
		float z = position.z;
		int num = Mathf.Clamp((int)x, 0, width - 1);
		int num2 = Mathf.Clamp((int)z, 0, depth - 1);
		CustomGridNode customGridNode = nodes[num + num2 * width];
		CustomGridNode customGridNode2 = null;
		float num3 = float.PositiveInfinity;
		int num4 = 2;
		Vector3 clampedPosition = Vector3.zero;
		NNInfoInternal result = new NNInfoInternal(null);
		if (constraint == null || constraint.Suitable(customGridNode))
		{
			customGridNode2 = customGridNode;
			num3 = GetDistanceSqr((Vector3)customGridNode2.position, p, constraint);
			float y = transform.InverseTransform((Vector3)customGridNode.position).y;
			clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(x, num, (float)num + 1f), y, Mathf.Clamp(z, num2, (float)num2 + 1f)));
		}
		if (customGridNode2 != null)
		{
			result.node = customGridNode2;
			result.clampedPosition = clampedPosition;
			if (num4 == 0)
			{
				return result;
			}
			num4--;
		}
		float num5 = ((constraint == null || constraint.constrainDistance) ? AstarPath.active.maxNearestNodeDistance : float.PositiveInfinity);
		float num6 = num5 * num5;
		for (int i = 1; !(nodeSize * (float)i > num5); i++)
		{
			bool flag = false;
			int num7 = num2 + i;
			int num8 = num7 * width;
			int j;
			for (j = num - i; j <= num + i; j++)
			{
				if (j < 0 || num7 < 0 || j >= width || num7 >= depth)
				{
					continue;
				}
				flag = true;
				if (constraint == null || constraint.Suitable(nodes[j + num8]))
				{
					float distanceSqr = GetDistanceSqr((Vector3)nodes[j + num8].position, p, constraint);
					if (distanceSqr < num3 && distanceSqr < num6)
					{
						num3 = distanceSqr;
						customGridNode2 = nodes[j + num8];
						clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(x, j, (float)j + 1f), transform.InverseTransform((Vector3)customGridNode2.position).y, Mathf.Clamp(z, num7, (float)num7 + 1f)));
					}
				}
			}
			num7 = num2 - i;
			num8 = num7 * width;
			for (j = num - i; j <= num + i; j++)
			{
				if (j < 0 || num7 < 0 || j >= width || num7 >= depth)
				{
					continue;
				}
				flag = true;
				if (constraint == null || constraint.Suitable(nodes[j + num8]))
				{
					float distanceSqr2 = GetDistanceSqr((Vector3)nodes[j + num8].position, p, constraint);
					if (distanceSqr2 < num3 && distanceSqr2 < num6)
					{
						num3 = distanceSqr2;
						customGridNode2 = nodes[j + num8];
						clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(x, j, (float)j + 1f), transform.InverseTransform((Vector3)customGridNode2.position).y, Mathf.Clamp(z, num7, (float)num7 + 1f)));
					}
				}
			}
			j = num - i;
			for (num7 = num2 - i + 1; num7 <= num2 + i - 1; num7++)
			{
				if (j < 0 || num7 < 0 || j >= width || num7 >= depth)
				{
					continue;
				}
				flag = true;
				if (constraint == null || constraint.Suitable(nodes[j + num7 * width]))
				{
					float distanceSqr3 = GetDistanceSqr((Vector3)nodes[j + num7 * width].position, p, constraint);
					if (distanceSqr3 < num3 && distanceSqr3 < num6)
					{
						num3 = distanceSqr3;
						customGridNode2 = nodes[j + num7 * width];
						clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(x, j, (float)j + 1f), transform.InverseTransform((Vector3)customGridNode2.position).y, Mathf.Clamp(z, num7, (float)num7 + 1f)));
					}
				}
			}
			j = num + i;
			for (num7 = num2 - i + 1; num7 <= num2 + i - 1; num7++)
			{
				if (j < 0 || num7 < 0 || j >= width || num7 >= depth)
				{
					continue;
				}
				flag = true;
				if (constraint == null || constraint.Suitable(nodes[j + num7 * width]))
				{
					float distanceSqr4 = GetDistanceSqr((Vector3)nodes[j + num7 * width].position, p, constraint);
					if (distanceSqr4 < num3 && distanceSqr4 < num6)
					{
						num3 = distanceSqr4;
						customGridNode2 = nodes[j + num7 * width];
						clampedPosition = transform.Transform(new Vector3(Mathf.Clamp(x, j, (float)j + 1f), transform.InverseTransform((Vector3)customGridNode2.position).y, Mathf.Clamp(z, num7, (float)num7 + 1f)));
					}
				}
			}
			if (customGridNode2 != null)
			{
				if (num4 == 0)
				{
					break;
				}
				num4--;
			}
			if (!flag)
			{
				break;
			}
		}
		result.node = customGridNode2;
		result.clampedPosition = clampedPosition;
		return result;
	}

	public virtual void SetUpOffsetsAndCosts()
	{
		neighbourOffsets[0] = -width;
		neighbourOffsets[1] = 1;
		neighbourOffsets[2] = width;
		neighbourOffsets[3] = -1;
		neighbourOffsets[4] = -width + 1;
		neighbourOffsets[5] = width + 1;
		neighbourOffsets[6] = width - 1;
		neighbourOffsets[7] = -width - 1;
		uint num = (uint)Mathf.RoundToInt(nodeSize * 1000f);
		uint num2 = (uniformEdgeCosts ? num : ((uint)Mathf.RoundToInt(nodeSize * Mathf.Sqrt(2f) * 1000f)));
		neighbourCosts[0] = num;
		neighbourCosts[1] = num;
		neighbourCosts[2] = num;
		neighbourCosts[3] = num;
		neighbourCosts[4] = num2;
		neighbourCosts[5] = num2;
		neighbourCosts[6] = num2;
		neighbourCosts[7] = num2;
		neighbourXOffsets[0] = 0;
		neighbourXOffsets[1] = 1;
		neighbourXOffsets[2] = 0;
		neighbourXOffsets[3] = -1;
		neighbourXOffsets[4] = 1;
		neighbourXOffsets[5] = 1;
		neighbourXOffsets[6] = -1;
		neighbourXOffsets[7] = -1;
		neighbourZOffsets[0] = -1;
		neighbourZOffsets[1] = 0;
		neighbourZOffsets[2] = 1;
		neighbourZOffsets[3] = 0;
		neighbourZOffsets[4] = -1;
		neighbourZOffsets[5] = 1;
		neighbourZOffsets[6] = 1;
		neighbourZOffsets[7] = -1;
	}

	protected override IEnumerable<Progress> ScanInternal()
	{
		if (collision == null)
		{
			collision = new GraphCollision();
		}
		int originalHeightMask = collision.heightMask;
		int originalObstacleMask = collision.mask;
		m_IsScanningNow = true;
		GraphCollision graphCollision = collision;
		graphCollision.mask = (int)graphCollision.mask & -524289;
		GraphCollision graphCollision2 = collision;
		graphCollision2.heightMask = (int)graphCollision2.heightMask & -262145;
		GraphCollision graphCollision3 = collision;
		graphCollision3.heightMask = (int)graphCollision3.heightMask & -524289;
		Exception exception = null;
		IEnumerator<Progress> scanProcess = DoScan();
		while (true)
		{
			try
			{
				if (!scanProcess.MoveNext())
				{
					break;
				}
			}
			catch (Exception ex)
			{
				exception = ex;
				break;
			}
			yield return scanProcess.Current;
		}
		m_IsScanningNow = false;
		collision.mask = originalObstacleMask;
		collision.heightMask = originalHeightMask;
		if (exception != null)
		{
			throw exception;
		}
	}

	private IEnumerator<Progress> DoScan()
	{
		losCache = null;
		losCacheReady = false;
		if (nodeSize <= 0f)
		{
			yield break;
		}
		UpdateTransform();
		if (width > 1024 || depth > 1024)
		{
			Debug.LogError("One of the grid's sides is longer than 1024 nodes");
			yield break;
		}
		if (useJumpPointSearch)
		{
			Debug.LogError("Trying to use Jump Point Search, but support for it is not enabled. Please enable it in the inspector (Grid Graph settings).");
		}
		SetUpOffsetsAndCosts();
		CustomGridNode.SetGridGraph((int)graphIndex, this);
		yield return new Progress(0.05f, "Creating nodes");
		nodes = new CustomGridNode[width * depth];
		for (int i = 0; i < depth; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int num = i * width + j;
				CustomGridNode obj = (nodes[num] = new CustomGridNode(active));
				obj.GraphIndex = graphIndex;
				obj.NodeInGridIndex = num;
			}
		}
		collision.Initialize(transform, nodeSize);
		textureData.Initialize();
		m_NavmeshMasks = AstarData.active.GetComponent<NavmeshMasks>()?.ConvertData() ?? default(NavmeshMasksGeneration);
		m_GridCuts = GridCuts.Create(transform);
		m_Fences = GridNavmeshFences.Create(transform);
		int progressCounter = 0;
		for (int z = 0; z < depth; z++)
		{
			if (progressCounter >= 1000)
			{
				progressCounter = 0;
				yield return new Progress(Mathf.Lerp(0.1f, 0.7f, (float)z / (float)depth), "Calculating positions");
			}
			progressCounter += width;
			for (int k = 0; k < width; k++)
			{
				RecalculateCell(k, z);
				textureData.Apply(nodes[z * width + k], k, z);
			}
		}
		progressCounter = 0;
		for (int z = 0; z < depth; z++)
		{
			if (progressCounter >= 1000)
			{
				progressCounter = 0;
				yield return new Progress(Mathf.Lerp(0.7f, 0.8f, (float)z / (float)depth), "Calculating connections");
			}
			progressCounter += width;
			for (int l = 0; l < width; l++)
			{
				CalculateConnections(l, z);
			}
		}
		yield return new Progress(0.9f, "Calculating erosion");
		ErodeWalkableArea();
		meshNodes = new CustomGridMeshNodeBaker(this).Bake();
	}

	public void InitLosCache()
	{
		InitLosCache(new IntRect(0, 0, Width - 1, Depth - 1));
	}

	public void InitLosCache(Bounds bounds)
	{
		InitLosCache(GetRectFromBounds(bounds));
	}

	public void InitLosCache(IntRect rect)
	{
	}

	public bool TryCheckLos(CustomGridNodeBase origin, IntRect originSize, CustomGridNodeBase end, IntRect endSize, out bool hasLos)
	{
		if (!losCacheReady)
		{
			hasLos = false;
			return false;
		}
		hasLos = losCache.CheckLos(origin, originSize, end, endSize);
		return true;
	}

	[Obsolete("Use RecalculateCell instead which works both for grid graphs and layered grid graphs")]
	public virtual void UpdateNodePositionCollision(CustomGridNode node, int x, int z, bool resetPenalty = true)
	{
		RecalculateCell(x, z, resetPenalty, resetTags: false);
	}

	public virtual void RecalculateCell(int x, int z, bool resetPenalties = true, bool resetTags = true)
	{
		CustomGridNode customGridNode = nodes[z * width + x];
		customGridNode.position = GraphPointToWorld(x, z, 0f);
		RaycastHit hit;
		bool walkable;
		Vector3 vector = collision.CheckHeight((Vector3)customGridNode.position, out hit, out walkable, 200f);
		float num = Mathf.Sin(maxSlope * (MathF.PI / 180f));
		float a = vector.y;
		float num2 = vector.y * 4f;
		for (int i = 0; i < Offsets.Length; i++)
		{
			Vector3 position = (Vector3)customGridNode.position + Offsets[i] * nodeSize;
			RaycastHit hit2;
			bool walkable2;
			Vector3 vector2 = collision.CheckHeight(position, out hit2, out walkable2, 200f);
			num2 += vector2.y;
			a = Mathf.Max(a, vector2.y);
			walkable = walkable && walkable2;
			float f = Vector3.Dot((vector2 - vector).normalized, Vector3.up);
			walkable &= Mathf.Abs(f) < num;
		}
		num2 /= (float)(Offsets.Length + 4);
		vector.y = (walkable ? num2 : vector.y);
		customGridNode.position = (Int3)vector;
		customGridNode.ResetFences();
		if (resetPenalties)
		{
			customGridNode.Penalty = initialPenalty;
			if (penaltyPosition)
			{
				customGridNode.Penalty += (uint)Mathf.RoundToInt(((float)customGridNode.position.y - penaltyPositionOffset) * penaltyPositionFactor);
			}
		}
		if (resetTags)
		{
			customGridNode.Tag = 0u;
		}
		if (m_NavmeshMasks.IsRemoved(x, z) || (!m_IsScanningNow && m_GridCuts.IsRemoved(x, z)))
		{
			customGridNode.StaticWalkable = false;
			customGridNode.WalkableErosion = customGridNode.StaticWalkable;
			return;
		}
		if (m_NavmeshMasks.IsAdded(x, z) || (!m_IsScanningNow && m_GridCuts.IsAdded(x, z)))
		{
			customGridNode.StaticWalkable = true;
			customGridNode.WalkableErosion = customGridNode.StaticWalkable;
			return;
		}
		if (walkable && useRaycastNormal && collision.heightCheck && hit.normal != Vector3.zero)
		{
			float num3 = Vector3.Dot(hit.normal.normalized, collision.up);
			if (penaltyAngle && resetPenalties)
			{
				customGridNode.Penalty += (uint)Mathf.RoundToInt((1f - Mathf.Pow(num3, penaltyAnglePower)) * penaltyAngleFactor);
			}
			float num4 = Mathf.Cos(maxSlope * (MathF.PI / 180f));
			if (num3 < num4)
			{
				walkable = false;
			}
		}
		walkable = walkable && collision.Check((Vector3)customGridNode.position);
		customGridNode.StaticWalkable = walkable;
		customGridNode.WalkableErosion = customGridNode.StaticWalkable;
	}

	protected virtual bool ErosionAnyFalseConnections(GraphNode baseNode)
	{
		CustomGridNode node = baseNode as CustomGridNode;
		if (neighbours == NumNeighbours.Six)
		{
			for (int i = 0; i < 6; i++)
			{
				if (!HasNodeConnection(node, hexagonNeighbourIndices[i]))
				{
					return true;
				}
			}
		}
		else
		{
			for (int j = 0; j < 4; j++)
			{
				if (!HasNodeConnection(node, j))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ErodeNode(GraphNode node)
	{
		if (node.StaticWalkable && ErosionAnyFalseConnections(node))
		{
			node.StaticWalkable = false;
		}
	}

	private void ErodeNodeWithTagsInit(GraphNode node)
	{
		if (node.StaticWalkable && ErosionAnyFalseConnections(node))
		{
			node.Tag = (uint)erosionFirstTag;
		}
		else
		{
			node.Tag = 0u;
		}
	}

	private void ErodeNodeWithTags(GraphNode node, int iteration)
	{
		CustomGridNodeBase customGridNodeBase = node as CustomGridNodeBase;
		if (!customGridNodeBase.StaticWalkable || customGridNodeBase.Tag < erosionFirstTag || customGridNodeBase.Tag >= erosionFirstTag + iteration)
		{
			return;
		}
		if (neighbours == NumNeighbours.Six)
		{
			for (int i = 0; i < 6; i++)
			{
				CustomGridNodeBase neighbourAlongDirection = customGridNodeBase.GetNeighbourAlongDirection(hexagonNeighbourIndices[i]);
				if (neighbourAlongDirection != null)
				{
					uint tag = neighbourAlongDirection.Tag;
					if (tag > erosionFirstTag + iteration || tag < erosionFirstTag)
					{
						neighbourAlongDirection.Tag = (uint)(erosionFirstTag + iteration);
					}
				}
			}
			return;
		}
		for (int j = 0; j < 4; j++)
		{
			CustomGridNodeBase neighbourAlongDirection2 = customGridNodeBase.GetNeighbourAlongDirection(j);
			if (neighbourAlongDirection2 != null)
			{
				uint tag2 = neighbourAlongDirection2.Tag;
				if (tag2 > erosionFirstTag + iteration || tag2 < erosionFirstTag)
				{
					neighbourAlongDirection2.Tag = (uint)(erosionFirstTag + iteration);
				}
			}
		}
	}

	public virtual void ErodeWalkableArea()
	{
		ErodeWalkableArea(0, 0, Width, Depth);
	}

	public void ErodeWalkableArea(int xmin, int zmin, int xmax, int zmax)
	{
		if (erosionUseTags)
		{
			if (erodeIterations + erosionFirstTag > 31)
			{
				Debug.LogError("Too few tags available for " + erodeIterations + " erode iterations and starting with tag " + erosionFirstTag + " (erodeIterations+erosionFirstTag > 31)", active);
				return;
			}
			if (erosionFirstTag <= 0)
			{
				Debug.LogError("First erosion tag must be greater or equal to 1", active);
				return;
			}
		}
		if (erodeIterations == 0)
		{
			return;
		}
		IntRect rect = new IntRect(xmin, zmin, xmax - 1, zmax - 1);
		List<GraphNode> list = GetNodesInRegion(rect);
		int count = list.Count;
		for (int i = 0; i < erodeIterations; i++)
		{
			if (erosionUseTags)
			{
				if (i == 0)
				{
					for (int j = 0; j < count; j++)
					{
						ErodeNodeWithTagsInit(list[j]);
					}
				}
				else
				{
					for (int k = 0; k < count; k++)
					{
						ErodeNodeWithTags(list[k], i);
					}
				}
			}
			else
			{
				for (int l = 0; l < count; l++)
				{
					ErodeNode(list[l]);
				}
				for (int m = 0; m < count; m++)
				{
					CalculateConnections(list[m] as CustomGridNodeBase);
				}
			}
		}
		global::Pathfinding.Util.ListPool<GraphNode>.Release(ref list);
	}

	public virtual bool IsValidConnection(CustomGridNodeBase node1, CustomGridNodeBase node2)
	{
		if (!node1.StaticWalkable || !node2.StaticWalkable)
		{
			return false;
		}
		if (node1.IsConnectionCut(node2, out var _))
		{
			return false;
		}
		if (maxClimb <= 0f || collision.use2D)
		{
			return true;
		}
		if (transform.matrixHasNoRotation)
		{
			return (float)Math.Abs(node1.position.y - node2.position.y) <= maxClimb * 1000f;
		}
		Vector3 vector = (Vector3)node1.position;
		Vector3 rhs = (Vector3)node2.position;
		Vector3 lhs = transform.WorldUpAtGraphPosition(vector);
		return Math.Abs(Vector3.Dot(lhs, vector) - Vector3.Dot(lhs, rhs)) <= maxClimb;
	}

	public void CalculateConnectionsForCellAndNeighbours(int x, int z)
	{
		CalculateConnections(x, z);
		for (int i = 0; i < 8; i++)
		{
			int num = x + neighbourXOffsets[i];
			int num2 = z + neighbourZOffsets[i];
			if ((num >= 0 && num2 >= 0) & (num < width) & (num2 < depth))
			{
				CalculateConnections(num, num2);
			}
		}
	}

	[Obsolete("Use the instance function instead")]
	public static void CalculateConnections(CustomGridNode node)
	{
		(AstarData.GetGraph(node) as CustomGridGraph).CalculateConnections((CustomGridNodeBase)node);
	}

	public virtual void CalculateConnections(CustomGridNodeBase node)
	{
		int nodeInGridIndex = node.NodeInGridIndex;
		int x = nodeInGridIndex % width;
		int z = nodeInGridIndex / width;
		CalculateConnections(x, z);
	}

	[Obsolete("Use CalculateConnections(x,z) or CalculateConnections(node) instead")]
	public virtual void CalculateConnections(int x, int z, CustomGridNode node)
	{
		CalculateConnections(x, z);
	}

	public virtual void CalculateConnections(int x, int z)
	{
		CustomGridNode customGridNode = nodes[z * width + x];
		if (!customGridNode.StaticWalkable)
		{
			customGridNode.ResetConnectionsInternal();
			return;
		}
		int nodeInGridIndex = customGridNode.NodeInGridIndex;
		if (neighbours == NumNeighbours.Four || neighbours == NumNeighbours.Eight)
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				int num2 = x + neighbourXOffsets[i];
				int num3 = z + neighbourZOffsets[i];
				if ((num2 >= 0 && num3 >= 0) & (num2 < width) & (num3 < depth))
				{
					CustomGridNode node = nodes[nodeInGridIndex + neighbourOffsets[i]];
					if (m_Fences.IsFenceBetween(customGridNode, node, out var fence))
					{
						customGridNode.SetFenceAlongDirection(i, fence.Value.height);
					}
					if (IsValidConnection(customGridNode, node))
					{
						num |= 1 << i;
					}
				}
			}
			int num4 = 0;
			if (neighbours == NumNeighbours.Eight)
			{
				for (int j = 0; j < 4; j++)
				{
					if (((uint)(num >> j) & (true ? 1u : 0u)) != 0 && ((uint)((num >> j + 1) | (num >> j + 1 - 4)) & (true ? 1u : 0u)) != 0)
					{
						int num5 = j + 4;
						CustomGridNode node2 = nodes[nodeInGridIndex + neighbourOffsets[num5]];
						if (m_Fences.IsFenceBetween(customGridNode, node2, out var fence2))
						{
							customGridNode.SetFenceAlongDirection(num5, fence2.Value.height);
						}
						if (IsValidConnection(customGridNode, node2))
						{
							num4 |= 1 << j + 4;
						}
					}
				}
			}
			customGridNode.SetAllConnectionInternal(num | num4);
			return;
		}
		customGridNode.ResetConnectionsInternal();
		for (int k = 0; k < hexagonNeighbourIndices.Length; k++)
		{
			int num6 = hexagonNeighbourIndices[k];
			int num7 = x + neighbourXOffsets[num6];
			int num8 = z + neighbourZOffsets[num6];
			if ((num7 >= 0 && num8 >= 0) & (num7 < width) & (num8 < depth))
			{
				CustomGridNode node3 = nodes[nodeInGridIndex + neighbourOffsets[num6]];
				customGridNode.SetConnectionInternal(num6, IsValidConnection(customGridNode, node3));
			}
		}
	}

	public override void OnDrawGizmos(RetainedGizmos gizmos, bool drawNodes)
	{
		using (GraphGizmoHelper graphGizmoHelper = gizmos.GetSingleFrameGizmoHelper(active))
		{
			CalculateDimensions(out var num, out var num2, out var _);
			Bounds bounds = default(Bounds);
			bounds.SetMinMax(Vector3.zero, new Vector3(num, 0f, num2));
			GraphTransform tr = CalculateTransform();
			graphGizmoHelper.builder.DrawWireCube(tr, bounds, Color.white);
			if (nodes != null)
			{
				_ = nodes.Length;
			}
		}
		if (!drawNodes)
		{
			return;
		}
		CustomGridNodeBase[] array = ArrayPool<CustomGridNodeBase>.Claim(1024 * LayerCount);
		for (int num4 = width / 32; num4 >= 0; num4--)
		{
			for (int num5 = depth / 32; num5 >= 0; num5--)
			{
				int nodesInRegion = GetNodesInRegion(new IntRect(num4 * 32, num5 * 32, (num4 + 1) * 32 - 1, (num5 + 1) * 32 - 1), array);
				RetainedGizmos.Hasher hasher = new RetainedGizmos.Hasher(active);
				hasher.AddHash(showMeshOutline ? 1 : 0);
				hasher.AddHash(showMeshSurface ? 1 : 0);
				hasher.AddHash(showNodeConnections ? 1 : 0);
				hasher.AddHash(active.showUnwalkableNodes ? 1 : 0);
				hasher.AddHash(showCustomColoring ? customColoringHash : 0);
				for (int i = 0; i < nodesInRegion; i++)
				{
					hasher.HashNode(array[i]);
				}
				if (!gizmos.Draw(hasher))
				{
					using GraphGizmoHelper graphGizmoHelper2 = gizmos.GetGizmoHelper(active, hasher);
					if (showNodeConnections)
					{
						for (int j = 0; j < nodesInRegion; j++)
						{
							if (array[j].Walkable)
							{
								graphGizmoHelper2.DrawConnections(array[j]);
							}
						}
					}
					if (showMeshSurface || showMeshOutline)
					{
						CreateNavmeshSurfaceVisualization(array, nodesInRegion, graphGizmoHelper2, isCustomColoring: false);
					}
					if (showCustomColoring)
					{
						CreateNavmeshSurfaceVisualization(array, nodesInRegion, graphGizmoHelper2, isCustomColoring: true);
					}
					if (active.showUnwalkableNodes)
					{
						DrawUnwalkable(graphGizmoHelper2, array, nodesInRegion, in Quad, nodeSize * 0.3f);
					}
				}
			}
		}
		ArrayPool<CustomGridNodeBase>.Release(ref array);
	}

	private static void DrawUnwalkable(GraphGizmoHelper helper, CustomGridNodeBase[] allNodes, int allNodesCount, in Vector3[] mesh, float size)
	{
		int num = allNodes.Take(allNodesCount).Count((CustomGridNodeBase v) => !v.Walkable);
		Vector3[] array = ArrayPool<Vector3>.Claim(num * mesh.Length);
		Color[] array2 = ArrayPool<Color>.Claim(num * mesh.Length);
		Span<Vector3> span = array;
		Span<Color> span2 = array2;
		int num2 = 0;
		for (int i = 0; i < allNodesCount; i++)
		{
			if (!allNodes[i].Walkable)
			{
				int num3 = num2;
				Span<Vector3> span3 = span;
				int num4 = num2;
				Span<Vector3> vertices = span3.Slice(num4, span3.Length - num4);
				Span<Color> span4 = span2;
				num4 = num2;
				num2 = num3 + EmitMesh(vertices, span4.Slice(num4, span4.Length - num4), (Vector3)allNodes[i].position, size, in mesh);
			}
		}
		helper.DrawTriangles(array, array2, num * mesh.Length / 3);
		ArrayPool<Vector3>.Release(ref array);
		ArrayPool<Color>.Release(ref array2);
	}

	private static int EmitMesh(Span<Vector3> vertices, Span<Color> colors, Vector3 position, float size, in Vector3[] mesh)
	{
		for (int i = 0; i < mesh.Length; i++)
		{
			vertices[i] = mesh[i] * size + position;
			colors[i] = AstarColor.UnwalkableNode;
		}
		return mesh.Length;
	}

	private void CreateNavmeshSurfaceVisualization(CustomGridNodeBase[] nodes, int nodeCount, GraphGizmoHelper helper, bool isCustomColoring)
	{
		int num = 0;
		for (int i = 0; i < nodeCount; i++)
		{
			if (nodes[i].Walkable)
			{
				num++;
			}
		}
		int[] array = ((neighbours == NumNeighbours.Six) ? hexagonNeighbourIndices : new int[4] { 0, 1, 2, 3 });
		float num2 = ((neighbours == NumNeighbours.Six) ? 0.333333f : 0.5f);
		int num3 = array.Length - 2;
		int num4 = 3 * num3;
		Vector3[] array2 = ArrayPool<Vector3>.Claim(num * num4);
		Color[] array3 = ArrayPool<Color>.Claim(num * num4);
		int num5 = 0;
		for (int j = 0; j < nodeCount; j++)
		{
			CustomGridNodeBase customGridNodeBase = nodes[j];
			if (!customGridNodeBase.Walkable)
			{
				continue;
			}
			Color color = helper.NodeColor(customGridNodeBase);
			if ((isCustomColoring && !TryGetCustomColor(customGridNodeBase, out color)) || color.a <= 0.001f)
			{
				continue;
			}
			for (int k = 0; k < array.Length; k++)
			{
				int num6 = array[k];
				int num7 = array[(k + 1) % array.Length];
				CustomGridNodeBase customGridNodeBase2 = null;
				CustomGridNodeBase neighbourAlongDirection = customGridNodeBase.GetNeighbourAlongDirection(num6);
				if (neighbourAlongDirection != null && neighbours != NumNeighbours.Six)
				{
					customGridNodeBase2 = neighbourAlongDirection.GetNeighbourAlongDirection(num7);
				}
				CustomGridNodeBase neighbourAlongDirection2 = customGridNodeBase.GetNeighbourAlongDirection(num7);
				if (neighbourAlongDirection2 != null && customGridNodeBase2 == null && neighbours != NumNeighbours.Six)
				{
					customGridNodeBase2 = neighbourAlongDirection2.GetNeighbourAlongDirection(num6);
				}
				Vector3 point = new Vector3((float)customGridNodeBase.XCoordinateInGrid + 0.5f, 0f, (float)customGridNodeBase.ZCoordinateInGrid + 0.5f);
				point.x += (float)(neighbourXOffsets[num6] + neighbourXOffsets[num7]) * num2;
				point.z += (float)(neighbourZOffsets[num6] + neighbourZOffsets[num7]) * num2;
				point.y += transform.InverseTransform((Vector3)customGridNodeBase.position).y;
				if (neighbourAlongDirection != null)
				{
					point.y += transform.InverseTransform((Vector3)neighbourAlongDirection.position).y;
				}
				if (neighbourAlongDirection2 != null)
				{
					point.y += transform.InverseTransform((Vector3)neighbourAlongDirection2.position).y;
				}
				if (customGridNodeBase2 != null)
				{
					point.y += transform.InverseTransform((Vector3)customGridNodeBase2.position).y;
				}
				point.y /= 1f + ((neighbourAlongDirection != null) ? 1f : 0f) + ((neighbourAlongDirection2 != null) ? 1f : 0f) + ((customGridNodeBase2 != null) ? 1f : 0f);
				point = transform.Transform(point);
				array2[num5 + k] = point;
			}
			if (neighbours == NumNeighbours.Six)
			{
				array2[num5 + 6] = array2[num5];
				array2[num5 + 7] = array2[num5 + 2];
				array2[num5 + 8] = array2[num5 + 3];
				array2[num5 + 9] = array2[num5];
				array2[num5 + 10] = array2[num5 + 3];
				array2[num5 + 11] = array2[num5 + 5];
			}
			else
			{
				array2[num5 + 4] = array2[num5];
				array2[num5 + 5] = array2[num5 + 2];
			}
			for (int l = 0; l < num4; l++)
			{
				array3[num5 + l] = color;
			}
			for (int m = 0; m < array.Length; m++)
			{
				CustomGridNodeBase neighbourAlongDirection3 = customGridNodeBase.GetNeighbourAlongDirection(array[(m + 1) % array.Length]);
				if (neighbourAlongDirection3 == null || (showMeshOutline && customGridNodeBase.NodeInGridIndex < neighbourAlongDirection3.NodeInGridIndex))
				{
					helper.builder.DrawLine(array2[num5 + m], array2[num5 + (m + 1) % array.Length], (neighbourAlongDirection3 == null) ? Color.black : color);
				}
			}
			num5 += num4;
		}
		if (showMeshSurface || isCustomColoring)
		{
			helper.DrawTriangles(array2, array3, num5 * num3 / num4);
		}
		ArrayPool<Vector3>.Release(ref array2);
		ArrayPool<Color>.Release(ref array3);
	}

	private void CreateMeshNodesVisualization(GraphGizmoHelper helper)
	{
		if (nodes == null || meshNodes == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].Walkable)
			{
				num++;
			}
		}
		int[] obj = ((neighbours == NumNeighbours.Six) ? hexagonNeighbourIndices : new int[4] { 0, 1, 2, 3 });
		float num2 = ((neighbours == NumNeighbours.Six) ? 0.333333f : 0.5f);
		int num3 = obj.Length - 2;
		int num4 = 3 * num3;
		Vector3[] array = ArrayPool<Vector3>.Claim(num * num4);
		Color[] array2 = ArrayPool<Color>.Claim(num * num4);
		int num5 = 0;
		for (int j = 0; j < nodes.Length; j++)
		{
			CustomGridNode customGridNode = nodes[j];
			if (customGridNode.Walkable)
			{
				Color color = new Color(1f, 0.2f, 0.2f, 0.3f);
				ref CustomGridMeshNode reference = ref meshNodes[j];
				Vector3 vector = new Vector3((float)customGridNode.XCoordinateInGrid + 0.5f, 0f, (float)customGridNode.ZCoordinateInGrid + 0.5f);
				vector.y += transform.InverseTransform((Vector3)customGridNode.position).y;
				reference.Unpack(vector.y, out var cornerHeightSW, out var cornerHeightSE, out var cornerHeightNW, out var cornerHeightNE);
				float num6 = num2;
				Vector3 point = new Vector3(vector.x - num6, cornerHeightSW, vector.z - num6);
				Vector3 point2 = new Vector3(vector.x + num6, cornerHeightSE, vector.z - num6);
				Vector3 point3 = new Vector3(vector.x - num6, cornerHeightNW, vector.z + num6);
				Vector3 point4 = new Vector3(vector.x + num6, cornerHeightNE, vector.z + num6);
				point2 = transform.Transform(point2);
				point = transform.Transform(point);
				point3 = transform.Transform(point3);
				point4 = transform.Transform(point4);
				array[num5] = point2;
				array[num5 + 1] = point;
				array[num5 + 2] = point3;
				array[num5 + 3] = point4;
				if (neighbours == NumNeighbours.Six)
				{
					array[num5 + 6] = array[num5];
					array[num5 + 7] = array[num5 + 2];
					array[num5 + 8] = array[num5 + 3];
					array[num5 + 9] = array[num5];
					array[num5 + 10] = array[num5 + 3];
					array[num5 + 11] = array[num5 + 5];
				}
				else
				{
					array[num5 + 4] = array[num5];
					array[num5 + 5] = array[num5 + 2];
				}
				for (int k = 0; k < num4; k++)
				{
					array2[num5 + k] = color;
				}
				num5 += num4;
			}
		}
		if (showMeshSurface)
		{
			helper.DrawTriangles(array, array2, num5 * num3 / num4);
		}
		ArrayPool<Vector3>.Release(ref array);
		ArrayPool<Color>.Release(ref array2);
	}

	protected IntRect GetRectFromBounds(Bounds bounds)
	{
		bounds = transform.InverseTransform(bounds);
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		int xmin = Mathf.RoundToInt(min.x - 0.5f);
		int xmax = Mathf.RoundToInt(max.x - 0.5f);
		int ymin = Mathf.RoundToInt(min.z - 0.5f);
		int ymax = Mathf.RoundToInt(max.z - 0.5f);
		IntRect a = new IntRect(xmin, ymin, xmax, ymax);
		IntRect b = new IntRect(0, 0, width - 1, depth - 1);
		return IntRect.Intersection(a, b);
	}

	[Obsolete("This method has been renamed to GetNodesInRegion", true)]
	public List<GraphNode> GetNodesInArea(Bounds bounds)
	{
		return GetNodesInRegion(bounds);
	}

	[Obsolete("This method has been renamed to GetNodesInRegion", true)]
	public List<GraphNode> GetNodesInArea(GraphUpdateShape shape)
	{
		return GetNodesInRegion(shape);
	}

	[Obsolete("This method has been renamed to GetNodesInRegion", true)]
	public List<GraphNode> GetNodesInArea(Bounds bounds, GraphUpdateShape shape)
	{
		return GetNodesInRegion(bounds, shape);
	}

	public List<GraphNode> GetNodesInRegion(Bounds bounds)
	{
		return GetNodesInRegion(bounds, null);
	}

	public List<GraphNode> GetNodesInRegion(GraphUpdateShape shape)
	{
		return GetNodesInRegion(shape.GetBounds(), shape);
	}

	protected virtual List<GraphNode> GetNodesInRegion(Bounds bounds, GraphUpdateShape shape)
	{
		IntRect rectFromBounds = GetRectFromBounds(bounds);
		if (nodes == null || !rectFromBounds.IsValid() || nodes.Length != width * depth)
		{
			return global::Pathfinding.Util.ListPool<GraphNode>.Claim();
		}
		List<GraphNode> list = global::Pathfinding.Util.ListPool<GraphNode>.Claim(rectFromBounds.Width * rectFromBounds.Height);
		for (int i = rectFromBounds.xmin; i <= rectFromBounds.xmax; i++)
		{
			for (int j = rectFromBounds.ymin; j <= rectFromBounds.ymax; j++)
			{
				int num = j * width + i;
				GraphNode graphNode = nodes[num];
				if (bounds.Contains((Vector3)graphNode.position) && (shape == null || shape.Contains((Vector3)graphNode.position)))
				{
					list.Add(graphNode);
				}
			}
		}
		return list;
	}

	public virtual List<GraphNode> GetNodesInRegion(IntRect rect)
	{
		rect = IntRect.Intersection(b: new IntRect(0, 0, width - 1, depth - 1), a: rect);
		if (nodes == null || !rect.IsValid() || nodes.Length != width * depth)
		{
			return global::Pathfinding.Util.ListPool<GraphNode>.Claim(0);
		}
		List<GraphNode> list = global::Pathfinding.Util.ListPool<GraphNode>.Claim(rect.Width * rect.Height);
		for (int i = rect.ymin; i <= rect.ymax; i++)
		{
			int num = i * Width;
			for (int j = rect.xmin; j <= rect.xmax; j++)
			{
				list.Add(nodes[num + j]);
			}
		}
		return list;
	}

	public virtual int GetNodesInRegion(IntRect rect, CustomGridNodeBase[] buffer)
	{
		rect = IntRect.Intersection(b: new IntRect(0, 0, width - 1, depth - 1), a: rect);
		if (nodes == null || !rect.IsValid() || nodes.Length != width * depth)
		{
			return 0;
		}
		if (buffer.Length < rect.Width * rect.Height)
		{
			throw new ArgumentException("Buffer is too small");
		}
		int num = 0;
		int num2 = rect.ymin;
		while (num2 <= rect.ymax)
		{
			Array.Copy(nodes, num2 * Width + rect.xmin, buffer, num, rect.Width);
			num2++;
			num += rect.Width;
		}
		return num;
	}

	[CanBeNull]
	public virtual CustomGridNodeBase GetNode(int x, int z)
	{
		if (x < 0 || z < 0 || x >= width || z >= depth)
		{
			return null;
		}
		return nodes[x + z * width];
	}

	GraphUpdateThreading IUpdatableGraph.CanUpdateAsync(GraphUpdateObject o)
	{
		return (GraphUpdateThreading)6;
	}

	void IUpdatableGraph.UpdateAreaInit(GraphUpdateObject o)
	{
		if (MasksChanged)
		{
			MasksChanged = false;
			m_NavmeshMasks = AstarData.active.GetComponent<NavmeshMasks>()?.ConvertData() ?? default(NavmeshMasksGeneration);
		}
		m_GridCuts = GridCuts.Create(transform);
		m_Fences = GridNavmeshFences.Create(transform);
	}

	void IUpdatableGraph.UpdateAreaPost(GraphUpdateObject o)
	{
	}

	protected void CalculateAffectedRegions(GraphUpdateObject o, out IntRect originalRect, out IntRect affectRect, out IntRect physicsRect, out bool willChangeWalkability, out int erosion)
	{
		Bounds bounds = transform.InverseTransform(o.bounds);
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		int xmin = Mathf.RoundToInt(min.x - 0.5f);
		int xmax = Mathf.RoundToInt(max.x - 0.5f);
		int ymin = Mathf.RoundToInt(min.z - 0.5f);
		int ymax = Mathf.RoundToInt(max.z - 0.5f);
		originalRect = new IntRect(xmin, ymin, xmax, ymax);
		affectRect = originalRect;
		physicsRect = originalRect;
		erosion = (o.updateErosion ? erodeIterations : 0);
		willChangeWalkability = o.updatePhysics || o.modifyWalkability || o.modifyDynamicWalkability;
		if (o.updatePhysics && !o.modifyWalkability && collision.collisionCheck)
		{
			Vector3 vector = new Vector3(collision.diameter, 0f, collision.diameter) * 0.5f;
			min -= vector * 1.02f;
			max += vector * 1.02f;
			physicsRect = new IntRect(Mathf.RoundToInt(min.x - 0.5f), Mathf.RoundToInt(min.z - 0.5f), Mathf.RoundToInt(max.x - 0.5f), Mathf.RoundToInt(max.z - 0.5f));
			affectRect = IntRect.Union(physicsRect, affectRect);
		}
		if (willChangeWalkability || erosion > 0)
		{
			affectRect = affectRect.Expand(erosion + 1);
		}
	}

	void IUpdatableGraph.UpdateArea(GraphUpdateObject o)
	{
		if (nodes == null || nodes.Length != width * depth)
		{
			Debug.LogWarning("The Grid Graph is not scanned, cannot update area");
			return;
		}
		CalculateAffectedRegions(o, out var originalRect, out var affectRect, out var physicsRect, out var willChangeWalkability, out var erosion);
		IntRect b = new IntRect(0, 0, width - 1, depth - 1);
		IntRect intRect = IntRect.Intersection(affectRect, b);
		for (int i = intRect.ymin; i <= intRect.ymax; i++)
		{
			for (int j = intRect.xmin; j <= intRect.xmax; j++)
			{
				o.WillUpdateNode(nodes[i * width + j]);
			}
		}
		if (o.updatePhysics && !o.modifyWalkability)
		{
			collision.Initialize(transform, nodeSize);
			intRect = IntRect.Intersection(physicsRect, b);
			for (int k = intRect.ymin; k <= intRect.ymax; k++)
			{
				for (int l = intRect.xmin; l <= intRect.xmax; l++)
				{
					RecalculateCell(l, k, o.resetPenaltyOnPhysics, resetTags: false);
				}
			}
		}
		intRect = IntRect.Intersection(originalRect, b);
		for (int m = intRect.ymin; m <= intRect.ymax; m++)
		{
			for (int n = intRect.xmin; n <= intRect.xmax; n++)
			{
				int num = m * width + n;
				CustomGridNode customGridNode = nodes[num];
				if (!o.bounds.Contains((Vector3)customGridNode.position))
				{
					continue;
				}
				if (willChangeWalkability)
				{
					Bounds bounds = o.bounds;
					bounds.min = new Vector3(bounds.min.x, 0f, bounds.min.z);
					bounds.max = new Vector3(bounds.max.x, 0f, bounds.max.z);
					bounds.extents = new Vector3(bounds.extents.x, 1f, bounds.extents.z);
					Vector3 point = (Vector3)customGridNode.position;
					point.y = 0f;
					if (bounds.Contains(point))
					{
						customGridNode.StaticWalkable = customGridNode.WalkableErosion;
						o.Apply(customGridNode);
						customGridNode.WalkableErosion = customGridNode.StaticWalkable;
					}
					customGridNode.WalkableErosion = customGridNode.StaticWalkable;
				}
				else
				{
					o.Apply(customGridNode);
				}
			}
		}
		if (willChangeWalkability && erosion == 0)
		{
			intRect = IntRect.Intersection(affectRect, b);
			for (int num2 = intRect.xmin; num2 <= intRect.xmax; num2++)
			{
				for (int num3 = intRect.ymin; num3 <= intRect.ymax; num3++)
				{
					CalculateConnections(num2, num3);
				}
			}
		}
		else if (willChangeWalkability && erosion > 0)
		{
			IntRect a = IntRect.Union(originalRect, physicsRect).Expand(erosion);
			IntRect a2 = a.Expand(erosion);
			a = IntRect.Intersection(a, b);
			a2 = IntRect.Intersection(a2, b);
			for (int num4 = a2.xmin; num4 <= a2.xmax; num4++)
			{
				for (int num5 = a2.ymin; num5 <= a2.ymax; num5++)
				{
					int num6 = num5 * width + num4;
					CustomGridNode customGridNode2 = nodes[num6];
					bool staticWalkable = customGridNode2.StaticWalkable;
					customGridNode2.StaticWalkable = customGridNode2.WalkableErosion;
					if (!a.Contains(num4, num5))
					{
						customGridNode2.TmpWalkable = staticWalkable;
					}
				}
			}
			for (int num7 = a2.xmin; num7 <= a2.xmax; num7++)
			{
				for (int num8 = a2.ymin; num8 <= a2.ymax; num8++)
				{
					CalculateConnections(num7, num8);
				}
			}
			ErodeWalkableArea(a2.xmin, a2.ymin, a2.xmax + 1, a2.ymax + 1);
			for (int num9 = a2.xmin; num9 <= a2.xmax; num9++)
			{
				for (int num10 = a2.ymin; num10 <= a2.ymax; num10++)
				{
					if (!a.Contains(num9, num10))
					{
						int num11 = num10 * width + num9;
						CustomGridNode obj = nodes[num11];
						obj.StaticWalkable = obj.TmpWalkable;
					}
				}
			}
			for (int num12 = a2.xmin; num12 <= a2.xmax; num12++)
			{
				for (int num13 = a2.ymin; num13 <= a2.ymax; num13++)
				{
					CalculateConnections(num12, num13);
				}
			}
		}
		o.updating = false;
	}

	public bool Linecast(Vector3 from, Vector3 to)
	{
		GraphHitInfo hit;
		return Linecast(from, to, null, out hit);
	}

	public bool Linecast(Vector3 from, Vector3 to, GraphNode hint)
	{
		GraphHitInfo hit;
		return Linecast(from, to, hint, out hit);
	}

	public bool Linecast(Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit)
	{
		return Linecast(from, to, hint, out hit, null);
	}

	protected static float CrossMagnitude(Vector2 a, Vector2 b)
	{
		return a.x * b.y - b.x * a.y;
	}

	protected static long CrossMagnitude(Int2 a, Int2 b)
	{
		return (long)a.x * (long)b.y - (long)b.x * (long)a.y;
	}

	protected bool ClipLineSegmentToBounds(Vector3 a, Vector3 b, out Vector3 outA, out Vector3 outB)
	{
		if (a.x < 0f || a.z < 0f || a.x > (float)width || a.z > (float)depth || b.x < 0f || b.z < 0f || b.x > (float)width || b.z > (float)depth)
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			Vector3 vector2 = new Vector3(0f, 0f, depth);
			Vector3 vector3 = new Vector3(width, 0f, depth);
			Vector3 vector4 = new Vector3(width, 0f, 0f);
			int num = 0;
			Vector3 vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector, vector2, out var intersects);
			if (intersects)
			{
				num++;
				if (!VectorMath.RightOrColinearXZ(vector, vector2, a))
				{
					a = vector5;
				}
				else
				{
					b = vector5;
				}
			}
			vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector2, vector3, out intersects);
			if (intersects)
			{
				num++;
				if (!VectorMath.RightOrColinearXZ(vector2, vector3, a))
				{
					a = vector5;
				}
				else
				{
					b = vector5;
				}
			}
			vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector3, vector4, out intersects);
			if (intersects)
			{
				num++;
				if (!VectorMath.RightOrColinearXZ(vector3, vector4, a))
				{
					a = vector5;
				}
				else
				{
					b = vector5;
				}
			}
			vector5 = VectorMath.SegmentIntersectionPointXZ(a, b, vector4, vector, out intersects);
			if (intersects)
			{
				num++;
				if (!VectorMath.RightOrColinearXZ(vector4, vector, a))
				{
					a = vector5;
				}
				else
				{
					b = vector5;
				}
			}
			if (num == 0)
			{
				outA = Vector3.zero;
				outB = Vector3.zero;
				return false;
			}
		}
		outA = a;
		outB = b;
		return true;
	}

	public bool Linecast(Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
	{
		hit = default(GraphHitInfo);
		hit.origin = from;
		Vector3 outA = transform.InverseTransform(from);
		Vector3 outB = transform.InverseTransform(to);
		if (!ClipLineSegmentToBounds(outA, outB, out outA, out outB))
		{
			hit.point = to;
			return false;
		}
		CustomGridNodeBase customGridNodeBase = GetNearest(transform.Transform(outA), NNConstraint.None).node as CustomGridNodeBase;
		CustomGridNodeBase customGridNodeBase2 = GetNearest(transform.Transform(outB), NNConstraint.None).node as CustomGridNodeBase;
		if (!customGridNodeBase.Walkable)
		{
			hit.node = customGridNodeBase;
			hit.point = transform.Transform(outA);
			hit.tangentOrigin = hit.point;
			return true;
		}
		Vector2 vector = new Vector2(outA.x - 0.5f, outA.z - 0.5f);
		Vector2 vector2 = new Vector2(outB.x - 0.5f, outB.z - 0.5f);
		if (customGridNodeBase == null || customGridNodeBase2 == null)
		{
			hit.node = null;
			hit.point = from;
			return true;
		}
		Vector2 a = vector2 - vector;
		float num = CrossMagnitude(b: new Vector2(Mathf.Sign(a.x), Mathf.Sign(a.y)), a: a) * 0.5f;
		int num2 = ((!(a.y >= 0f)) ? 3 : 0) ^ ((!(a.x >= 0f)) ? 1 : 0);
		int num3 = (num2 + 1) & 3;
		int num4 = (num2 + 2) & 3;
		CustomGridNodeBase customGridNodeBase3 = customGridNodeBase;
		while (customGridNodeBase3.NodeInGridIndex != customGridNodeBase2.NodeInGridIndex)
		{
			trace?.Add(customGridNodeBase3);
			Vector2 vector3 = new Vector2(customGridNodeBase3.XCoordinateInGrid, customGridNodeBase3.ZCoordinateInGrid);
			int num5 = ((CrossMagnitude(a, vector3 - vector) + num < 0f) ? num4 : num3);
			CustomGridNodeBase neighbourAlongDirection = customGridNodeBase3.GetNeighbourAlongDirection(num5);
			if (neighbourAlongDirection != null)
			{
				customGridNodeBase3 = neighbourAlongDirection;
				continue;
			}
			Vector2 vector4 = new Vector2(neighbourXOffsets[num5], neighbourZOffsets[num5]);
			Vector2 vector5 = new Vector2(neighbourXOffsets[(num5 - 1 + 4) & 3], neighbourZOffsets[(num5 - 1 + 4) & 3]);
			Vector2 vector6 = new Vector2(neighbourXOffsets[(num5 + 1) & 3], neighbourZOffsets[(num5 + 1) & 3]);
			Vector2 vector7 = vector3 + (vector4 + vector5) * 0.5f;
			Vector2 vector8 = VectorMath.LineIntersectionPoint(vector7, vector7 + vector6, vector, vector2);
			Vector3 vector9 = transform.InverseTransform((Vector3)customGridNodeBase3.position);
			Vector3 point = new Vector3(vector8.x + 0.5f, vector9.y, vector8.y + 0.5f);
			Vector3 point2 = new Vector3(vector7.x + 0.5f, vector9.y, vector7.y + 0.5f);
			hit.point = transform.Transform(point);
			hit.tangentOrigin = transform.Transform(point2);
			hit.tangent = transform.TransformVector(new Vector3(vector6.x, 0f, vector6.y));
			hit.node = customGridNodeBase3;
			return true;
		}
		trace?.Add(customGridNodeBase3);
		if (customGridNodeBase3 == customGridNodeBase2)
		{
			hit.point = to;
			hit.node = customGridNodeBase3;
			return false;
		}
		hit.point = (Vector3)customGridNodeBase3.position;
		hit.tangentOrigin = hit.point;
		return true;
	}

	public bool SnappedLinecast(Vector3 from, Vector3 to, GraphNode hint, out GraphHitInfo hit)
	{
		return Linecast((Vector3)GetNearest(from, NNConstraint.None).node.position, (Vector3)GetNearest(to, NNConstraint.None).node.position, hint, out hit);
	}

	public bool Linecast(CustomGridNodeBase fromNode, CustomGridNodeBase toNode)
	{
		Int2 a = new Int2(toNode.XCoordinateInGrid - fromNode.XCoordinateInGrid, toNode.ZCoordinateInGrid - fromNode.ZCoordinateInGrid);
		long num = CrossMagnitude(a, new Int2(Math.Sign(a.x), Math.Sign(a.y)));
		int num2 = 0;
		if (a.x <= 0 && a.y > 0)
		{
			num2 = 1;
		}
		else if (a.x < 0 && a.y <= 0)
		{
			num2 = 2;
		}
		else if (a.x >= 0 && a.y < 0)
		{
			num2 = 3;
		}
		int num3 = (num2 + 1) & 3;
		int num4 = (num2 + 2) & 3;
		int num5 = ((a.x != 0 && a.y != 0) ? (4 + ((num2 + 1) & 3)) : (-1));
		Int2 b = new Int2(0, 0);
		while (fromNode != null && fromNode.NodeInGridIndex != toNode.NodeInGridIndex)
		{
			long num6 = CrossMagnitude(a, b) * 2 + num;
			int num7 = ((num6 < 0) ? num4 : num3);
			if (num6 == 0L && num5 != -1)
			{
				num7 = num5;
			}
			fromNode = fromNode.GetNeighbourAlongDirection(num7);
			b += new Int2(neighbourXOffsets[num7], neighbourZOffsets[num7]);
		}
		return fromNode != toNode;
	}

	public bool CheckConnection(CustomGridNode node, int dir)
	{
		if (neighbours == NumNeighbours.Eight || neighbours == NumNeighbours.Six || dir < 4)
		{
			return HasNodeConnection(node, dir);
		}
		int num = (dir - 4 - 1) & 3;
		int num2 = (dir - 4 + 1) & 3;
		if (!HasNodeConnection(node, num) || !HasNodeConnection(node, num2))
		{
			return false;
		}
		CustomGridNode customGridNode = nodes[node.NodeInGridIndex + neighbourOffsets[num]];
		CustomGridNode customGridNode2 = nodes[node.NodeInGridIndex + neighbourOffsets[num2]];
		if (!customGridNode.StaticWalkable || !customGridNode2.StaticWalkable)
		{
			return false;
		}
		if (!HasNodeConnection(customGridNode2, num) || !HasNodeConnection(customGridNode, num2))
		{
			return false;
		}
		return true;
	}

	protected override void SerializeExtraInfo(GraphSerializationContext ctx)
	{
		ctx.writer.Write(kDataFormatSignature);
		ctx.writer.Write(kDataFormatVersion);
		if (nodes != null)
		{
			ctx.writer.Write(nodes.Length);
			CustomGridNode[] array = nodes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SerializeNode(ctx);
			}
			CustomGridMeshNode[] array2 = meshNodes;
			foreach (CustomGridMeshNode customGridMeshNode in array2)
			{
				customGridMeshNode.Write(ctx.writer);
			}
		}
		else
		{
			ctx.writer.Write(kInvalidNodesCount);
		}
	}

	protected override void DeserializeExtraInfo(GraphSerializationContext ctx)
	{
		int num = ctx.reader.ReadInt32();
		uint num2;
		int num3;
		if (num == kDataFormatSignature)
		{
			num2 = ctx.reader.ReadUInt32();
			num3 = ctx.reader.ReadInt32();
		}
		else
		{
			num2 = kDataFormatVersionLegacy;
			num3 = num;
		}
		if (num3 >= 0)
		{
			nodes = new CustomGridNode[num3];
			for (int i = 0; i < num3; i++)
			{
				CustomGridNode customGridNode = new CustomGridNode(active);
				customGridNode.DeserializeNode(ctx);
				nodes[i] = customGridNode;
			}
			if (num2 >= kBakedSurfaceDataMinVersion)
			{
				meshNodes = new CustomGridMeshNode[num3];
				for (int j = 0; j < num3; j++)
				{
					meshNodes[j].Read(ctx.reader);
				}
			}
			else
			{
				meshNodes = null;
			}
		}
		else
		{
			nodes = null;
			meshNodes = null;
		}
	}

	protected override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
	{
		base.DeserializeSettingsCompatibility(ctx);
		aspectRatio = ctx.reader.ReadSingle();
		rotation = ctx.DeserializeVector3();
		center = ctx.DeserializeVector3();
		unclampedSize = ctx.DeserializeVector3();
		nodeSize = ctx.reader.ReadSingle();
		collision.DeserializeSettingsCompatibility(ctx);
		maxClimb = ctx.reader.ReadSingle();
		ctx.reader.ReadInt32();
		maxSlope = ctx.reader.ReadSingle();
		erodeIterations = ctx.reader.ReadInt32();
		erosionUseTags = ctx.reader.ReadBoolean();
		erosionFirstTag = ctx.reader.ReadInt32();
		ctx.reader.ReadBoolean();
		neighbours = (NumNeighbours)ctx.reader.ReadInt32();
		cutCorners = ctx.reader.ReadBoolean();
		penaltyPosition = ctx.reader.ReadBoolean();
		penaltyPositionFactor = ctx.reader.ReadSingle();
		penaltyAngle = ctx.reader.ReadBoolean();
		penaltyAngleFactor = ctx.reader.ReadSingle();
		penaltyAnglePower = ctx.reader.ReadSingle();
		isometricAngle = ctx.reader.ReadSingle();
		uniformEdgeCosts = ctx.reader.ReadBoolean();
		useJumpPointSearch = ctx.reader.ReadBoolean();
	}

	protected override void PostDeserialization(GraphSerializationContext ctx)
	{
		UpdateTransform();
		SetUpOffsetsAndCosts();
		CustomGridNode.SetGridGraph((int)graphIndex, this);
		if (nodes == null || nodes.Length == 0)
		{
			return;
		}
		if (width * depth != nodes.Length)
		{
			Debug.LogError("Node data did not match with bounds data. Probably a change to the bounds/width/depth data was made after scanning the graph just prior to saving it. Nodes will be discarded");
			nodes = new CustomGridNode[0];
			return;
		}
		for (int i = 0; i < depth; i++)
		{
			for (int j = 0; j < width; j++)
			{
				CustomGridNode customGridNode = nodes[i * width + j];
				if (customGridNode == null)
				{
					Debug.LogError("Deserialization Error : Couldn't cast the node to the appropriate type - GridGenerator");
					return;
				}
				customGridNode.NodeInGridIndex = i * width + j;
			}
		}
	}

	public override void ApplyValidAreas(HashSet<uint> validAreas)
	{
		CustomGridNode[] array = nodes;
		foreach (CustomGridNode customGridNode in array)
		{
			if (!validAreas.Contains(customGridNode.Area))
			{
				customGridNode.StaticWalkable = false;
			}
		}
	}

	public void SetCustomColor(CustomGridNodeBase node, Color color)
	{
		customColoring[node.NodeInGridIndex] = color;
	}

	public bool TryGetCustomColor(CustomGridNodeBase node, out Color color)
	{
		return customColoring.TryGetValue(node.NodeInGridIndex, out color);
	}

	public void SubmitCustomColoring()
	{
		customColoringHash++;
	}

	public void ClearCustomColoring()
	{
		customColoring.Clear();
	}
}
