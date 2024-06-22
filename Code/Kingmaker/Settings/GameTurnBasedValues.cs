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
	public bool DisableActionCamera { get; set; }

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
		bool value5 = value.DisableActionCamera;
		float value6 = value.TimeScaleInPlayerTurn;
		float value7 = value.TimeScaleInNonPlayerTurn;
		writer.WriteUnmanagedWithObjectHeader(6, in value2, in value3, in value4, in value5, in value6, in value7);
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
		bool value5;
		float value6;
		float value7;
		if (memberCount == 6)
		{
			if (value != null)
			{
				value2 = value.SpeedUpMode;
				value3 = value.FastMovement;
				value4 = value.FastPartyCast;
				value5 = value.DisableActionCamera;
				value6 = value.TimeScaleInPlayerTurn;
				value7 = value.TimeScaleInNonPlayerTurn;
				reader.ReadUnmanaged<SpeedUpMode>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
				reader.ReadUnmanaged<bool>(out value4);
				reader.ReadUnmanaged<bool>(out value5);
				reader.ReadUnmanaged<float>(out value6);
				reader.ReadUnmanaged<float>(out value7);
				goto IL_014d;
			}
			reader.ReadUnmanaged<SpeedUpMode, bool, bool, bool, float, float>(out value2, out value3, out value4, out value5, out value6, out value7);
		}
		else
		{
			if (memberCount > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameTurnBasedValues), 6, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = SpeedUpMode.Off;
				value3 = false;
				value4 = false;
				value5 = false;
				value6 = 0f;
				value7 = 0f;
			}
			else
			{
				value2 = value.SpeedUpMode;
				value3 = value.FastMovement;
				value4 = value.FastPartyCast;
				value5 = value.DisableActionCamera;
				value6 = value.TimeScaleInPlayerTurn;
				value7 = value.TimeScaleInNonPlayerTurn;
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
							reader.ReadUnmanaged<bool>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<float>(out value6);
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<float>(out value7);
									_ = 6;
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_014d;
			}
		}
		value = new GameTurnBasedValues
		{
			SpeedUpMode = value2,
			FastMovement = value3,
			FastPartyCast = value4,
			DisableActionCamera = value5,
			TimeScaleInPlayerTurn = value6,
			TimeScaleInNonPlayerTurn = value7
		};
		return;
		IL_014d:
		value.SpeedUpMode = value2;
		value.FastMovement = value3;
		value.FastPartyCast = value4;
		value.DisableActionCamera = value5;
		value.TimeScaleInPlayerTurn = value6;
		value.TimeScaleInNonPlayerTurn = value7;
	}
}
