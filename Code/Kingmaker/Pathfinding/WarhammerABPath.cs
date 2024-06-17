using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[MemoryPackable(GenerateType.Object)]
public class WarhammerABPath : ABPath, ILinkTraversePath, IMemoryPackable<WarhammerABPath>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class WarhammerABPathFormatter : MemoryPackFormatter<WarhammerABPath>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref WarhammerABPath value)
		{
			WarhammerABPath.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref WarhammerABPath value)
		{
			WarhammerABPath.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackIgnore]
	public ILinkTraversalProvider LinkTraversalProvider { get; set; }

	public new static WarhammerABPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
	{
		WarhammerABPath warhammerABPath = PathPool.GetPath<WarhammerABPath>();
		warhammerABPath.Setup(start, end, callback);
		return warhammerABPath;
	}

	protected override void OnEnterPool()
	{
		base.OnEnterPool();
		LinkTraversalProvider = null;
	}

	static WarhammerABPath()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<WarhammerABPath>())
		{
			MemoryPackFormatterProvider.Register(new WarhammerABPathFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<WarhammerABPath[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<WarhammerABPath>());
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
	public static void Serialize(ref MemoryPackWriter writer, ref WarhammerABPath? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref WarhammerABPath? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(WarhammerABPath), 4, memberCount);
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
		value = new WarhammerABPath
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
