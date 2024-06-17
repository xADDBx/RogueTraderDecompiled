using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CameraFollowTimeScaleGameCommand : GameCommand, IMemoryPackable<CameraFollowTimeScaleGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CameraFollowTimeScaleGameCommandFormatter : MemoryPackFormatter<CameraFollowTimeScaleGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CameraFollowTimeScaleGameCommand value)
		{
			CameraFollowTimeScaleGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CameraFollowTimeScaleGameCommand value)
		{
			CameraFollowTimeScaleGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private byte m_Scale;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_Force;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private CameraFollowTimeScaleGameCommand()
	{
	}

	[MemoryPackConstructor]
	private CameraFollowTimeScaleGameCommand(byte m_scale, bool m_force)
	{
		m_Scale = m_scale;
		m_Force = m_force;
	}

	public CameraFollowTimeScaleGameCommand(float scale, bool force)
		: this(FloatToByte(scale), force)
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.TimeController.SetCameraFollowTimeScale(ByteToFloat(m_Scale), m_Force);
	}

	private static byte FloatToByte(float scale)
	{
		return (byte)(255f * Mathf.Clamp01(scale));
	}

	private static float ByteToFloat(byte scale)
	{
		return (float)(int)scale / 255f;
	}

	static CameraFollowTimeScaleGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CameraFollowTimeScaleGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CameraFollowTimeScaleGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CameraFollowTimeScaleGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CameraFollowTimeScaleGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CameraFollowTimeScaleGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_Scale, in value.m_Force);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CameraFollowTimeScaleGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<byte, bool>(out value2, out value3);
			}
			else
			{
				value2 = value.m_Scale;
				value3 = value.m_Force;
				reader.ReadUnmanaged<byte>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CameraFollowTimeScaleGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = false;
			}
			else
			{
				value2 = value.m_Scale;
				value3 = value.m_Force;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<byte>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CameraFollowTimeScaleGameCommand(value2, value3);
	}
}
