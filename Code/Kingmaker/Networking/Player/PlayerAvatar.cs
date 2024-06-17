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
	private PlayerAvatar(ushort width, byte[] data)
	{
		Width = width;
		Data = data;
	}

	public PlayerAvatar(int width, byte[] data)
		: this((ushort)width, data)
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
		writer.WriteUnmanagedWithObjectHeader(2, in value.Width);
		writer.WriteUnmanagedArray(value.Data);
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
		if (memberCount == 2)
		{
			reader.ReadUnmanaged<ushort>(out value2);
			value3 = reader.ReadUnmanagedArray<byte>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PlayerAvatar), 2, memberCount);
				return;
			}
			value2 = 0;
			value3 = null;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<ushort>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanagedArray(ref value3);
					_ = 2;
				}
			}
		}
		value = new PlayerAvatar(value2, value3);
	}
}
