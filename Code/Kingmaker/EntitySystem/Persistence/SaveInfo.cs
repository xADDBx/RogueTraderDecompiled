using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
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

		public WriteLockScope(SaveInfo saveInfo, bool writeOnlyMode)
		{
			if (!saveInfo.m_FileAccessLock.TryEnterWriteLock(20.Seconds()))
			{
				throw new Exception("Cannot get access to file: possible deadlock");
			}
			saveInfo.UpdateSaverMode(writeOnlyMode);
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
	private BlueprintCampaignReference m_CampaignReference;

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
	public BlueprintArea Area { get; set; }

	[JsonProperty]
	public BlueprintAreaPart AreaPart { get; set; }

	[JsonProperty]
	public string AreaNameOverride { get; set; }

	[JsonProperty]
	public List<PortraitForSave> PartyPortraits { get; set; }

	[JsonProperty]
	public DateTime? GameStartSystemTime { get; set; }

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
	public BlueprintCampaign Campaign
	{
		get
		{
			if (m_CampaignReference != null && !m_CampaignReference.IsEmpty())
			{
				return m_CampaignReference.Get();
			}
			return null;
		}
		set
		{
			m_CampaignReference = value?.ToReference<BlueprintCampaignReference>();
		}
	}

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
			foreach (BlueprintDlcReward item in DlcRewards.Where((BlueprintDlcReward reward) => !reward.IsAvailable))
			{
				List<IBlueprintDlc> list2 = new List<IBlueprintDlc>();
				foreach (IBlueprintDlc dlc in item.Dlcs)
				{
					if (!dlc.IsActive && !list.Any((List<IBlueprintDlc> r) => r.Contains(dlc)))
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

	public IDisposable GetWriteScope(bool writeOnly = false)
	{
		return new WriteLockScope(this, writeOnly);
	}

	private void UpdateSaverMode(bool writeOnlyMode = false)
	{
		ISaver.Mode mode = ((!m_FileAccessLock.IsWriteLockHeld) ? ((m_FileAccessLock.IsReadLockHeld || m_FileAccessLock.IsUpgradeableReadLockHeld) ? ISaver.Mode.Read : ISaver.Mode.None) : (writeOnlyMode ? ISaver.Mode.WriteOnly : ISaver.Mode.Write));
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
		if (!MemoryPackFormatterProvider.IsRegistered<DateTime?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<DateTime>());
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
		writer.WriteObjectHeader(23);
		writer.WriteString(value.Name);
		writer.WriteString(value.Description);
		writer.WriteString(value.SaveId);
		BlueprintQuest value2 = value.QuestWithSaveDescription;
		writer.WriteValue(in value2);
		writer.WriteString(value.PlayerCharacterName);
		int value3 = value.PlayerCharacterRank;
		writer.WriteUnmanaged(in value3);
		writer.WriteString(value.GameId);
		IEnumerable<BlueprintDlcReward> value4 = value.DlcRewards;
		writer.WriteValue(in value4);
		SaveType value5 = value.Type;
		bool value6 = value.IsAutoLevelupSave;
		value3 = value.QuickSaveNumber;
		writer.WriteUnmanaged(in value5, in value6, in value3);
		BlueprintArea value7 = value.Area;
		writer.WriteValue(in value7);
		BlueprintAreaPart value8 = value.AreaPart;
		writer.WriteValue(in value8);
		writer.WriteString(value.AreaNameOverride);
		List<PortraitForSave> source = value.PartyPortraits;
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in source));
		DateTime? value9 = value.GameStartSystemTime;
		DateTime value10 = value.SystemSaveTime;
		TimeSpan value11 = value.GameSaveTime;
		writer.DangerousWriteUnmanaged(in value9, in value10, in value11);
		writer.WriteString(value.GameSaveTimeText);
		value11 = value.GameTotalTime;
		writer.WriteUnmanaged(in value11);
		List<int> value12 = value.Versions;
		writer.WriteValue(in value12);
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
		IEnumerable<BlueprintDlcReward> value4;
		SaveType value5;
		bool value6;
		int value7;
		BlueprintArea value8;
		BlueprintAreaPart value9;
		List<PortraitForSave> value10;
		DateTime? value11;
		DateTime value12;
		TimeSpan value13;
		TimeSpan value14;
		List<int> value15;
		int value16;
		List<SerializableRandState> value17;
		string name;
		string description;
		string saveId;
		string playerCharacterName;
		string gameId;
		string areaNameOverride;
		string gameSaveTimeText;
		if (memberCount == 23)
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
				value4 = value.DlcRewards;
				value5 = value.Type;
				value6 = value.IsAutoLevelupSave;
				value7 = value.QuickSaveNumber;
				value8 = value.Area;
				value9 = value.AreaPart;
				areaNameOverride = value.AreaNameOverride;
				value10 = value.PartyPortraits;
				value11 = value.GameStartSystemTime;
				value12 = value.SystemSaveTime;
				value13 = value.GameSaveTime;
				gameSaveTimeText = value.GameSaveTimeText;
				value14 = value.GameTotalTime;
				value15 = value.Versions;
				value16 = value.CompatibilityVersion;
				value17 = value.StatefulRandomStates;
				name = reader.ReadString();
				description = reader.ReadString();
				saveId = reader.ReadString();
				reader.ReadValue(ref value2);
				playerCharacterName = reader.ReadString();
				reader.ReadUnmanaged<int>(out value3);
				gameId = reader.ReadString();
				reader.ReadValue(ref value4);
				reader.ReadUnmanaged<SaveType>(out value5);
				reader.ReadUnmanaged<bool>(out value6);
				reader.ReadUnmanaged<int>(out value7);
				reader.ReadValue(ref value8);
				reader.ReadValue(ref value9);
				areaNameOverride = reader.ReadString();
				ListFormatter.DeserializePackable(ref reader, ref value10);
				reader.DangerousReadUnmanaged<DateTime?>(out value11);
				reader.ReadUnmanaged<DateTime>(out value12);
				reader.ReadUnmanaged<TimeSpan>(out value13);
				gameSaveTimeText = reader.ReadString();
				reader.ReadUnmanaged<TimeSpan>(out value14);
				reader.ReadValue(ref value15);
				reader.ReadUnmanaged<int>(out value16);
				ListFormatter.DeserializePackable(ref reader, ref value17);
				goto IL_04dc;
			}
			name = reader.ReadString();
			description = reader.ReadString();
			saveId = reader.ReadString();
			value2 = reader.ReadValue<BlueprintQuest>();
			playerCharacterName = reader.ReadString();
			reader.ReadUnmanaged<int>(out value3);
			gameId = reader.ReadString();
			value4 = reader.ReadValue<IEnumerable<BlueprintDlcReward>>();
			reader.ReadUnmanaged<SaveType, bool, int>(out value5, out value6, out value7);
			value8 = reader.ReadValue<BlueprintArea>();
			value9 = reader.ReadValue<BlueprintAreaPart>();
			areaNameOverride = reader.ReadString();
			value10 = ListFormatter.DeserializePackable<PortraitForSave>(ref reader);
			reader.DangerousReadUnmanaged<DateTime?, DateTime, TimeSpan>(out value11, out value12, out value13);
			gameSaveTimeText = reader.ReadString();
			reader.ReadUnmanaged<TimeSpan>(out value14);
			value15 = reader.ReadValue<List<int>>();
			reader.ReadUnmanaged<int>(out value16);
			value17 = ListFormatter.DeserializePackable<SerializableRandState>(ref reader);
		}
		else
		{
			if (memberCount > 23)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SaveInfo), 23, memberCount);
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
				value5 = SaveType.Manual;
				value6 = false;
				value7 = 0;
				value8 = null;
				value9 = null;
				areaNameOverride = null;
				value10 = null;
				value11 = null;
				value12 = default(DateTime);
				value13 = default(TimeSpan);
				gameSaveTimeText = null;
				value14 = default(TimeSpan);
				value15 = null;
				value16 = 0;
				value17 = null;
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
				value4 = value.DlcRewards;
				value5 = value.Type;
				value6 = value.IsAutoLevelupSave;
				value7 = value.QuickSaveNumber;
				value8 = value.Area;
				value9 = value.AreaPart;
				areaNameOverride = value.AreaNameOverride;
				value10 = value.PartyPortraits;
				value11 = value.GameStartSystemTime;
				value12 = value.SystemSaveTime;
				value13 = value.GameSaveTime;
				gameSaveTimeText = value.GameSaveTimeText;
				value14 = value.GameTotalTime;
				value15 = value.Versions;
				value16 = value.CompatibilityVersion;
				value17 = value.StatefulRandomStates;
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
											reader.ReadValue(ref value4);
											if (memberCount != 8)
											{
												reader.ReadUnmanaged<SaveType>(out value5);
												if (memberCount != 9)
												{
													reader.ReadUnmanaged<bool>(out value6);
													if (memberCount != 10)
													{
														reader.ReadUnmanaged<int>(out value7);
														if (memberCount != 11)
														{
															reader.ReadValue(ref value8);
															if (memberCount != 12)
															{
																reader.ReadValue(ref value9);
																if (memberCount != 13)
																{
																	areaNameOverride = reader.ReadString();
																	if (memberCount != 14)
																	{
																		ListFormatter.DeserializePackable(ref reader, ref value10);
																		if (memberCount != 15)
																		{
																			reader.DangerousReadUnmanaged<DateTime?>(out value11);
																			if (memberCount != 16)
																			{
																				reader.ReadUnmanaged<DateTime>(out value12);
																				if (memberCount != 17)
																				{
																					reader.ReadUnmanaged<TimeSpan>(out value13);
																					if (memberCount != 18)
																					{
																						gameSaveTimeText = reader.ReadString();
																						if (memberCount != 19)
																						{
																							reader.ReadUnmanaged<TimeSpan>(out value14);
																							if (memberCount != 20)
																							{
																								reader.ReadValue(ref value15);
																								if (memberCount != 21)
																								{
																									reader.ReadUnmanaged<int>(out value16);
																									if (memberCount != 22)
																									{
																										ListFormatter.DeserializePackable(ref reader, ref value17);
																										_ = 23;
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
				goto IL_04dc;
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
			DlcRewards = value4,
			Type = value5,
			IsAutoLevelupSave = value6,
			QuickSaveNumber = value7,
			Area = value8,
			AreaPart = value9,
			AreaNameOverride = areaNameOverride,
			PartyPortraits = value10,
			GameStartSystemTime = value11,
			SystemSaveTime = value12,
			GameSaveTime = value13,
			GameSaveTimeText = gameSaveTimeText,
			GameTotalTime = value14,
			Versions = value15,
			CompatibilityVersion = value16,
			StatefulRandomStates = value17
		};
		return;
		IL_04dc:
		value.Name = name;
		value.Description = description;
		value.SaveId = saveId;
		value.QuestWithSaveDescription = value2;
		value.PlayerCharacterName = playerCharacterName;
		value.PlayerCharacterRank = value3;
		value.GameId = gameId;
		value.DlcRewards = value4;
		value.Type = value5;
		value.IsAutoLevelupSave = value6;
		value.QuickSaveNumber = value7;
		value.Area = value8;
		value.AreaPart = value9;
		value.AreaNameOverride = areaNameOverride;
		value.PartyPortraits = value10;
		value.GameStartSystemTime = value11;
		value.SystemSaveTime = value12;
		value.GameSaveTime = value13;
		value.GameSaveTimeText = gameSaveTimeText;
		value.GameTotalTime = value14;
		value.Versions = value15;
		value.CompatibilityVersion = value16;
		value.StatefulRandomStates = value17;
	}
}
