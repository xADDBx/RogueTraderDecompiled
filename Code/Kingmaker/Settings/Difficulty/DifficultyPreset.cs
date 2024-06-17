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
		int num2 = CombatEncountersCapacity.CompareTo(other.CombatEncountersCapacity);
		if (num2 < 0)
		{
			return -1;
		}
		int num3 = -RespecAllowed.CompareTo(other.RespecAllowed);
		if (num3 < 0)
		{
			return -1;
		}
		int num4 = -AutoLevelUp.CompareTo(other.AutoLevelUp);
		if (num4 < 0)
		{
			return -1;
		}
		int num5 = EnemyDodgePercentModifier.CompareTo(other.EnemyDodgePercentModifier);
		if (num5 < 0)
		{
			return -1;
		}
		int num6 = CoverHitBonusHalfModifier.CompareTo(other.CoverHitBonusHalfModifier);
		if (num6 < 0)
		{
			return -1;
		}
		int num7 = CoverHitBonusFullModifier.CompareTo(other.CoverHitBonusFullModifier);
		if (num7 < 0)
		{
			return -1;
		}
		int num8 = -MinPartyDamage.CompareTo(other.MinPartyDamage);
		if (num8 < 0)
		{
			return -1;
		}
		int num9 = -MinPartyDamageFraction.CompareTo(other.MinPartyDamageFraction);
		if (num9 < 0)
		{
			return -1;
		}
		int num10 = -MinPartyStarshipDamage.CompareTo(other.MinPartyStarshipDamage);
		if (num10 < 0)
		{
			return -1;
		}
		int num11 = -MinPartyStarshipDamageFraction.CompareTo(other.MinPartyStarshipDamageFraction);
		if (num11 < 0)
		{
			return -1;
		}
		int num12 = PartyMomentumPercentModifier.CompareTo(other.PartyMomentumPercentModifier);
		if (num12 < 0)
		{
			return -1;
		}
		int num13 = NPCAttributesBaseValuePercentModifier.CompareTo(other.NPCAttributesBaseValuePercentModifier);
		if (num13 < 0)
		{
			return -1;
		}
		int num14 = HardCrowdControlOnPartyMaxDurationRounds.CompareTo(other.HardCrowdControlOnPartyMaxDurationRounds);
		if (num14 < 0)
		{
			return -1;
		}
		int num15 = SkillCheckModifier.CompareTo(other.SkillCheckModifier);
		if (num15 < 0)
		{
			return -1;
		}
		int num16 = -EnemyHitPointsPercentModifier.CompareTo(other.EnemyHitPointsPercentModifier);
		if (num16 < 0)
		{
			return -1;
		}
		int num17 = AllyResolveModifier.CompareTo(other.AllyResolveModifier);
		if (num17 < 0)
		{
			return -1;
		}
		int num18 = -PartyDamageDealtAfterArmorReductionPercentModifier.CompareTo(other.PartyDamageDealtAfterArmorReductionPercentModifier);
		if (num18 < 0)
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
		int num19 = SpaceCombatDifficulty.CompareTo(other.SpaceCombatDifficulty);
		if (num19 < 0)
		{
			return -1;
		}
		if (num2 <= 0 && num3 <= 0 && num4 <= 0 && num5 <= 0 && num6 <= 0 && num7 <= 0 && num8 <= 0 && num9 <= 0 && num10 <= 0 && num11 <= 0 && num12 <= 0 && num13 <= 0 && num14 <= 0 && num15 <= 0 && num16 <= 0 && num17 <= 0 && num18 <= 0 && num19 <= 0)
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
		writer.WriteUnmanagedWithObjectHeader(23, in value.GameDifficulty, in value.RespecAllowed, in value.AutoLevelUp, in value.AdditionalAIBehaviors, in value.CombatEncountersCapacity, in value.EnemyDodgePercentModifier, in value.CoverHitBonusHalfModifier, in value.CoverHitBonusFullModifier, in value.MinPartyDamage, in value.MinPartyDamageFraction, in value.MinPartyStarshipDamage, in value.MinPartyStarshipDamageFraction, in value.PartyMomentumPercentModifier, in value.NPCAttributesBaseValuePercentModifier, in value.HardCrowdControlOnPartyMaxDurationRounds);
		writer.WriteUnmanaged(in value.SkillCheckModifier, in value.EnemyHitPointsPercentModifier, in value.AllyResolveModifier, in value.PartyDamageDealtAfterArmorReductionPercentModifier, in value.WoundDamagePerTurnThresholdHPFraction, in value.OldWoundDelayRounds, in value.WoundStacksForTrauma, in value.SpaceCombatDifficulty);
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
		AutoLevelUpOption value4;
		bool value5;
		CombatEncountersCapacity value6;
		int value7;
		int value8;
		int value9;
		int value10;
		int value11;
		int value12;
		int value13;
		int value14;
		int value15;
		HardCrowdControlDurationLimit value16;
		int value17;
		int value18;
		int value19;
		int value20;
		int value21;
		int value22;
		int value23;
		SpaceCombatDifficulty value24;
		if (memberCount == 23)
		{
			if (value != null)
			{
				value2 = value.GameDifficulty;
				value3 = value.RespecAllowed;
				value4 = value.AutoLevelUp;
				value5 = value.AdditionalAIBehaviors;
				value6 = value.CombatEncountersCapacity;
				value7 = value.EnemyDodgePercentModifier;
				value8 = value.CoverHitBonusHalfModifier;
				value9 = value.CoverHitBonusFullModifier;
				value10 = value.MinPartyDamage;
				value11 = value.MinPartyDamageFraction;
				value12 = value.MinPartyStarshipDamage;
				value13 = value.MinPartyStarshipDamageFraction;
				value14 = value.PartyMomentumPercentModifier;
				value15 = value.NPCAttributesBaseValuePercentModifier;
				value16 = value.HardCrowdControlOnPartyMaxDurationRounds;
				value17 = value.SkillCheckModifier;
				value18 = value.EnemyHitPointsPercentModifier;
				value19 = value.AllyResolveModifier;
				value20 = value.PartyDamageDealtAfterArmorReductionPercentModifier;
				value21 = value.WoundDamagePerTurnThresholdHPFraction;
				value22 = value.OldWoundDelayRounds;
				value23 = value.WoundStacksForTrauma;
				value24 = value.SpaceCombatDifficulty;
				reader.ReadUnmanaged<GameDifficultyOption>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
				reader.ReadUnmanaged<AutoLevelUpOption>(out value4);
				reader.ReadUnmanaged<bool>(out value5);
				reader.ReadUnmanaged<CombatEncountersCapacity>(out value6);
				reader.ReadUnmanaged<int>(out value7);
				reader.ReadUnmanaged<int>(out value8);
				reader.ReadUnmanaged<int>(out value9);
				reader.ReadUnmanaged<int>(out value10);
				reader.ReadUnmanaged<int>(out value11);
				reader.ReadUnmanaged<int>(out value12);
				reader.ReadUnmanaged<int>(out value13);
				reader.ReadUnmanaged<int>(out value14);
				reader.ReadUnmanaged<int>(out value15);
				reader.ReadUnmanaged<HardCrowdControlDurationLimit>(out value16);
				reader.ReadUnmanaged<int>(out value17);
				reader.ReadUnmanaged<int>(out value18);
				reader.ReadUnmanaged<int>(out value19);
				reader.ReadUnmanaged<int>(out value20);
				reader.ReadUnmanaged<int>(out value21);
				reader.ReadUnmanaged<int>(out value22);
				reader.ReadUnmanaged<int>(out value23);
				reader.ReadUnmanaged<SpaceCombatDifficulty>(out value24);
				goto IL_0468;
			}
			reader.ReadUnmanaged<GameDifficultyOption, bool, AutoLevelUpOption, bool, CombatEncountersCapacity, int, int, int, int, int, int, int, int, int, HardCrowdControlDurationLimit>(out value2, out value3, out value4, out value5, out value6, out value7, out value8, out value9, out value10, out value11, out value12, out value13, out value14, out value15, out value16);
			reader.ReadUnmanaged<int, int, int, int, int, int, int, SpaceCombatDifficulty>(out value17, out value18, out value19, out value20, out value21, out value22, out value23, out value24);
		}
		else
		{
			if (memberCount > 23)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DifficultyPreset), 23, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = GameDifficultyOption.Story;
				value3 = false;
				value4 = AutoLevelUpOption.Off;
				value5 = false;
				value6 = CombatEncountersCapacity.Standard;
				value7 = 0;
				value8 = 0;
				value9 = 0;
				value10 = 0;
				value11 = 0;
				value12 = 0;
				value13 = 0;
				value14 = 0;
				value15 = 0;
				value16 = (HardCrowdControlDurationLimit)0;
				value17 = 0;
				value18 = 0;
				value19 = 0;
				value20 = 0;
				value21 = 0;
				value22 = 0;
				value23 = 0;
				value24 = SpaceCombatDifficulty.Easy;
			}
			else
			{
				value2 = value.GameDifficulty;
				value3 = value.RespecAllowed;
				value4 = value.AutoLevelUp;
				value5 = value.AdditionalAIBehaviors;
				value6 = value.CombatEncountersCapacity;
				value7 = value.EnemyDodgePercentModifier;
				value8 = value.CoverHitBonusHalfModifier;
				value9 = value.CoverHitBonusFullModifier;
				value10 = value.MinPartyDamage;
				value11 = value.MinPartyDamageFraction;
				value12 = value.MinPartyStarshipDamage;
				value13 = value.MinPartyStarshipDamageFraction;
				value14 = value.PartyMomentumPercentModifier;
				value15 = value.NPCAttributesBaseValuePercentModifier;
				value16 = value.HardCrowdControlOnPartyMaxDurationRounds;
				value17 = value.SkillCheckModifier;
				value18 = value.EnemyHitPointsPercentModifier;
				value19 = value.AllyResolveModifier;
				value20 = value.PartyDamageDealtAfterArmorReductionPercentModifier;
				value21 = value.WoundDamagePerTurnThresholdHPFraction;
				value22 = value.OldWoundDelayRounds;
				value23 = value.WoundStacksForTrauma;
				value24 = value.SpaceCombatDifficulty;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<GameDifficultyOption>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<AutoLevelUpOption>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<bool>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<CombatEncountersCapacity>(out value6);
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<int>(out value7);
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
																		reader.ReadUnmanaged<HardCrowdControlDurationLimit>(out value16);
																		if (memberCount != 15)
																		{
																			reader.ReadUnmanaged<int>(out value17);
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
																										reader.ReadUnmanaged<SpaceCombatDifficulty>(out value24);
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
				goto IL_0468;
			}
		}
		value = new DifficultyPreset
		{
			GameDifficulty = value2,
			RespecAllowed = value3,
			AutoLevelUp = value4,
			AdditionalAIBehaviors = value5,
			CombatEncountersCapacity = value6,
			EnemyDodgePercentModifier = value7,
			CoverHitBonusHalfModifier = value8,
			CoverHitBonusFullModifier = value9,
			MinPartyDamage = value10,
			MinPartyDamageFraction = value11,
			MinPartyStarshipDamage = value12,
			MinPartyStarshipDamageFraction = value13,
			PartyMomentumPercentModifier = value14,
			NPCAttributesBaseValuePercentModifier = value15,
			HardCrowdControlOnPartyMaxDurationRounds = value16,
			SkillCheckModifier = value17,
			EnemyHitPointsPercentModifier = value18,
			AllyResolveModifier = value19,
			PartyDamageDealtAfterArmorReductionPercentModifier = value20,
			WoundDamagePerTurnThresholdHPFraction = value21,
			OldWoundDelayRounds = value22,
			WoundStacksForTrauma = value23,
			SpaceCombatDifficulty = value24
		};
		return;
		IL_0468:
		value.GameDifficulty = value2;
		value.RespecAllowed = value3;
		value.AutoLevelUp = value4;
		value.AdditionalAIBehaviors = value5;
		value.CombatEncountersCapacity = value6;
		value.EnemyDodgePercentModifier = value7;
		value.CoverHitBonusHalfModifier = value8;
		value.CoverHitBonusFullModifier = value9;
		value.MinPartyDamage = value10;
		value.MinPartyDamageFraction = value11;
		value.MinPartyStarshipDamage = value12;
		value.MinPartyStarshipDamageFraction = value13;
		value.PartyMomentumPercentModifier = value14;
		value.NPCAttributesBaseValuePercentModifier = value15;
		value.HardCrowdControlOnPartyMaxDurationRounds = value16;
		value.SkillCheckModifier = value17;
		value.EnemyHitPointsPercentModifier = value18;
		value.AllyResolveModifier = value19;
		value.PartyDamageDealtAfterArmorReductionPercentModifier = value20;
		value.WoundDamagePerTurnThresholdHPFraction = value21;
		value.OldWoundDelayRounds = value22;
		value.WoundStacksForTrauma = value23;
		value.SpaceCombatDifficulty = value24;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref GameDifficulty);
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
