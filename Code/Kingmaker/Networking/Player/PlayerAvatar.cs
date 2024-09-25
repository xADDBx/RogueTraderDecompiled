using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Player;

[MemoryPackable(GenerateType.Object)]
public readonly struct PlayerAvatar : IMemoryPackable<PlayerAvatar>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PlayerAvatarFormatter : MemoryPackFormatter<PlayerAvatar>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PlayerAvatar value)
		{
			PlayerAvatar.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PlayerAvatar value)
		{
			PlayerAvatar.Deserialize(ref reader, ref value);
		}
	}

	public static readonly PlayerAvatar Invalid;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly ushort Width;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly byte[] Data;

	[JsonProperty]
	[MemoryPackInclude]
	public readonly bool IsCompressed;

	[MemoryPackIgnore]
	public bool IsValid
	{
		get
		{
			if (0 < Width && Data != null)
			{
				return Width < Data.Length;
			}
			return false;
		}
	}

	[JsonConstructor]
	[MemoryPackConstructor]
	private PlayerAvatar(ushort width, byte[] data, bool isCompressed)
	{
		Width = width;
		Data = data;
		IsCompressed = isCompressed;
	}

	public PlayerAvatar(int width, byte[] data, bool isCompressed = false)
		: this((ushort)width, data, isCompressed)
	{
		if (65535 < width)
		{
			throw new OverflowException($"width={width}/{ushort.MaxValue}");
		}
	}

	static PlayerAvatar()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PlayerAvatar>())
		{
			MemoryPackFormatterProvider.Register(new PlayerAvatarFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PlayerAvatar[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PlayerAvatar>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<byte[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<byte>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PlayerAvatar value)
	{
		writer.WriteUnmanagedWithObjectHeader(3, in value.Width);
		writer.WriteUnmanagedArray(value.Data);
		writer.WriteUnmanaged(in value.IsCompressed);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PlayerAvatar value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(PlayerAvatar);
			return;
		}
		ushort value2;
		byte[] value3;
		bool value4;
		if (memberCount == 3)
		{
			reader.ReadUnmanaged<ushort>(out value2);
			value3 = reader.ReadUnmanagedArray<byte>();
			reader.ReadUnmanaged<bool>(out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PlayerAvatar), 3, memberCount);
				return;
			}
			value2 = 0;
			value3 = null;
			value4 = false;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<ushort>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanagedArray(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						_ = 3;
					}
				}
			}
		}
		value = new PlayerAvatar(value2, value3, value4);
	}
}
