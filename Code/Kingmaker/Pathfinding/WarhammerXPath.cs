using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[MemoryPackable(GenerateType.Object)]
public class WarhammerXPath : XPath, ILinkTraversePath, IMemoryPackable<WarhammerXPath>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class WarhammerXPathFormatter : MemoryPackFormatter<WarhammerXPath>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref WarhammerXPath value)
		{
			WarhammerXPath.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref WarhammerXPath value)
		{
			WarhammerXPath.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackIgnore]
	public ILinkTraversalProvider LinkTraversalProvider { get; set; }

	public new static WarhammerXPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
	{
		WarhammerXPath warhammerXPath = PathPool.GetPath<WarhammerXPath>();
		warhammerXPath.Setup(start, end, callback);
		return warhammerXPath;
	}

	protected override void OnEnterPool()
	{
		base.OnEnterPool();
		LinkTraversalProvider = null;
	}

	static WarhammerXPath()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<WarhammerXPath>())
		{
			MemoryPackFormatterProvider.Register(new WarhammerXPathFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<WarhammerXPath[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<WarhammerXPath>());
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
	public static void Serialize(ref MemoryPackWriter writer, ref WarhammerXPath? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref WarhammerXPath? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(WarhammerXPath), 4, memberCount);
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
		value = new WarhammerXPath
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
