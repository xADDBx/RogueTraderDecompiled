using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Settings;

[MemoryPackable(GenerateType.Object)]
public class AutoPauseValues : IMemoryPackable<AutoPauseValues>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AutoPauseValuesFormatter : MemoryPackFormatter<AutoPauseValues>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AutoPauseValues value)
		{
			AutoPauseValues.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AutoPauseValues value)
		{
			AutoPauseValues.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public bool PauseOnLostFocus { get; set; }

	[JsonProperty]
	public bool PauseOnTrapDetected { get; set; }

	[JsonProperty]
	public bool PauseOnHiddenObjectDetected { get; set; }

	[JsonProperty]
	public bool PauseOnAreaLoaded { get; set; }

	[JsonProperty]
	public bool PauseOnLoadingScreen { get; set; }

	static AutoPauseValues()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AutoPauseValues>())
		{
			MemoryPackFormatterProvider.Register(new AutoPauseValuesFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AutoPauseValues[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AutoPauseValues>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AutoPauseValues? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		bool value2 = value.PauseOnLostFocus;
		bool value3 = value.PauseOnTrapDetected;
		bool value4 = value.PauseOnHiddenObjectDetected;
		bool value5 = value.PauseOnAreaLoaded;
		bool value6 = value.PauseOnLoadingScreen;
		writer.WriteUnmanagedWithObjectHeader(5, in value2, in value3, in value4, in value5, in value6);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AutoPauseValues? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		bool value3;
		bool value4;
		bool value5;
		bool value6;
		if (memberCount == 5)
		{
			if (value != null)
			{
				value2 = value.PauseOnLostFocus;
				value3 = value.PauseOnTrapDetected;
				value4 = value.PauseOnHiddenObjectDetected;
				value5 = value.PauseOnAreaLoaded;
				value6 = value.PauseOnLoadingScreen;
				reader.ReadUnmanaged<bool>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
				reader.ReadUnmanaged<bool>(out value4);
				reader.ReadUnmanaged<bool>(out value5);
				reader.ReadUnmanaged<bool>(out value6);
				goto IL_0117;
			}
			reader.ReadUnmanaged<bool, bool, bool, bool, bool>(out value2, out value3, out value4, out value5, out value6);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AutoPauseValues), 5, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = false;
				value3 = false;
				value4 = false;
				value5 = false;
				value6 = false;
			}
			else
			{
				value2 = value.PauseOnLostFocus;
				value3 = value.PauseOnTrapDetected;
				value4 = value.PauseOnHiddenObjectDetected;
				value5 = value.PauseOnAreaLoaded;
				value6 = value.PauseOnLoadingScreen;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<bool>(out value5);
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
				goto IL_0117;
			}
		}
		value = new AutoPauseValues
		{
			PauseOnLostFocus = value2,
			PauseOnTrapDetected = value3,
			PauseOnHiddenObjectDetected = value4,
			PauseOnAreaLoaded = value5,
			PauseOnLoadingScreen = value6
		};
		return;
		IL_0117:
		value.PauseOnLostFocus = value2;
		value.PauseOnTrapDetected = value3;
		value.PauseOnHiddenObjectDetected = value4;
		value.PauseOnAreaLoaded = value5;
		value.PauseOnLoadingScreen = value6;
	}
}
