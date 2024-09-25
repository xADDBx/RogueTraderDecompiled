using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Save;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public readonly struct SaveInfoShort : IMemoryPackable<SaveInfoShort>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SaveInfoShortFormatter : MemoryPackFormatter<SaveInfoShort>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SaveInfoShort value)
		{
			SaveInfoShort.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveInfoShort value)
		{
			SaveInfoShort.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public readonly string Name;

	[JsonProperty]
	public readonly BlueprintArea Area;

	[JsonProperty]
	public readonly string AreaNameOverride;

	[JsonProperty]
	public readonly List<PortraitForSave> PartyPortraits;

	[JsonProperty]
	public readonly DateTime SystemSaveTime;

	[JsonProperty]
	public readonly TimeSpan GameTotalTime;

	[JsonIgnore]
	[MemoryPackIgnore]
	public bool IsEmpty => SystemSaveTime == default(DateTime);

	[MemoryPackConstructor]
	public SaveInfoShort(string name, BlueprintArea area, string areaNameOverride, List<PortraitForSave> partyPortraits, DateTime systemSaveTime, TimeSpan gameTotalTime)
	{
		Name = name;
		Area = area;
		AreaNameOverride = areaNameOverride;
		PartyPortraits = partyPortraits;
		SystemSaveTime = systemSaveTime;
		GameTotalTime = gameTotalTime;
	}

	public SaveInfoShort(SaveInfo saveInfo)
		: this(saveInfo.Name, saveInfo.Area, saveInfo.AreaNameOverride, saveInfo.PartyPortraits, saveInfo.SystemSaveTime, saveInfo.GameTotalTime)
	{
	}

	public static explicit operator SaveInfo(SaveInfoShort saveInfoShort)
	{
		return new SaveInfo
		{
			Name = saveInfoShort.Name,
			Area = saveInfoShort.Area,
			AreaNameOverride = saveInfoShort.AreaNameOverride,
			PartyPortraits = saveInfoShort.PartyPortraits,
			SystemSaveTime = saveInfoShort.SystemSaveTime,
			GameTotalTime = saveInfoShort.GameTotalTime
		};
	}

	public static explicit operator SaveInfoShort(SaveInfo saveInfo)
	{
		return new SaveInfoShort(saveInfo);
	}

	static SaveInfoShort()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveInfoShort>())
		{
			MemoryPackFormatterProvider.Register(new SaveInfoShortFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveInfoShort[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveInfoShort>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<PortraitForSave>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<PortraitForSave>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SaveInfoShort value)
	{
		writer.WriteObjectHeader(6);
		writer.WriteString(value.Name);
		writer.WriteValue(in value.Area);
		writer.WriteString(value.AreaNameOverride);
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.PartyPortraits));
		writer.WriteUnmanaged(in value.SystemSaveTime, in value.GameTotalTime);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveInfoShort value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(SaveInfoShort);
			return;
		}
		string name;
		BlueprintArea value2;
		string areaNameOverride;
		List<PortraitForSave> value3;
		DateTime value4;
		TimeSpan value5;
		if (memberCount == 6)
		{
			name = reader.ReadString();
			value2 = reader.ReadValue<BlueprintArea>();
			areaNameOverride = reader.ReadString();
			value3 = ListFormatter.DeserializePackable<PortraitForSave>(ref reader);
			reader.ReadUnmanaged<DateTime, TimeSpan>(out value4, out value5);
		}
		else
		{
			if (memberCount > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveInfoShort), 6, memberCount);
				return;
			}
			name = null;
			value2 = null;
			areaNameOverride = null;
			value3 = null;
			value4 = default(DateTime);
			value5 = default(TimeSpan);
			if (memberCount != 0)
			{
				name = reader.ReadString();
				if (memberCount != 1)
				{
					reader.ReadValue(ref value2);
					if (memberCount != 2)
					{
						areaNameOverride = reader.ReadString();
						if (memberCount != 3)
						{
							ListFormatter.DeserializePackable(ref reader, ref value3);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<DateTime>(out value4);
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<TimeSpan>(out value5);
									_ = 6;
								}
							}
						}
					}
				}
			}
		}
		value = new SaveInfoShort(name, value2, areaNameOverride, value3, value4, value5);
	}
}
