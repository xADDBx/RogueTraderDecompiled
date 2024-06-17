using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Localization;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence;

[MemoryPackable(GenerateType.Object)]
public class SaveInfo : IDisposable, IMemoryPackable<SaveInfo>, IMemoryPackFormatterRegister
{
	public enum SaveType
	{
		Manual,
		Quick,
		Auto,
		Remote,
		Bugreport,
		IronMan,
		ForImport,
		Coop
	}

	public enum StateType
	{
		None,
		Serializing,
		Saving
	}

	private class ReadLockScope : IDisposable
	{
		private SaveInfo m_SaveInfo;

		private readonly bool m_Upgradeable;

		public ReadLockScope(SaveInfo saveInfo, bool upgradeable)
		{
			m_Upgradeable = upgradeable;
			if (!(upgradeable ? saveInfo.m_FileAccessLock.TryEnterUpgradeableReadLock(20.Seconds()) : saveInfo.m_FileAccessLock.TryEnterReadLock(20.Seconds())))
			{
				throw new Exception("Cannot get access to file: possible deadlock");
			}
			saveInfo.UpdateSaverMode();
			m_SaveInfo = saveInfo;
		}

		public void Dispose()
		{
			if (m_Upgradeable)
			{
				m_SaveInfo?.m_FileAccessLock.ExitUpgradeableReadLock();
			}
			else
			{
				m_SaveInfo?.m_FileAccessLock.ExitReadLock();
			}
			m_SaveInfo?.UpdateSaverMode();
			m_SaveInfo = null;
		}
	}

	private class WriteLockScope : IDisposable
	{
		private SaveInfo m_SaveInfo;

		public WriteLockScope(SaveInfo saveInfo)
		{
			if (!saveInfo.m_FileAccessLock.TryEnterWriteLock(20.Seconds()))
			{
				throw new Exception("Cannot get access to file: possible deadlock");
			}
			saveInfo.UpdateSaverMode();
			m_SaveInfo = saveInfo;
		}

		public void Dispose()
		{
			m_SaveInfo?.m_FileAccessLock.ExitWriteLock();
			m_SaveInfo?.UpdateSaverMode();
			m_SaveInfo = null;
		}
	}

	[Preserve]
	private sealed class SaveInfoFormatter : MemoryPackFormatter<SaveInfo>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SaveInfo value)
		{
			SaveInfo.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SaveInfo value)
		{
			SaveInfo.Deserialize(ref reader, ref value);
		}
	}

	public const int CurrentCompatibilityVersion = 1;

	private readonly ReaderWriterLockSlim m_FileAccessLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	[JsonProperty]
	private List<BlueprintDlcRewardReference> m_DlcRewards;

	[JsonProperty]
	public List<SerializableRandState> StatefulRandomStates;

	[MemoryPackIgnore]
	public volatile StateType m_OperationState;

	[JsonProperty]
	public string Name { get; set; }

	[JsonProperty]
	public string Description { get; set; }

	[JsonProperty]
	public string SaveId { get; set; }

	[JsonProperty]
	public BlueprintQuest QuestWithSaveDescription { private get; set; }

	[JsonProperty]
	public string PlayerCharacterName { get; set; }

	[JsonProperty]
	public int PlayerCharacterRank { get; set; }

	[JsonProperty]
	public string GameId { get; set; }

	[JsonProperty]
	public BlueprintCampaignReference DLCCampaign { get; set; }

	public IEnumerable<BlueprintDlcReward> DlcRewards
	{
		get
		{
			return m_DlcRewards?.Dereference();
		}
		set
		{
			m_DlcRewards = value.Select((BlueprintDlcReward dlc) => dlc.ToReference<BlueprintDlcRewardReference>()).ToList();
		}
	}

	[JsonProperty]
	public SaveType Type { get; set; }

	[JsonProperty]
	public bool IsAutoLevelupSave { get; set; }

	[JsonProperty]
	public int QuickSaveNumber { get; set; }

	[JsonProperty]
	public int LoadedTimes { get; set; }

	[JsonProperty]
	public BlueprintArea Area { get; set; }

	[JsonProperty]
	public BlueprintAreaPart AreaPart { get; set; }

	[JsonProperty]
	public string AreaNameOverride { get; set; }

	[JsonProperty]
	public List<PortraitForSave> PartyPortraits { get; set; }

	[JsonProperty]
	public DateTime SystemSaveTime { get; set; }

	[JsonProperty]
	public TimeSpan GameSaveTime { get; set; }

	[JsonProperty]
	public string GameSaveTimeText { get; set; }

	[JsonProperty]
	public TimeSpan GameTotalTime { get; set; }

	[JsonProperty]
	public List<int> Versions { get; set; } = new List<int>();


	[JsonProperty]
	public int CompatibilityVersion { get; set; } = 1;


	[MemoryPackIgnore]
	public string FolderName { get; set; }

	[MemoryPackIgnore]
	public Texture2D Screenshot { get; set; }

	[MemoryPackIgnore]
	public Texture2D ScreenshotHighRes { get; set; }

	[MemoryPackIgnore]
	public ISaver Saver { get; set; }

	[MemoryPackIgnore]
	public string LocalizedDescription
	{
		get
		{
			LocalizedString localizedString = QuestWithSaveDescription?.Description;
			if (localizedString == null)
			{
				return Description;
			}
			return localizedString;
		}
	}

	[MemoryPackIgnore]
	public string FileName
	{
		get
		{
			if (!IsActuallySaved)
			{
				return null;
			}
			return Path.GetFileName(FolderName);
		}
	}

	[MemoryPackIgnore]
	public bool IsActuallySaved => !string.IsNullOrEmpty(FolderName);

	[MemoryPackIgnore]
	public StateType OperationState
	{
		get
		{
			return m_OperationState;
		}
		set
		{
			m_OperationState = value;
		}
	}

	public bool CheckDlcAvailable()
	{
		using (ContextData<DlcExtension.LoadSaveDlcCheck>.Request())
		{
			return DlcRewards?.All((BlueprintDlcReward dlcReward) => dlcReward.IsAvailable) ?? true;
		}
	}

	public List<List<IBlueprintDlc>> GetRequiredDLCMap()
	{
		using (ContextData<DlcExtension.LoadSaveDlcCheck>.Request())
		{
			List<List<IBlueprintDlc>> list = new List<List<IBlueprintDlc>>();
			foreach (BlueprintDlcReward dlcReward in DlcRewards)
			{
				List<IBlueprintDlc> list2 = new List<IBlueprintDlc>();
				foreach (IBlueprintDlc dlc in dlcReward.Dlcs)
				{
					if (!dlc.IsAvailable && !list.Any((List<IBlueprintDlc> r) => r.Contains(dlc)))
					{
						list2.Add(dlc);
					}
				}
				if (list2.Any())
				{
					list.Add(list2);
				}
			}
			return list;
		}
	}

	public void Dispose()
	{
		SaveScreenshotManager.Instance.DisposeScreenshotTexture(ScreenshotHighRes);
		SaveScreenshotManager.Instance.DisposeScreenshotTexture(Screenshot);
		ScreenshotHighRes = null;
		Screenshot = null;
	}

	public IDisposable GetReadScope(bool upgradeable = false)
	{
		return new ReadLockScope(this, upgradeable);
	}

	public IDisposable GetWriteScope()
	{
		return new WriteLockScope(this);
	}

	private void UpdateSaverMode()
	{
		ISaver.Mode mode = (m_FileAccessLock.IsWriteLockHeld ? ISaver.Mode.Write : ((m_FileAccessLock.IsReadLockHeld || m_FileAccessLock.IsUpgradeableReadLockHeld) ? ISaver.Mode.Read : ISaver.Mode.None));
		Saver?.SetMode(mode);
	}

	public override string ToString()
	{
		return $"Save {Name}(Area:{Area},Part:{AreaPart})";
	}

	static SaveInfo()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SaveInfo>())
		{
			MemoryPackFormatterProvider.Register(new SaveInfoFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveInfo[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SaveInfo>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<IEnumerable<BlueprintDlcReward>>())
		{
			MemoryPackFormatterProvider.Register(new InterfaceEnumerableFormatter<BlueprintDlcReward>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SaveType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<SaveType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<PortraitForSave>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<PortraitForSave>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<int>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<SerializableRandState>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<SerializableRandState>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SaveInfo? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(24);
		writer.WriteString(value.Name);
		writer.WriteString(value.Description);
		writer.WriteString(value.SaveId);
		BlueprintQuest value2 = value.QuestWithSaveDescription;
		writer.WriteValue(in value2);
		writer.WriteString(value.PlayerCharacterName);
		int value3 = value.PlayerCharacterRank;
		writer.WriteUnmanaged(in value3);
		writer.WriteString(value.GameId);
		BlueprintCampaignReference value4 = value.DLCCampaign;
		writer.WritePackable(in value4);
		IEnumerable<BlueprintDlcReward> value5 = value.DlcRewards;
		writer.WriteValue(in value5);
		SaveType value6 = value.Type;
		bool value7 = value.IsAutoLevelupSave;
		value3 = value.QuickSaveNumber;
		int value8 = value.LoadedTimes;
		writer.WriteUnmanaged(in value6, in value7, in value3, in value8);
		BlueprintArea value9 = value.Area;
		writer.WriteValue(in value9);
		BlueprintAreaPart value10 = value.AreaPart;
		writer.WriteValue(in value10);
		writer.WriteString(value.AreaNameOverride);
		List<PortraitForSave> source = value.PartyPortraits;
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in source));
		DateTime value11 = value.SystemSaveTime;
		TimeSpan value12 = value.GameSaveTime;
		writer.WriteUnmanaged(in value11, in value12);
		writer.WriteString(value.GameSaveTimeText);
		value12 = value.GameTotalTime;
		writer.WriteUnmanaged(in value12);
		List<int> value13 = value.Versions;
		writer.WriteValue(in value13);
		value3 = value.CompatibilityVersion;
		writer.WriteUnmanaged(in value3);
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.StatefulRandomStates));
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SaveInfo? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintQuest value2;
		int value3;
		BlueprintCampaignReference value4;
		IEnumerable<BlueprintDlcReward> value5;
		SaveType value6;
		bool value7;
		int value8;
		int value9;
		BlueprintArea value10;
		BlueprintAreaPart value11;
		List<PortraitForSave> value12;
		DateTime value13;
		TimeSpan value14;
		TimeSpan value15;
		List<int> value16;
		int value17;
		List<SerializableRandState> value18;
		string name;
		string description;
		string saveId;
		string playerCharacterName;
		string gameId;
		string areaNameOverride;
		string gameSaveTimeText;
		if (memberCount == 24)
		{
			if (value != null)
			{
				name = value.Name;
				description = value.Description;
				saveId = value.SaveId;
				value2 = value.QuestWithSaveDescription;
				playerCharacterName = value.PlayerCharacterName;
				value3 = value.PlayerCharacterRank;
				gameId = value.GameId;
				value4 = value.DLCCampaign;
				value5 = value.DlcRewards;
				value6 = value.Type;
				value7 = value.IsAutoLevelupSave;
				value8 = value.QuickSaveNumber;
				value9 = value.LoadedTimes;
				value10 = value.Area;
				value11 = value.AreaPart;
				areaNameOverride = value.AreaNameOverride;
				value12 = value.PartyPortraits;
				value13 = value.SystemSaveTime;
				value14 = value.GameSaveTime;
				gameSaveTimeText = value.GameSaveTimeText;
				value15 = value.GameTotalTime;
				value16 = value.Versions;
				value17 = value.CompatibilityVersion;
				value18 = value.StatefulRandomStates;
				name = reader.ReadString();
				description = reader.ReadString();
				saveId = reader.ReadString();
				reader.ReadValue(ref value2);
				playerCharacterName = reader.ReadString();
				reader.ReadUnmanaged<int>(out value3);
				gameId = reader.ReadString();
				reader.ReadPackable(ref value4);
				reader.ReadValue(ref value5);
				reader.ReadUnmanaged<SaveType>(out value6);
				reader.ReadUnmanaged<bool>(out value7);
				reader.ReadUnmanaged<int>(out value8);
				reader.ReadUnmanaged<int>(out value9);
				reader.ReadValue(ref value10);
				reader.ReadValue(ref value11);
				areaNameOverride = reader.ReadString();
				ListFormatter.DeserializePackable(ref reader, ref value12);
				reader.ReadUnmanaged<DateTime>(out value13);
				reader.ReadUnmanaged<TimeSpan>(out value14);
				gameSaveTimeText = reader.ReadString();
				reader.ReadUnmanaged<TimeSpan>(out value15);
				reader.ReadValue(ref value16);
				reader.ReadUnmanaged<int>(out value17);
				ListFormatter.DeserializePackable(ref reader, ref value18);
				goto IL_050c;
			}
			name = reader.ReadString();
			description = reader.ReadString();
			saveId = reader.ReadString();
			value2 = reader.ReadValue<BlueprintQuest>();
			playerCharacterName = reader.ReadString();
			reader.ReadUnmanaged<int>(out value3);
			gameId = reader.ReadString();
			value4 = reader.ReadPackable<BlueprintCampaignReference>();
			value5 = reader.ReadValue<IEnumerable<BlueprintDlcReward>>();
			reader.ReadUnmanaged<SaveType, bool, int, int>(out value6, out value7, out value8, out value9);
			value10 = reader.ReadValue<BlueprintArea>();
			value11 = reader.ReadValue<BlueprintAreaPart>();
			areaNameOverride = reader.ReadString();
			value12 = ListFormatter.DeserializePackable<PortraitForSave>(ref reader);
			reader.ReadUnmanaged<DateTime, TimeSpan>(out value13, out value14);
			gameSaveTimeText = reader.ReadString();
			reader.ReadUnmanaged<TimeSpan>(out value15);
			value16 = reader.ReadValue<List<int>>();
			reader.ReadUnmanaged<int>(out value17);
			value18 = ListFormatter.DeserializePackable<SerializableRandState>(ref reader);
		}
		else
		{
			if (memberCount > 24)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveInfo), 24, memberCount);
				return;
			}
			if (value == null)
			{
				name = null;
				description = null;
				saveId = null;
				value2 = null;
				playerCharacterName = null;
				value3 = 0;
				gameId = null;
				value4 = null;
				value5 = null;
				value6 = SaveType.Manual;
				value7 = false;
				value8 = 0;
				value9 = 0;
				value10 = null;
				value11 = null;
				areaNameOverride = null;
				value12 = null;
				value13 = default(DateTime);
				value14 = default(TimeSpan);
				gameSaveTimeText = null;
				value15 = default(TimeSpan);
				value16 = null;
				value17 = 0;
				value18 = null;
			}
			else
			{
				name = value.Name;
				description = value.Description;
				saveId = value.SaveId;
				value2 = value.QuestWithSaveDescription;
				playerCharacterName = value.PlayerCharacterName;
				value3 = value.PlayerCharacterRank;
				gameId = value.GameId;
				value4 = value.DLCCampaign;
				value5 = value.DlcRewards;
				value6 = value.Type;
				value7 = value.IsAutoLevelupSave;
				value8 = value.QuickSaveNumber;
				value9 = value.LoadedTimes;
				value10 = value.Area;
				value11 = value.AreaPart;
				areaNameOverride = value.AreaNameOverride;
				value12 = value.PartyPortraits;
				value13 = value.SystemSaveTime;
				value14 = value.GameSaveTime;
				gameSaveTimeText = value.GameSaveTimeText;
				value15 = value.GameTotalTime;
				value16 = value.Versions;
				value17 = value.CompatibilityVersion;
				value18 = value.StatefulRandomStates;
			}
			if (memberCount != 0)
			{
				name = reader.ReadString();
				if (memberCount != 1)
				{
					description = reader.ReadString();
					if (memberCount != 2)
					{
						saveId = reader.ReadString();
						if (memberCount != 3)
						{
							reader.ReadValue(ref value2);
							if (memberCount != 4)
							{
								playerCharacterName = reader.ReadString();
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<int>(out value3);
									if (memberCount != 6)
									{
										gameId = reader.ReadString();
										if (memberCount != 7)
										{
											reader.ReadPackable(ref value4);
											if (memberCount != 8)
											{
												reader.ReadValue(ref value5);
												if (memberCount != 9)
												{
													reader.ReadUnmanaged<SaveType>(out value6);
													if (memberCount != 10)
													{
														reader.ReadUnmanaged<bool>(out value7);
														if (memberCount != 11)
														{
															reader.ReadUnmanaged<int>(out value8);
															if (memberCount != 12)
															{
																reader.ReadUnmanaged<int>(out value9);
																if (memberCount != 13)
																{
																	reader.ReadValue(ref value10);
																	if (memberCount != 14)
																	{
																		reader.ReadValue(ref value11);
																		if (memberCount != 15)
																		{
																			areaNameOverride = reader.ReadString();
																			if (memberCount != 16)
																			{
																				ListFormatter.DeserializePackable(ref reader, ref value12);
																				if (memberCount != 17)
																				{
																					reader.ReadUnmanaged<DateTime>(out value13);
																					if (memberCount != 18)
																					{
																						reader.ReadUnmanaged<TimeSpan>(out value14);
																						if (memberCount != 19)
																						{
																							gameSaveTimeText = reader.ReadString();
																							if (memberCount != 20)
																							{
																								reader.ReadUnmanaged<TimeSpan>(out value15);
																								if (memberCount != 21)
																								{
																									reader.ReadValue(ref value16);
																									if (memberCount != 22)
																									{
																										reader.ReadUnmanaged<int>(out value17);
																										if (memberCount != 23)
																										{
																											ListFormatter.DeserializePackable(ref reader, ref value18);
																											_ = 24;
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
				goto IL_050c;
			}
		}
		value = new SaveInfo
		{
			Name = name,
			Description = description,
			SaveId = saveId,
			QuestWithSaveDescription = value2,
			PlayerCharacterName = playerCharacterName,
			PlayerCharacterRank = value3,
			GameId = gameId,
			DLCCampaign = value4,
			DlcRewards = value5,
			Type = value6,
			IsAutoLevelupSave = value7,
			QuickSaveNumber = value8,
			LoadedTimes = value9,
			Area = value10,
			AreaPart = value11,
			AreaNameOverride = areaNameOverride,
			PartyPortraits = value12,
			SystemSaveTime = value13,
			GameSaveTime = value14,
			GameSaveTimeText = gameSaveTimeText,
			GameTotalTime = value15,
			Versions = value16,
			CompatibilityVersion = value17,
			StatefulRandomStates = value18
		};
		return;
		IL_050c:
		value.Name = name;
		value.Description = description;
		value.SaveId = saveId;
		value.QuestWithSaveDescription = value2;
		value.PlayerCharacterName = playerCharacterName;
		value.PlayerCharacterRank = value3;
		value.GameId = gameId;
		value.DLCCampaign = value4;
		value.DlcRewards = value5;
		value.Type = value6;
		value.IsAutoLevelupSave = value7;
		value.QuickSaveNumber = value8;
		value.LoadedTimes = value9;
		value.Area = value10;
		value.AreaPart = value11;
		value.AreaNameOverride = areaNameOverride;
		value.PartyPortraits = value12;
		value.SystemSaveTime = value13;
		value.GameSaveTime = value14;
		value.GameSaveTimeText = gameSaveTimeText;
		value.GameTotalTime = value15;
		value.Versions = value16;
		value.CompatibilityVersion = value17;
		value.StatefulRandomStates = value18;
	}
}
