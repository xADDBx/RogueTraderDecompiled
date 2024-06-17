using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameInfo;
using Kingmaker.Utility.Serialization;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence.SavesStorage;

[MemoryPackable(GenerateType.Object)]
public class SaveMetadata : IMemoryPackable<SaveMetadata>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SaveMetadataFormatter : MemoryPackFormatter<SaveMetadata>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SaveMetadata value)
		{
			SaveMetadata.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveMetadata value)
		{
			SaveMetadata.Deserialize(ref reader, ref value);
		}
	}

	[CanBeNull]
	[JsonProperty]
	public string Username { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string DeviceId { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string CharacterName { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string SaveType { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string SaveName { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string Description { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string Version { get; set; }

	[JsonProperty]
	public TimeSpan RealTime { get; set; }

	[JsonProperty]
	public TimeSpan GameTime { get; set; }

	[JsonProperty]
	public string GameTimeText { get; set; }

	[JsonProperty]
	public int KingdomDay { get; set; } = -1;


	[CanBeNull]
	[JsonProperty]
	public string Zone { get; set; }

	[JsonProperty]
	public int Level { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string Chapter { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string MainQuest { get; set; }

	[CanBeNull]
	[JsonProperty]
	public string Alignment { get; set; }

	[MemoryPackConstructor]
	public SaveMetadata()
	{
	}

	public SaveMetadata([NotNull] SaveInfo save, [CanBeNull] Player player = null)
	{
		Version = GameVersion.Revision;
		Username = Environment.UserName;
		DeviceId = GameVersion.DeviceUniqueIdentifier;
		SaveType = save.Type.ToString();
		SaveName = save.Name;
		CharacterName = save.PlayerCharacterName;
		RealTime = save.GameTotalTime;
		GameTime = save.GameSaveTime;
		GameTimeText = save.GameSaveTimeText;
		if (save.Area != null)
		{
			Zone = save.Area.name;
		}
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
		if (player != null)
		{
			if (player.Chapter > 0)
			{
				Chapter = "c" + player.Chapter;
			}
			BaseUnitEntity baseUnitEntity = player.CrossSceneState.AllEntityData.OfType<BaseUnitEntity>().FirstOrDefault((BaseUnitEntity u) => u.UniqueId == player.MainCharacter.Id);
			if (baseUnitEntity != null)
			{
				Level = baseUnitEntity.Progression.CharacterLevel;
				Alignment = baseUnitEntity.Alignment.Value.ToString();
			}
		}
	}

	static SaveMetadata()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetadata>())
		{
			MemoryPackFormatterProvider.Register(new SaveMetadataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveMetadata[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveMetadata>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SaveMetadata? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(16);
		writer.WriteString(value.Username);
		writer.WriteString(value.DeviceId);
		writer.WriteString(value.CharacterName);
		writer.WriteString(value.SaveType);
		writer.WriteString(value.SaveName);
		writer.WriteString(value.Description);
		writer.WriteString(value.Version);
		TimeSpan value2 = value.RealTime;
		TimeSpan value3 = value.GameTime;
		writer.WriteUnmanaged(in value2, in value3);
		writer.WriteString(value.GameTimeText);
		int value4 = value.KingdomDay;
		writer.WriteUnmanaged(in value4);
		writer.WriteString(value.Zone);
		value4 = value.Level;
		writer.WriteUnmanaged(in value4);
		writer.WriteString(value.Chapter);
		writer.WriteString(value.MainQuest);
		writer.WriteString(value.Alignment);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveMetadata? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		TimeSpan value2;
		TimeSpan value3;
		int value4;
		int value5;
		string username;
		string deviceId;
		string characterName;
		string saveType;
		string saveName;
		string description;
		string version;
		string gameTimeText;
		string zone;
		string chapter;
		string mainQuest;
		string alignment;
		if (memberCount == 16)
		{
			if (value != null)
			{
				username = value.Username;
				deviceId = value.DeviceId;
				characterName = value.CharacterName;
				saveType = value.SaveType;
				saveName = value.SaveName;
				description = value.Description;
				version = value.Version;
				value2 = value.RealTime;
				value3 = value.GameTime;
				gameTimeText = value.GameTimeText;
				value4 = value.KingdomDay;
				zone = value.Zone;
				value5 = value.Level;
				chapter = value.Chapter;
				mainQuest = value.MainQuest;
				alignment = value.Alignment;
				username = reader.ReadString();
				deviceId = reader.ReadString();
				characterName = reader.ReadString();
				saveType = reader.ReadString();
				saveName = reader.ReadString();
				description = reader.ReadString();
				version = reader.ReadString();
				reader.ReadUnmanaged<TimeSpan>(out value2);
				reader.ReadUnmanaged<TimeSpan>(out value3);
				gameTimeText = reader.ReadString();
				reader.ReadUnmanaged<int>(out value4);
				zone = reader.ReadString();
				reader.ReadUnmanaged<int>(out value5);
				chapter = reader.ReadString();
				mainQuest = reader.ReadString();
				alignment = reader.ReadString();
				goto IL_036e;
			}
			username = reader.ReadString();
			deviceId = reader.ReadString();
			characterName = reader.ReadString();
			saveType = reader.ReadString();
			saveName = reader.ReadString();
			description = reader.ReadString();
			version = reader.ReadString();
			reader.ReadUnmanaged<TimeSpan, TimeSpan>(out value2, out value3);
			gameTimeText = reader.ReadString();
			reader.ReadUnmanaged<int>(out value4);
			zone = reader.ReadString();
			reader.ReadUnmanaged<int>(out value5);
			chapter = reader.ReadString();
			mainQuest = reader.ReadString();
			alignment = reader.ReadString();
		}
		else
		{
			if (memberCount > 16)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveMetadata), 16, memberCount);
				return;
			}
			if (value == null)
			{
				username = null;
				deviceId = null;
				characterName = null;
				saveType = null;
				saveName = null;
				description = null;
				version = null;
				value2 = default(TimeSpan);
				value3 = default(TimeSpan);
				gameTimeText = null;
				value4 = 0;
				zone = null;
				value5 = 0;
				chapter = null;
				mainQuest = null;
				alignment = null;
			}
			else
			{
				username = value.Username;
				deviceId = value.DeviceId;
				characterName = value.CharacterName;
				saveType = value.SaveType;
				saveName = value.SaveName;
				description = value.Description;
				version = value.Version;
				value2 = value.RealTime;
				value3 = value.GameTime;
				gameTimeText = value.GameTimeText;
				value4 = value.KingdomDay;
				zone = value.Zone;
				value5 = value.Level;
				chapter = value.Chapter;
				mainQuest = value.MainQuest;
				alignment = value.Alignment;
			}
			if (memberCount != 0)
			{
				username = reader.ReadString();
				if (memberCount != 1)
				{
					deviceId = reader.ReadString();
					if (memberCount != 2)
					{
						characterName = reader.ReadString();
						if (memberCount != 3)
						{
							saveType = reader.ReadString();
							if (memberCount != 4)
							{
								saveName = reader.ReadString();
								if (memberCount != 5)
								{
									description = reader.ReadString();
									if (memberCount != 6)
									{
										version = reader.ReadString();
										if (memberCount != 7)
										{
											reader.ReadUnmanaged<TimeSpan>(out value2);
											if (memberCount != 8)
											{
												reader.ReadUnmanaged<TimeSpan>(out value3);
												if (memberCount != 9)
												{
													gameTimeText = reader.ReadString();
													if (memberCount != 10)
													{
														reader.ReadUnmanaged<int>(out value4);
														if (memberCount != 11)
														{
															zone = reader.ReadString();
															if (memberCount != 12)
															{
																reader.ReadUnmanaged<int>(out value5);
																if (memberCount != 13)
																{
																	chapter = reader.ReadString();
																	if (memberCount != 14)
																	{
																		mainQuest = reader.ReadString();
																		if (memberCount != 15)
																		{
																			alignment = reader.ReadString();
																			_ = 16;
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_036e;
			}
		}
		value = new SaveMetadata
		{
			Username = username,
			DeviceId = deviceId,
			CharacterName = characterName,
			SaveType = saveType,
			SaveName = saveName,
			Description = description,
			Version = version,
			RealTime = value2,
			GameTime = value3,
			GameTimeText = gameTimeText,
			KingdomDay = value4,
			Zone = zone,
			Level = value5,
			Chapter = chapter,
			MainQuest = mainQuest,
			Alignment = alignment
		};
		return;
		IL_036e:
		value.Username = username;
		value.DeviceId = deviceId;
		value.CharacterName = characterName;
		value.SaveType = saveType;
		value.SaveName = saveName;
		value.Description = description;
		value.Version = version;
		value.RealTime = value2;
		value.GameTime = value3;
		value.GameTimeText = gameTimeText;
		value.KingdomDay = value4;
		value.Zone = zone;
		value.Level = value5;
		value.Chapter = chapter;
		value.MainQuest = mainQuest;
		value.Alignment = alignment;
	}
}
