using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Pathfinding;
using MemoryPack;
using MemoryPack.Internal;

namespace Pathfinding;

[Preserve]
internal class PathFormatter : MemoryPackFormatter<Path>
{
	private static readonly Dictionary<Type, ushort> __typeToTag = new Dictionary<Type, ushort>(6)
	{
		{
			typeof(ForcedPath),
			0
		},
		{
			typeof(ShipPath),
			1
		},
		{
			typeof(WarhammerPathPlayer),
			2
		},
		{
			typeof(WarhammerPathAi),
			3
		},
		{
			typeof(WarhammerXPath),
			4
		},
		{
			typeof(WarhammerABPath),
			5
		}
	};

	[Preserve]
	public override void Serialize(ref MemoryPackWriter writer, ref Path? value)
	{
		ushort value2;
		if (value == null)
		{
			writer.WriteNullUnionHeader();
		}
		else if (__typeToTag.TryGetValue(value.GetType(), out value2))
		{
			writer.WriteUnionHeader(value2);
			switch (value2)
			{
			case 0:
				writer.WritePackable(in Unsafe.As<Path, ForcedPath>(ref value));
				break;
			case 1:
				writer.WritePackable(in Unsafe.As<Path, ShipPath>(ref value));
				break;
			case 2:
				writer.WritePackable(in Unsafe.As<Path, WarhammerPathPlayer>(ref value));
				break;
			case 3:
				writer.WritePackable(in Unsafe.As<Path, WarhammerPathAi>(ref value));
				break;
			case 4:
				writer.WritePackable(in Unsafe.As<Path, WarhammerXPath>(ref value));
				break;
			case 5:
				writer.WritePackable(in Unsafe.As<Path, WarhammerABPath>(ref value));
				break;
			}
		}
		else
		{
			MemoryPackSerializationException.ThrowNotFoundInUnionType(value.GetType(), typeof(Path));
		}
	}

	[Preserve]
	public override void Deserialize(ref MemoryPackReader reader, ref Path? value)
	{
		if (!reader.TryReadUnionHeader(out var tag))
		{
			value = null;
			return;
		}
		switch (tag)
		{
		case 0:
			if (value is ForcedPath)
			{
				reader.ReadPackable(ref Unsafe.As<Path, ForcedPath>(ref value));
			}
			else
			{
				value = reader.ReadPackable<ForcedPath>();
			}
			break;
		case 1:
			if (value is ShipPath)
			{
				reader.ReadPackable(ref Unsafe.As<Path, ShipPath>(ref value));
			}
			else
			{
				value = reader.ReadPackable<ShipPath>();
			}
			break;
		case 2:
			if (value is WarhammerPathPlayer)
			{
				reader.ReadPackable(ref Unsafe.As<Path, WarhammerPathPlayer>(ref value));
			}
			else
			{
				value = reader.ReadPackable<WarhammerPathPlayer>();
			}
			break;
		case 3:
			if (value is WarhammerPathAi)
			{
				reader.ReadPackable(ref Unsafe.As<Path, WarhammerPathAi>(ref value));
			}
			else
			{
				value = reader.ReadPackable<WarhammerPathAi>();
			}
			break;
		case 4:
			if (value is WarhammerXPath)
			{
				reader.ReadPackable(ref Unsafe.As<Path, WarhammerXPath>(ref value));
			}
			else
			{
				value = reader.ReadPackable<WarhammerXPath>();
			}
			break;
		case 5:
			if (value is WarhammerABPath)
			{
				reader.ReadPackable(ref Unsafe.As<Path, WarhammerABPath>(ref value));
			}
			else
			{
				value = reader.ReadPackable<WarhammerABPath>();
			}
			break;
		default:
			MemoryPackSerializationException.ThrowInvalidTag(tag, typeof(Path));
			break;
		}
	}
}
