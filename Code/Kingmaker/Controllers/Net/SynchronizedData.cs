using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public struct SynchronizedData : IMemoryPackable<SynchronizedData>, IMemoryPackFormatterRegister, IHashable
{
	public static class CameraDataType
	{
		public const byte None = 0;

		public const byte NoData = 1;

		public const byte Repeat = 2;

		public const byte RigCamera = 3;

		public const byte NonRigCamera = 4;
	}

	[Preserve]
	private sealed class SynchronizedDataFormatter : MemoryPackFormatter<SynchronizedData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SynchronizedData value)
		{
			SynchronizedData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SynchronizedData value)
		{
			SynchronizedData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "ct")]
	[MemoryPackInclude]
	public byte cameraType;

	[JsonProperty(PropertyName = "cm")]
	[MemoryPackInclude]
	public CameraData camera;

	[JsonProperty(PropertyName = "ls")]
	[MemoryPackInclude]
	public LeftStickData leftStick;

	[JsonProperty(PropertyName = "sh")]
	[MemoryPackInclude]
	public int stateHash;

	[JsonProperty(PropertyName = "ml")]
	[MemoryPackInclude]
	public byte maxLag;

	[MemoryPackIgnore]
	public Matrix4x4 cameraMatrix => camera?.matrix ?? Matrix4x4.zero;

	[MemoryPackIgnore]
	public bool IsEmpty
	{
		get
		{
			if (cameraType == 0 && camera == null && leftStick == null && stateHash == 0)
			{
				return maxLag == 0;
			}
			return false;
		}
	}

	[MemoryPackConstructor]
	private SynchronizedData(byte cameraType, CameraData camera, LeftStickData leftStick, int stateHash, byte maxLag)
	{
		this.cameraType = cameraType;
		this.camera = camera;
		this.leftStick = leftStick;
		this.stateHash = stateHash;
		this.maxLag = maxLag;
	}

	public SynchronizedData(SynchronizedData other, byte cameraType, CameraData camera = null)
		: this(cameraType, camera, other.leftStick, other.stateHash, other.maxLag)
	{
	}

	static SynchronizedData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SynchronizedData>())
		{
			MemoryPackFormatterProvider.Register(new SynchronizedDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SynchronizedData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SynchronizedData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SynchronizedData value)
	{
		writer.WriteUnmanagedWithObjectHeader(5, in value.cameraType);
		writer.WritePackable(in value.camera);
		writer.WritePackable(in value.leftStick);
		writer.WriteUnmanaged(in value.stateHash, in value.maxLag);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SynchronizedData value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(SynchronizedData);
			return;
		}
		byte value2;
		CameraData value3;
		LeftStickData value4;
		int value5;
		byte value6;
		if (memberCount == 5)
		{
			reader.ReadUnmanaged<byte>(out value2);
			value3 = reader.ReadPackable<CameraData>();
			value4 = reader.ReadPackable<LeftStickData>();
			reader.ReadUnmanaged<int, byte>(out value5, out value6);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SynchronizedData), 5, memberCount);
				return;
			}
			value2 = 0;
			value3 = null;
			value4 = null;
			value5 = 0;
			value6 = 0;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<byte>(out value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<int>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<byte>(out value6);
								_ = 5;
							}
						}
					}
				}
			}
		}
		value = new SynchronizedData(value2, value3, value4, value5, value6);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref cameraType);
		Hash128 val = ClassHasher<CameraData>.GetHash128(camera);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<LeftStickData>.GetHash128(leftStick);
		result.Append(ref val2);
		result.Append(ref stateHash);
		result.Append(ref maxLag);
		return result;
	}
}
