using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public class CameraData : IMemoryPackable<CameraData>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class CameraDataFormatter : MemoryPackFormatter<CameraData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CameraData value)
		{
			CameraData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CameraData value)
		{
			CameraData.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(PropertyName = "p")]
	public Vector3 position;

	[JsonProperty(PropertyName = "q")]
	public Quaternion rotation;

	[JsonProperty(PropertyName = "pr")]
	public Vector4 projParams;

	[JsonProperty(PropertyName = "pp")]
	public Vector3 parentPosition;

	[JsonProperty(PropertyName = "sr")]
	public bool isScrollingByRoutine;

	[MemoryPackIgnore]
	public Matrix4x4 matrix;

	public bool IsEquals(CameraData other)
	{
		if (position.Equals(other.position) && rotation.Equals(other.rotation) && projParams.Equals(other.projParams) && isScrollingByRoutine.Equals(other.isScrollingByRoutine))
		{
			return parentPosition.Equals(other.parentPosition);
		}
		return false;
	}

	static CameraData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CameraData>())
		{
			MemoryPackFormatterProvider.Register(new CameraDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CameraData[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CameraData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CameraData? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(5, in value.position, in value.rotation, in value.projParams, in value.parentPosition, in value.isScrollingByRoutine);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CameraData? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Vector3 value2;
		Quaternion value3;
		Vector4 value4;
		Vector3 value5;
		bool value6;
		if (memberCount == 5)
		{
			if (value != null)
			{
				value2 = value.position;
				value3 = value.rotation;
				value4 = value.projParams;
				value5 = value.parentPosition;
				value6 = value.isScrollingByRoutine;
				reader.ReadUnmanaged<Vector3>(out value2);
				reader.ReadUnmanaged<Quaternion>(out value3);
				reader.ReadUnmanaged<Vector4>(out value4);
				reader.ReadUnmanaged<Vector3>(out value5);
				reader.ReadUnmanaged<bool>(out value6);
				goto IL_012e;
			}
			reader.ReadUnmanaged<Vector3, Quaternion, Vector4, Vector3, bool>(out value2, out value3, out value4, out value5, out value6);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CameraData), 5, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(Vector3);
				value3 = default(Quaternion);
				value4 = default(Vector4);
				value5 = default(Vector3);
				value6 = false;
			}
			else
			{
				value2 = value.position;
				value3 = value.rotation;
				value4 = value.projParams;
				value5 = value.parentPosition;
				value6 = value.isScrollingByRoutine;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Vector3>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<Quaternion>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<Vector4>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<Vector3>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<bool>(out value6);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_012e;
			}
		}
		value = new CameraData
		{
			position = value2,
			rotation = value3,
			projParams = value4,
			parentPosition = value5,
			isScrollingByRoutine = value6
		};
		return;
		IL_012e:
		value.position = value2;
		value.rotation = value3;
		value.projParams = value4;
		value.parentPosition = value5;
		value.isScrollingByRoutine = value6;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref position);
		result.Append(ref rotation);
		result.Append(ref projParams);
		result.Append(ref parentPosition);
		result.Append(ref isScrollingByRoutine);
		return result;
	}
}
