using System;
using Kingmaker.Enums;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Settings.Difficulty;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class DifficultyPreset : IComparable<DifficultyPreset>, IMemoryPackable<DifficultyPreset>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class DifficultyPresetFormatter : MemoryPackFormatter<DifficultyPreset>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DifficultyPreset value)
		{
			DifficultyPreset.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DifficultyPreset value)
		{
			DifficultyPreset.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public GameDifficultyOption GameDifficulty;

	[JsonProperty]
	public bool OnlyOneSave;

	[JsonProperty]
	public bool RespecAllowed;

	[JsonProperty]
	public AutoLevelUpOption AutoLevelUp;

	[JsonProperty]
	public bool AdditionalAIBehaviors;

	[JsonProperty]
	public CombatEncountersCapacity CombatEncountersCapacity;

	[JsonProperty]
	public int EnemyDodgePercentModifier;

	[JsonProperty]
	public int CoverHitBonusHalfModifier;

	[JsonProperty]
	public int CoverHitBonusFullModifier;

	[JsonProperty]
	public int MinPartyDamage;

	[JsonProperty]
	public int MinPartyDamageFraction;

	[JsonProperty]
	public int MinPartyStarshipDamage;

	[JsonProperty]
	public int MinPartyStarshipDamageFraction;

	[JsonProperty]
	public int PartyMomentumPercentModifier;

	[JsonProperty]
	public int NPCAttributesBaseValuePercentModifier;

	[JsonProperty]
	public HardCrowdControlDurationLimit HardCrowdControlOnPartyMaxDurationRounds = HardCrowdControlDurationLimit.Unlimited;

	[JsonProperty]
	public int SkillCheckModifier;

	[JsonProperty]
	public int EnemyHitPointsPercentModifier;

	[JsonProperty]
	public int AllyResolveModifier;

	[JsonProperty]
	public int PartyDamageDealtAfterArmorReductionPercentModifier;

	[JsonProperty]
	public int WoundDamagePerTurnThresholdHPFraction = 50;

	[JsonProperty]
	public int OldWoundDelayRounds = 5;

	[JsonProperty]
	public int WoundStacksForTrauma = 5;

	[JsonProperty]
	public SpaceCombatDifficulty SpaceCombatDifficulty;

	[MemoryPackConstructor]
	public DifficultyPreset()
	{
	}

	public DifficultyPreset Copy()
	{
		return new DifficultyPreset
		{
			GameDifficulty = GameDifficulty,
			OnlyOneSave = OnlyOneSave,
			AutoLevelUp = AutoLevelUp,
			RespecAllowed = RespecAllowed,
			CombatEncountersCapacity = CombatEncountersCapacity,
			AdditionalAIBehaviors = AdditionalAIBehaviors,
			EnemyDodgePercentModifier = EnemyDodgePercentModifier,
			CoverHitBonusHalfModifier = CoverHitBonusHalfModifier,
			CoverHitBonusFullModifier = CoverHitBonusFullModifier,
			MinPartyDamage = MinPartyDamage,
			MinPartyDamageFraction = MinPartyDamageFraction,
			MinPartyStarshipDamage = MinPartyStarshipDamage,
			MinPartyStarshipDamageFraction = MinPartyStarshipDamageFraction,
			PartyMomentumPercentModifier = PartyMomentumPercentModifier,
			NPCAttributesBaseValuePercentModifier = NPCAttributesBaseValuePercentModifier,
			HardCrowdControlOnPartyMaxDurationRounds = HardCrowdControlOnPartyMaxDurationRounds,
			SkillCheckModifier = SkillCheckModifier,
			EnemyHitPointsPercentModifier = EnemyHitPointsPercentModifier,
			AllyResolveModifier = AllyResolveModifier,
			PartyDamageDealtAfterArmorReductionPercentModifier = PartyDamageDealtAfterArmorReductionPercentModifier,
			WoundDamagePerTurnThresholdHPFraction = WoundDamagePerTurnThresholdHPFraction,
			OldWoundDelayRounds = OldWoundDelayRounds,
			WoundStacksForTrauma = WoundStacksForTrauma,
			SpaceCombatDifficulty = SpaceCombatDifficulty
		};
	}

	public int CompareTo(DifficultyPreset other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		if (other.GameDifficulty != GameDifficultyOption.Custom && GameDifficulty != GameDifficultyOption.Custom)
		{
			int num = GameDifficulty.CompareTo(other.GameDifficulty);
			if (num != 0)
			{
				return num;
			}
		}
		int num2 = -OnlyOneSave.CompareTo(other.OnlyOneSave);
		if (num2 < 0)
		{
			return -1;
		}
		int num3 = CombatEncountersCapacity.CompareTo(other.CombatEncountersCapacity);
		if (num3 < 0)
		{
			return -1;
		}
		int num4 = -RespecAllowed.CompareTo(other.RespecAllowed);
		if (num4 < 0)
		{
			return -1;
		}
		int num5 = -AutoLevelUp.CompareTo(other.AutoLevelUp);
		if (num5 < 0)
		{
			return -1;
		}
		int num6 = EnemyDodgePercentModifier.CompareTo(other.EnemyDodgePercentModifier);
		if (num6 < 0)
		{
			return -1;
		}
		int num7 = CoverHitBonusHalfModifier.CompareTo(other.CoverHitBonusHalfModifier);
		if (num7 < 0)
		{
			return -1;
		}
		int num8 = CoverHitBonusFullModifier.CompareTo(other.CoverHitBonusFullModifier);
		if (num8 < 0)
		{
			return -1;
		}
		int num9 = -MinPartyDamage.CompareTo(other.MinPartyDamage);
		if (num9 < 0)
		{
			return -1;
		}
		int num10 = -MinPartyDamageFraction.CompareTo(other.MinPartyDamageFraction);
		if (num10 < 0)
		{
			return -1;
		}
		int num11 = -MinPartyStarshipDamage.CompareTo(other.MinPartyStarshipDamage);
		if (num11 < 0)
		{
			return -1;
		}
		int num12 = -MinPartyStarshipDamageFraction.CompareTo(other.MinPartyStarshipDamageFraction);
		if (num12 < 0)
		{
			return -1;
		}
		int num13 = PartyMomentumPercentModifier.CompareTo(other.PartyMomentumPercentModifier);
		if (num13 < 0)
		{
			return -1;
		}
		int num14 = NPCAttributesBaseValuePercentModifier.CompareTo(other.NPCAttributesBaseValuePercentModifier);
		if (num14 < 0)
		{
			return -1;
		}
		int num15 = HardCrowdControlOnPartyMaxDurationRounds.CompareTo(other.HardCrowdControlOnPartyMaxDurationRounds);
		if (num15 < 0)
		{
			return -1;
		}
		int num16 = SkillCheckModifier.CompareTo(other.SkillCheckModifier);
		if (num16 < 0)
		{
			return -1;
		}
		int num17 = -EnemyHitPointsPercentModifier.CompareTo(other.EnemyHitPointsPercentModifier);
		if (num17 < 0)
		{
			return -1;
		}
		int num18 = AllyResolveModifier.CompareTo(other.AllyResolveModifier);
		if (num18 < 0)
		{
			return -1;
		}
		int num19 = -PartyDamageDealtAfterArmorReductionPercentModifier.CompareTo(other.PartyDamageDealtAfterArmorReductionPercentModifier);
		if (num19 < 0)
		{
			return -1;
		}
		if (-WoundDamagePerTurnThresholdHPFraction.CompareTo(other.WoundDamagePerTurnThresholdHPFraction) < 0)
		{
			return -1;
		}
		if (-OldWoundDelayRounds.CompareTo(other.OldWoundDelayRounds) < 0)
		{
			return -1;
		}
		if (-WoundStacksForTrauma.CompareTo(other.WoundStacksForTrauma) < 0)
		{
			return -1;
		}
		int num20 = SpaceCombatDifficulty.CompareTo(other.SpaceCombatDifficulty);
		if (num20 < 0)
		{
			return -1;
		}
		if (num2 <= 0 && num3 <= 0 && num4 <= 0 && num5 <= 0 && num6 <= 0 && num7 <= 0 && num8 <= 0 && num9 <= 0 && num10 <= 0 && num11 <= 0 && num12 <= 0 && num13 <= 0 && num14 <= 0 && num15 <= 0 && num16 <= 0 && num17 <= 0 && num18 <= 0 && num19 <= 0 && num20 <= 0)
		{
			return 0;
		}
		return 1;
	}

	static DifficultyPreset()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DifficultyPreset>())
		{
			MemoryPackFormatterProvider.Register(new DifficultyPresetFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DifficultyPreset[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DifficultyPreset>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameDifficultyOption>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<GameDifficultyOption>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AutoLevelUpOption>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<AutoLevelUpOption>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CombatEncountersCapacity>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CombatEncountersCapacity>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<HardCrowdControlDurationLimit>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<HardCrowdControlDurationLimit>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SpaceCombatDifficulty>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<SpaceCombatDifficulty>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DifficultyPreset? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(24, in value.GameDifficulty, in value.OnlyOneSave, in value.RespecAllowed, in value.AutoLevelUp, in value.AdditionalAIBehaviors, in value.CombatEncountersCapacity, in value.EnemyDodgePercentModifier, in value.CoverHitBonusHalfModifier, in value.CoverHitBonusFullModifier, in value.MinPartyDamage, in value.MinPartyDamageFraction, in value.MinPartyStarshipDamage, in value.MinPartyStarshipDamageFraction, in value.PartyMomentumPercentModifier, in value.NPCAttributesBaseValuePercentModifier);
		writer.WriteUnmanaged(in value.HardCrowdControlOnPartyMaxDurationRounds, in value.SkillCheckModifier, in value.EnemyHitPointsPercentModifier, in value.AllyResolveModifier, in value.PartyDamageDealtAfterArmorReductionPercentModifier, in value.WoundDamagePerTurnThresholdHPFraction, in value.OldWoundDelayRounds, in value.WoundStacksForTrauma, in value.SpaceCombatDifficulty);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DifficultyPreset? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		GameDifficultyOption value2;
		bool value3;
		bool value4;
		AutoLevelUpOption value5;
		bool value6;
		CombatEncountersCapacity value7;
		int value8;
		int value9;
		int value10;
		int value11;
		int value12;
		int value13;
		int value14;
		int value15;
		int value16;
		HardCrowdControlDurationLimit value17;
		int value18;
		int value19;
		int value20;
		int value21;
		int value22;
		int value23;
		int value24;
		SpaceCombatDifficulty value25;
		if (memberCount == 24)
		{
			if (value != null)
			{
				value2 = value.GameDifficulty;
				value3 = value.OnlyOneSave;
				value4 = value.RespecAllowed;
				value5 = value.AutoLevelUp;
				value6 = value.AdditionalAIBehaviors;
				value7 = value.CombatEncountersCapacity;
				value8 = value.EnemyDodgePercentModifier;
				value9 = value.CoverHitBonusHalfModifier;
				value10 = value.CoverHitBonusFullModifier;
				value11 = value.MinPartyDamage;
				value12 = value.MinPartyDamageFraction;
				value13 = value.MinPartyStarshipDamage;
				value14 = value.MinPartyStarshipDamageFraction;
				value15 = value.PartyMomentumPercentModifier;
				value16 = value.NPCAttributesBaseValuePercentModifier;
				value17 = value.HardCrowdControlOnPartyMaxDurationRounds;
				value18 = value.SkillCheckModifier;
				value19 = value.EnemyHitPointsPercentModifier;
				value20 = value.AllyResolveModifier;
				value21 = value.PartyDamageDealtAfterArmorReductionPercentModifier;
				value22 = value.WoundDamagePerTurnThresholdHPFraction;
				value23 = value.OldWoundDelayRounds;
				value24 = value.WoundStacksForTrauma;
				value25 = value.SpaceCombatDifficulty;
				reader.ReadUnmanaged<GameDifficultyOption>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
				reader.ReadUnmanaged<bool>(out value4);
				reader.ReadUnmanaged<AutoLevelUpOption>(out value5);
				reader.ReadUnmanaged<bool>(out value6);
				reader.ReadUnmanaged<CombatEncountersCapacity>(out value7);
				reader.ReadUnmanaged<int>(out value8);
				reader.ReadUnmanaged<int>(out value9);
				reader.ReadUnmanaged<int>(out value10);
				reader.ReadUnmanaged<int>(out value11);
				reader.ReadUnmanaged<int>(out value12);
				reader.ReadUnmanaged<int>(out value13);
				reader.ReadUnmanaged<int>(out value14);
				reader.ReadUnmanaged<int>(out value15);
				reader.ReadUnmanaged<int>(out value16);
				reader.ReadUnmanaged<HardCrowdControlDurationLimit>(out value17);
				reader.ReadUnmanaged<int>(out value18);
				reader.ReadUnmanaged<int>(out value19);
				reader.ReadUnmanaged<int>(out value20);
				reader.ReadUnmanaged<int>(out value21);
				reader.ReadUnmanaged<int>(out value22);
				reader.ReadUnmanaged<int>(out value23);
				reader.ReadUnmanaged<int>(out value24);
				reader.ReadUnmanaged<SpaceCombatDifficulty>(out value25);
				goto IL_0497;
			}
			reader.ReadUnmanaged<GameDifficultyOption, bool, bool, AutoLevelUpOption, bool, CombatEncountersCapacity, int, int, int, int, int, int, int, int, int>(out value2, out value3, out value4, out value5, out value6, out value7, out value8, out value9, out value10, out value11, out value12, out value13, out value14, out value15, out value16);
			reader.ReadUnmanaged<HardCrowdControlDurationLimit, int, int, int, int, int, int, int, SpaceCombatDifficulty>(out value17, out value18, out value19, out value20, out value21, out value22, out value23, out value24, out value25);
		}
		else
		{
			if (memberCount > 24)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DifficultyPreset), 24, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = GameDifficultyOption.Story;
				value3 = false;
				value4 = false;
				value5 = AutoLevelUpOption.Off;
				value6 = false;
				value7 = CombatEncountersCapacity.Standard;
				value8 = 0;
				value9 = 0;
				value10 = 0;
				value11 = 0;
				value12 = 0;
				value13 = 0;
				value14 = 0;
				value15 = 0;
				value16 = 0;
				value17 = (HardCrowdControlDurationLimit)0;
				value18 = 0;
				value19 = 0;
				value20 = 0;
				value21 = 0;
				value22 = 0;
				value23 = 0;
				value24 = 0;
				value25 = SpaceCombatDifficulty.Easy;
			}
			else
			{
				value2 = value.GameDifficulty;
				value3 = value.OnlyOneSave;
				value4 = value.RespecAllowed;
				value5 = value.AutoLevelUp;
				value6 = value.AdditionalAIBehaviors;
				value7 = value.CombatEncountersCapacity;
				value8 = value.EnemyDodgePercentModifier;
				value9 = value.CoverHitBonusHalfModifier;
				value10 = value.CoverHitBonusFullModifier;
				value11 = value.MinPartyDamage;
				value12 = value.MinPartyDamageFraction;
				value13 = value.MinPartyStarshipDamage;
				value14 = value.MinPartyStarshipDamageFraction;
				value15 = value.PartyMomentumPercentModifier;
				value16 = value.NPCAttributesBaseValuePercentModifier;
				value17 = value.HardCrowdControlOnPartyMaxDurationRounds;
				value18 = value.SkillCheckModifier;
				value19 = value.EnemyHitPointsPercentModifier;
				value20 = value.AllyResolveModifier;
				value21 = value.PartyDamageDealtAfterArmorReductionPercentModifier;
				value22 = value.WoundDamagePerTurnThresholdHPFraction;
				value23 = value.OldWoundDelayRounds;
				value24 = value.WoundStacksForTrauma;
				value25 = value.SpaceCombatDifficulty;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<GameDifficultyOption>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<AutoLevelUpOption>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<bool>(out value6);
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<CombatEncountersCapacity>(out value7);
									if (memberCount != 6)
									{
										reader.ReadUnmanaged<int>(out value8);
										if (memberCount != 7)
										{
											reader.ReadUnmanaged<int>(out value9);
											if (memberCount != 8)
											{
												reader.ReadUnmanaged<int>(out value10);
												if (memberCount != 9)
												{
													reader.ReadUnmanaged<int>(out value11);
													if (memberCount != 10)
													{
														reader.ReadUnmanaged<int>(out value12);
														if (memberCount != 11)
														{
															reader.ReadUnmanaged<int>(out value13);
															if (memberCount != 12)
															{
																reader.ReadUnmanaged<int>(out value14);
																if (memberCount != 13)
																{
																	reader.ReadUnmanaged<int>(out value15);
																	if (memberCount != 14)
																	{
																		reader.ReadUnmanaged<int>(out value16);
																		if (memberCount != 15)
																		{
																			reader.ReadUnmanaged<HardCrowdControlDurationLimit>(out value17);
																			if (memberCount != 16)
																			{
																				reader.ReadUnmanaged<int>(out value18);
																				if (memberCount != 17)
																				{
																					reader.ReadUnmanaged<int>(out value19);
																					if (memberCount != 18)
																					{
																						reader.ReadUnmanaged<int>(out value20);
																						if (memberCount != 19)
																						{
																							reader.ReadUnmanaged<int>(out value21);
																							if (memberCount != 20)
																							{
																								reader.ReadUnmanaged<int>(out value22);
																								if (memberCount != 21)
																								{
																									reader.ReadUnmanaged<int>(out value23);
																									if (memberCount != 22)
																									{
																										reader.ReadUnmanaged<int>(out value24);
																										if (memberCount != 23)
																										{
																											reader.ReadUnmanaged<SpaceCombatDifficulty>(out value25);
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
				goto IL_0497;
			}
		}
		value = new DifficultyPreset
		{
			GameDifficulty = value2,
			OnlyOneSave = value3,
			RespecAllowed = value4,
			AutoLevelUp = value5,
			AdditionalAIBehaviors = value6,
			CombatEncountersCapacity = value7,
			EnemyDodgePercentModifier = value8,
			CoverHitBonusHalfModifier = value9,
			CoverHitBonusFullModifier = value10,
			MinPartyDamage = value11,
			MinPartyDamageFraction = value12,
			MinPartyStarshipDamage = value13,
			MinPartyStarshipDamageFraction = value14,
			PartyMomentumPercentModifier = value15,
			NPCAttributesBaseValuePercentModifier = value16,
			HardCrowdControlOnPartyMaxDurationRounds = value17,
			SkillCheckModifier = value18,
			EnemyHitPointsPercentModifier = value19,
			AllyResolveModifier = value20,
			PartyDamageDealtAfterArmorReductionPercentModifier = value21,
			WoundDamagePerTurnThresholdHPFraction = value22,
			OldWoundDelayRounds = value23,
			WoundStacksForTrauma = value24,
			SpaceCombatDifficulty = value25
		};
		return;
		IL_0497:
		value.GameDifficulty = value2;
		value.OnlyOneSave = value3;
		value.RespecAllowed = value4;
		value.AutoLevelUp = value5;
		value.AdditionalAIBehaviors = value6;
		value.CombatEncountersCapacity = value7;
		value.EnemyDodgePercentModifier = value8;
		value.CoverHitBonusHalfModifier = value9;
		value.CoverHitBonusFullModifier = value10;
		value.MinPartyDamage = value11;
		value.MinPartyDamageFraction = value12;
		value.MinPartyStarshipDamage = value13;
		value.MinPartyStarshipDamageFraction = value14;
		value.PartyMomentumPercentModifier = value15;
		value.NPCAttributesBaseValuePercentModifier = value16;
		value.HardCrowdControlOnPartyMaxDurationRounds = value17;
		value.SkillCheckModifier = value18;
		value.EnemyHitPointsPercentModifier = value19;
		value.AllyResolveModifier = value20;
		value.PartyDamageDealtAfterArmorReductionPercentModifier = value21;
		value.WoundDamagePerTurnThresholdHPFraction = value22;
		value.OldWoundDelayRounds = value23;
		value.WoundStacksForTrauma = value24;
		value.SpaceCombatDifficulty = value25;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref GameDifficulty);
		result.Append(ref OnlyOneSave);
		result.Append(ref RespecAllowed);
		result.Append(ref AutoLevelUp);
		result.Append(ref AdditionalAIBehaviors);
		result.Append(ref CombatEncountersCapacity);
		result.Append(ref EnemyDodgePercentModifier);
		result.Append(ref CoverHitBonusHalfModifier);
		result.Append(ref CoverHitBonusFullModifier);
		result.Append(ref MinPartyDamage);
		result.Append(ref MinPartyDamageFraction);
		result.Append(ref MinPartyStarshipDamage);
		result.Append(ref MinPartyStarshipDamageFraction);
		result.Append(ref PartyMomentumPercentModifier);
		result.Append(ref NPCAttributesBaseValuePercentModifier);
		result.Append(ref HardCrowdControlOnPartyMaxDurationRounds);
		result.Append(ref SkillCheckModifier);
		result.Append(ref EnemyHitPointsPercentModifier);
		result.Append(ref AllyResolveModifier);
		result.Append(ref PartyDamageDealtAfterArmorReductionPercentModifier);
		result.Append(ref WoundDamagePerTurnThresholdHPFraction);
		result.Append(ref OldWoundDelayRounds);
		result.Append(ref WoundStacksForTrauma);
		result.Append(ref SpaceCombatDifficulty);
		return result;
	}
}
