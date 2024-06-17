using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Settings;

[MemoryPackable(GenerateType.Object)]
public class GameTurnBasedValues : IMemoryPackable<GameTurnBasedValues>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GameTurnBasedValuesFormatter : MemoryPackFormatter<GameTurnBasedValues>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GameTurnBasedValues value)
		{
			GameTurnBasedValues.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GameTurnBasedValues value)
		{
			GameTurnBasedValues.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public SpeedUpMode SpeedUpMode { get; set; }

	[JsonProperty]
	public bool FastMovement { get; set; }

	[JsonProperty]
	public bool FastPartyCast { get; set; }

	[JsonProperty]
	public float TimeScaleInPlayerTurn { get; set; }

	[JsonProperty]
	public float TimeScaleInNonPlayerTurn { get; set; }

	static GameTurnBasedValues()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GameTurnBasedValues>())
		{
			MemoryPackFormatterProvider.Register(new GameTurnBasedValuesFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameTurnBasedValues[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<GameTurnBasedValues>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SpeedUpMode>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<SpeedUpMode>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GameTurnBasedValues? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		SpeedUpMode value2 = value.SpeedUpMode;
		bool value3 = value.FastMovement;
		bool value4 = value.FastPartyCast;
		float value5 = value.TimeScaleInPlayerTurn;
		float value6 = value.TimeScaleInNonPlayerTurn;
		writer.WriteUnmanagedWithObjectHeader(5, in value2, in value3, in value4, in value5, in value6);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GameTurnBasedValues? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		SpeedUpMode value2;
		bool value3;
		bool value4;
		float value5;
		float value6;
		if (memberCount == 5)
		{
			if (value != null)
			{
				value2 = value.SpeedUpMode;
				value3 = value.FastMovement;
				value4 = value.FastPartyCast;
				value5 = value.TimeScaleInPlayerTurn;
				value6 = value.TimeScaleInNonPlayerTurn;
				reader.ReadUnmanaged<SpeedUpMode>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
				reader.ReadUnmanaged<bool>(out value4);
				reader.ReadUnmanaged<float>(out value5);
				reader.ReadUnmanaged<float>(out value6);
				goto IL_011f;
			}
			reader.ReadUnmanaged<SpeedUpMode, bool, bool, float, float>(out value2, out value3, out value4, out value5, out value6);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameTurnBasedValues), 5, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = SpeedUpMode.Off;
				value3 = false;
				value4 = false;
				value5 = 0f;
				value6 = 0f;
			}
			else
			{
				value2 = value.SpeedUpMode;
				value3 = value.FastMovement;
				value4 = value.FastPartyCast;
				value5 = value.TimeScaleInPlayerTurn;
				value6 = value.TimeScaleInNonPlayerTurn;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<SpeedUpMode>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<float>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<float>(out value6);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_011f;
			}
		}
		value = new GameTurnBasedValues
		{
			SpeedUpMode = value2,
			FastMovement = value3,
			FastPartyCast = value4,
			TimeScaleInPlayerTurn = value5,
			TimeScaleInNonPlayerTurn = value6
		};
		return;
		IL_011f:
		value.SpeedUpMode = value2;
		value.FastMovement = value3;
		value.FastPartyCast = value4;
		value.TimeScaleInPlayerTurn = value5;
		value.TimeScaleInNonPlayerTurn = value6;
	}
}
