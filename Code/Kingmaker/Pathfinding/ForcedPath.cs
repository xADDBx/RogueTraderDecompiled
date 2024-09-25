using System.Collections.Generic;
using System.Linq;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[MemoryPackable(GenerateType.Object)]
public class ForcedPath : Path, IMemoryPackable<ForcedPath>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ForcedPathFormatter : MemoryPackFormatter<ForcedPath>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ForcedPath value)
		{
			ForcedPath.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ForcedPath value)
		{
			ForcedPath.Deserialize(ref reader, ref value);
		}
	}

	public static readonly ForcedPath ErrorPath;

	[MemoryPackIgnore]
	public string UserTag { get; set; }

	[JsonConstructor]
	[MemoryPackConstructor]
	public ForcedPath()
	{
	}

	public static ForcedPath Construct(List<Vector3> points)
	{
		ForcedPath forcedPath = PathPool.GetPath<ForcedPath>();
		forcedPath.vectorPath = ListPool<Vector3>.Claim();
		forcedPath.vectorPath.AddRange(points);
		forcedPath.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		forcedPath.CompleteState = PathCompleteState.Complete;
		return forcedPath;
	}

	public static ForcedPath Construct(Path sourcePath)
	{
		if (sourcePath.error)
		{
			return ErrorPath;
		}
		ForcedPath forcedPath = PathPool.GetPath<ForcedPath>();
		forcedPath.vectorPath = ListPool<Vector3>.Claim();
		forcedPath.path = ListPool<GraphNode>.Claim();
		if (sourcePath.GetTotalLength() < float.PositiveInfinity)
		{
			forcedPath.vectorPath.AddRange(sourcePath.vectorPath);
			forcedPath.path.AddRange(sourcePath.path);
		}
		forcedPath.pathRequestedTick = sourcePath.pathRequestedTick;
		forcedPath.CompleteState = PathCompleteState.Complete;
		return forcedPath;
	}

	public static ForcedPath Construct(IEnumerable<GraphNode> nodes)
	{
		ForcedPath forcedPath = PathPool.GetPath<ForcedPath>();
		forcedPath.vectorPath = ListPool<Vector3>.Claim();
		forcedPath.path = ListPool<GraphNode>.Claim();
		forcedPath.vectorPath.AddRange(nodes.Select((GraphNode i) => i.Vector3Position));
		forcedPath.path.AddRange(nodes);
		forcedPath.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		forcedPath.CompleteState = PathCompleteState.Complete;
		return forcedPath;
	}

	public static ForcedPath Construct(IEnumerable<Vector3> points, IEnumerable<GraphNode> nodes)
	{
		ForcedPath forcedPath = PathPool.GetPath<ForcedPath>();
		forcedPath.vectorPath = ListPool<Vector3>.Claim();
		forcedPath.path = ListPool<GraphNode>.Claim();
		forcedPath.vectorPath.AddRange(points);
		forcedPath.path.AddRange(nodes);
		forcedPath.pathRequestedTick = Game.Instance.Player.GameTime.Ticks;
		forcedPath.CompleteState = PathCompleteState.Complete;
		return forcedPath;
	}

	protected override void Prepare()
	{
	}

	protected override void Initialize()
	{
	}

	protected override void CalculateStep(long targetTick)
	{
	}

	protected override void OnEnterPool()
	{
		base.OnEnterPool();
		if (vectorPath != null)
		{
			ListPool<Vector3>.Release(vectorPath);
			vectorPath = null;
		}
		if (path != null)
		{
			ListPool<GraphNode>.Release(path);
			path = null;
		}
		pathRequestedTick = 0L;
	}

	protected override void Reset()
	{
		if ((object)AstarPath.active != null)
		{
			base.Reset();
			return;
		}
		hasBeenReset = true;
		pathHandler = null;
		callback = null;
		immediateCallback = null;
		completeState = PathCompleteState.NotCalculated;
		currentR = null;
		duration = 0f;
		nnConstraint.Reset();
		enabledTags = -1;
		base.tagPenalties = null;
		hTarget = Int3.zero;
		hTargetNode = null;
		traversalProvider = null;
	}

	static ForcedPath()
	{
		ErrorPath = new ForcedPath
		{
			CompleteState = PathCompleteState.Error
		};
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ForcedPath>())
		{
			MemoryPackFormatterProvider.Register(new ForcedPathFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ForcedPath[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ForcedPath>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<Vector3>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<Vector3>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PathCompleteState>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<PathCompleteState>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ForcedPath? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		writer.WriteValue(in value.vectorPath);
		ref long value2 = ref value.pathRequestedTick;
		ref bool value3 = ref value.persistentPath;
		PathCompleteState value4 = value.CompleteState;
		writer.WriteUnmanaged(in value2, in value3, in value4);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ForcedPath? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		List<Vector3> value2;
		long value3;
		bool value4;
		PathCompleteState value5;
		if (memberCount == 4)
		{
			if (value != null)
			{
				value2 = value.vectorPath;
				value3 = value.pathRequestedTick;
				value4 = value.persistentPath;
				value5 = value.CompleteState;
				reader.ReadValue(ref value2);
				reader.ReadUnmanaged<long>(out value3);
				reader.ReadUnmanaged<bool>(out value4);
				reader.ReadUnmanaged<PathCompleteState>(out value5);
				goto IL_00ef;
			}
			value2 = reader.ReadValue<List<Vector3>>();
			reader.ReadUnmanaged<long, bool, PathCompleteState>(out value3, out value4, out value5);
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ForcedPath), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0L;
				value4 = false;
				value5 = PathCompleteState.NotCalculated;
			}
			else
			{
				value2 = value.vectorPath;
				value3 = value.pathRequestedTick;
				value4 = value.persistentPath;
				value5 = value.CompleteState;
			}
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<long>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<PathCompleteState>(out value5);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00ef;
			}
		}
		value = new ForcedPath
		{
			vectorPath = value2,
			pathRequestedTick = value3,
			persistentPath = value4,
			CompleteState = value5
		};
		return;
		IL_00ef:
		value.vectorPath = value2;
		value.pathRequestedTick = value3;
		value.persistentPath = value4;
		value.CompleteState = value5;
	}
}
