using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[MemoryPackable(GenerateType.Object)]
public class WarhammerPathPlayer : WarhammerPath<WarhammerPathPlayerMetric, WarhammerPathPlayerCell>, IMemoryPackable<WarhammerPathPlayer>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class WarhammerPathPlayerFormatter : MemoryPackFormatter<WarhammerPathPlayer>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref WarhammerPathPlayer value)
		{
			WarhammerPathPlayer.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref WarhammerPathPlayer value)
		{
			WarhammerPathPlayer.Deserialize(ref reader, ref value);
		}
	}

	public static WarhammerPathPlayer Construct(Vector3 start, BlockMode blockMode, WarhammerPathPlayerMetric initialLength, ITraversalCostProvider<WarhammerPathPlayerMetric, WarhammerPathPlayerCell> traversalCostProvider, OnPathDelegate callback = null)
	{
		WarhammerPathPlayer warhammerPathPlayer = PathPool.GetPath<WarhammerPathPlayer>();
		warhammerPathPlayer.Setup(start, blockMode, initialLength, traversalCostProvider, callback);
		return warhammerPathPlayer;
	}

	static WarhammerPathPlayer()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<WarhammerPathPlayer>())
		{
			MemoryPackFormatterProvider.Register(new WarhammerPathPlayerFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<WarhammerPathPlayer[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<WarhammerPathPlayer>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<Vector3>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<Vector3>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PathCompleteState>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<PathCompleteState>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlockMode>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<BlockMode>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref WarhammerPathPlayer? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(5);
		writer.WriteValue(in value.vectorPath);
		ref long value2 = ref value.pathRequestedTick;
		ref bool value3 = ref value.persistentPath;
		PathCompleteState value4 = value.CompleteState;
		BlockMode value5 = value.PathBlockMode;
		writer.WriteUnmanaged(in value2, in value3, in value4, in value5);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref WarhammerPathPlayer? value)
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
		if (memberCount == 5)
		{
			BlockMode value6;
			if (value != null)
			{
				value2 = value.vectorPath;
				value3 = value.pathRequestedTick;
				value4 = value.persistentPath;
				value5 = value.CompleteState;
				value6 = value.PathBlockMode;
				reader.ReadValue(ref value2);
				reader.ReadUnmanaged<long>(out value3);
				reader.ReadUnmanaged<bool>(out value4);
				reader.ReadUnmanaged<PathCompleteState>(out value5);
				reader.ReadUnmanaged<BlockMode>(out value6);
				goto IL_011d;
			}
			value2 = reader.ReadValue<List<Vector3>>();
			reader.ReadUnmanaged<long, bool, PathCompleteState, BlockMode>(out value3, out value4, out value5, out value6);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(WarhammerPathPlayer), 5, memberCount);
				return;
			}
			BlockMode value6;
			if (value == null)
			{
				value2 = null;
				value3 = 0L;
				value4 = false;
				value5 = PathCompleteState.NotCalculated;
				value6 = BlockMode.AllExceptSelector;
			}
			else
			{
				value2 = value.vectorPath;
				value3 = value.pathRequestedTick;
				value4 = value.persistentPath;
				value5 = value.CompleteState;
				value6 = value.PathBlockMode;
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
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<BlockMode>(out value6);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_011d;
			}
		}
		value = new WarhammerPathPlayer
		{
			vectorPath = value2,
			pathRequestedTick = value3,
			persistentPath = value4,
			CompleteState = value5
		};
		return;
		IL_011d:
		value.vectorPath = value2;
		value.pathRequestedTick = value3;
		value.persistentPath = value4;
		value.CompleteState = value5;
	}
}
