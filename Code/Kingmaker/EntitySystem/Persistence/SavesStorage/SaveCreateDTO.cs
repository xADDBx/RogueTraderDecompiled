using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Serialization;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence.SavesStorage;

[MemoryPackable(GenerateType.Object)]
public class SaveCreateDTO : IMemoryPackable<SaveCreateDTO>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SaveCreateDTOFormatter : MemoryPackFormatter<SaveCreateDTO>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SaveCreateDTO value)
		{
			SaveCreateDTO.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveCreateDTO value)
		{
			SaveCreateDTO.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public SaveMetadata Save { get; set; }

	[JsonProperty]
	public List<GameElementData> Elements { get; set; }

	[MemoryPackConstructor]
	private SaveCreateDTO()
	{
	}

	public static SaveCreateDTO Build([NotNull] SaveInfo save, [CanBeNull] Player player)
	{
		if (player == null)
		{
			try
			{
				string source = save.Saver.ReadJson("player");
				player = SaveSystemJsonSerializer.Serializer.DeserializeObject<Player>(source);
			}
			catch (Exception)
			{
				player = null;
			}
		}
		SaveCreateDTO saveCreateDTO = new SaveCreateDTO
		{
			Save = new SaveMetadata(save, player),
			Elements = new List<GameElementData>()
		};
		if (player != null)
		{
			foreach (KeyValuePair<BlueprintUnlockableFlag, int> unlockedFlag in player.UnlockableFlags.UnlockedFlags)
			{
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Flag, unlockedFlag.Key, unlockedFlag.Value.ToString()));
			}
			foreach (Quest quest in player.QuestBook.Quests)
			{
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Quest, quest.Blueprint, quest.State.ToString()));
				foreach (QuestObjective objective in quest.Objectives)
				{
					saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Objective, objective.Blueprint, objective.State.ToString()));
				}
			}
			foreach (BaseUnitEntity allCrossSceneUnit in player.AllCrossSceneUnits)
			{
				if (allCrossSceneUnit.IsPet)
				{
					continue;
				}
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Companion, allCrossSceneUnit.Blueprint, allCrossSceneUnit.LifeState.State.ToString()));
				string value = "Unknown";
				CompanionState companionState = allCrossSceneUnit.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None;
				if (allCrossSceneUnit.UniqueId == player.MainCharacter.Id)
				{
					value = "Player";
				}
				else
				{
					switch (companionState)
					{
					case CompanionState.InParty:
					case CompanionState.InPartyDetached:
						value = "Party";
						break;
					case CompanionState.ExCompanion:
						value = "Ex";
						break;
					case CompanionState.Remote:
						value = "Remote";
						break;
					}
				}
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Companion, allCrossSceneUnit.Blueprint, value));
				if (companionState == CompanionState.InPartyDetached)
				{
					saveCreateDTO.Elements.Add(new GameElementData(GameElementType.Companion, allCrossSceneUnit.Blueprint, "Detached"));
				}
			}
			foreach (int version in save.Versions)
			{
				saveCreateDTO.Elements.Add(new GameElementData(GameElementType.UpgradeVersion, null, version.ToString()));
			}
		}
		return saveCreateDTO;
	}

	private static bool ListHasUnit(IList<UnitReference> list, BaseUnitEntity unit)
	{
		foreach (UnitReference item in list)
		{
			if (item.Id == unit.UniqueId)
			{
				return true;
			}
		}
		return false;
	}

	static SaveCreateDTO()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveCreateDTO>())
		{
			MemoryPackFormatterProvider.Register(new SaveCreateDTOFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveCreateDTO[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveCreateDTO>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<GameElementData>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<GameElementData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SaveCreateDTO? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		SaveMetadata value2 = value.Save;
		writer.WritePackable(in value2);
		List<GameElementData> source = value.Elements;
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in source));
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveCreateDTO? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		SaveMetadata value2;
		List<GameElementData> value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.Save;
				value3 = value.Elements;
				reader.ReadPackable(ref value2);
				ListFormatter.DeserializePackable(ref reader, ref value3);
				goto IL_009a;
			}
			value2 = reader.ReadPackable<SaveMetadata>();
			value3 = ListFormatter.DeserializePackable<GameElementData>(ref reader);
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveCreateDTO), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
			}
			else
			{
				value2 = value.Save;
				value3 = value.Elements;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					ListFormatter.DeserializePackable(ref reader, ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009a;
			}
		}
		value = new SaveCreateDTO
		{
			Save = value2,
			Elements = value3
		};
		return;
		IL_009a:
		value.Save = value2;
		value.Elements = value3;
	}
}
