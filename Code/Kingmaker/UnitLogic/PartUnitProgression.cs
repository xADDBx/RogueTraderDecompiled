using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Levelup.Components;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.UnitLogic;

public class PartUnitProgression : BaseUnitPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitProgression>, IEntityPartOwner
	{
		PartUnitProgression Progression { get; }
	}

	[CanBeNull]
	[JsonProperty]
	private BlueprintRace m_Race;

	[NotNull]
	[JsonProperty]
	private readonly List<FeatureSelectionData> m_Selections = new List<FeatureSelectionData>();

	[JsonProperty]
	public int CharacterLevel { get; private set; }

	[JsonProperty]
	public RespecInfo RespecInfo { get; private set; }

	[JsonProperty]
	public int Experience { get; private set; }

	public FeatureCollection Features { get; private set; }

	private BlueprintStatProgression ExperienceTable
	{
		get
		{
			if (!(base.Owner is StarshipEntity))
			{
				return Game.Instance.BlueprintRoot.Progression.XPTable;
			}
			return base.Owner.GetStarshipProgressionOptional()?.ExperienceTable;
		}
	}

	public BlueprintRace Race => m_Race;

	public bool CanLevelUp => CharacterLevel < ExperienceLevel;

	public int ExperienceLevel => ExperienceTable.GetLevel(Experience);

	public IEnumerable<(BlueprintCareerPath Blueprint, int Rank)> AllCareerPaths
	{
		get
		{
			foreach (Feature rawFact in Features.RawFacts)
			{
				if (rawFact.Blueprint is BlueprintCareerPath item)
				{
					yield return (Blueprint: item, Rank: rawFact.Rank);
				}
			}
		}
	}

	[Obsolete]
	public int MythicLevel => 0;

	[Obsolete]
	public List<ClassData> Classes => TempList.Get<ClassData>();

	protected override void OnAttach()
	{
		Features = base.Owner.Facts.EnsureFactProcessor<FeatureCollection>();
		if (!base.Owner.IsStarship())
		{
			if (BlueprintRoot.Instance.Progression.FeatsProgression != null)
			{
				Features.Add(BlueprintRoot.Instance.Progression.FeatsProgression);
			}
			SpawningData current = ContextData<SpawningData>.Current;
			if (current != null)
			{
				SetRace(current.Race);
			}
			else if (base.Owner.Blueprint.Race != null)
			{
				SetRace(base.Owner.Blueprint.Race);
			}
		}
	}

	public void AddPathRank(BlueprintPath path)
	{
		if (path is BlueprintCareerPath)
		{
			CharacterLevel++;
		}
		Features.Add(path);
		base.Owner.OnGainPathRank(path);
	}

	public int GetPathRank(BlueprintPath path)
	{
		return Features.GetRank(path);
	}

	public bool CanUpgradePath(BlueprintPath path)
	{
		if (!CanLevelUp)
		{
			return false;
		}
		if (GetPathRank(path) >= path.Ranks)
		{
			return false;
		}
		if (!path.Prerequisites.Meet(base.Owner))
		{
			return false;
		}
		BlueprintCareerPath career = path as BlueprintCareerPath;
		if (career != null)
		{
			if (AllCareerPaths.Any(((BlueprintCareerPath Blueprint, int Rank) i) => i.Rank < i.Blueprint.Ranks && i.Blueprint != path))
			{
				return false;
			}
			if (career.Tier < CareerPathTier.Three && AllCareerPaths.Any(((BlueprintCareerPath Blueprint, int Rank) i) => i.Blueprint != career && i.Blueprint.Tier == career.Tier))
			{
				return false;
			}
		}
		return true;
	}

	protected override void OnPrePostLoad()
	{
		Features = base.Owner.Facts.EnsureFactProcessor<FeatureCollection>();
	}

	protected override void OnPostLoad()
	{
		Dictionary<EntityRef<MechanicEntity>, int> respecUsedByChar = Game.Instance.Player.RespecUsedByChar;
		if (respecUsedByChar.ContainsKey(base.Owner))
		{
			RespecInfo = new RespecInfo(respecUsedByChar[base.Owner]);
			respecUsedByChar.Remove(base.Owner);
		}
		if (RespecInfo == null)
		{
			RespecInfo respecInfo2 = (RespecInfo = new RespecInfo(0));
		}
	}

	protected override void OnApplyPostLoadFixes()
	{
		if (m_Race == null && base.Owner.OriginalBlueprint.Race != null)
		{
			SetRace(base.Owner.OriginalBlueprint.Race);
		}
	}

	protected override void OnDetach()
	{
		Features.Dispose();
	}

	public void SetRace(BlueprintRace race)
	{
		if (m_Race != race)
		{
			if ((bool)m_Race)
			{
				Features.Remove(m_Race);
			}
			m_Race = race;
			Features.Add(m_Race);
		}
	}

	public void ReapplyFeaturesOnLevelUp()
	{
		foreach (Feature item in Features.RawFacts.ToTempList())
		{
			if (!item.IsDisposed)
			{
				if (item.Blueprint.ReapplyOnLevelUp)
				{
					item.Reapply();
				}
				else
				{
					item.Context.Recalculate();
				}
			}
		}
		foreach (Buff buff in base.Owner.Buffs)
		{
			buff.Context.Recalculate();
		}
		foreach (ActivatableAbility activatableAbility in base.Owner.ActivatableAbilities)
		{
			activatableAbility.ReapplyBuff();
		}
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitReapplyFeaturesOnLevelUpHandler>)delegate(IUnitReapplyFeaturesOnLevelUpHandler h)
		{
			h.HandleUnitReapplyFeaturesOnLevelUp();
		}, isCheckRuntime: true);
	}

	public void GainExperience(int exp, bool log = true, bool allowForPet = false)
	{
		if (exp < 0)
		{
			PFLog.LevelUp.ErrorWithReport($"Unit received invalid amount of experience: {exp}");
		}
		else
		{
			if (base.Owner.IsPet && !allowForPet)
			{
				return;
			}
			int expResult = exp * Game.Instance.Player.ExperienceRatePercent / 100;
			if (expResult < 1)
			{
				return;
			}
			bool needNewLevelSound = Experience < ExperienceTable.GetBonus(CharacterLevel + 1);
			Experience += expResult;
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitGainExperienceHandler>)delegate(IUnitGainExperienceHandler h)
			{
				h.HandleUnitGainExperience(expResult, needNewLevelSound);
			}, isCheckRuntime: true);
			if (log)
			{
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUILogGainExperienceHandler>)delegate(IUILogGainExperienceHandler h)
				{
					h.HandleUnitGainExperience(expResult);
				}, isCheckRuntime: true);
			}
		}
	}

	public void AdvanceExperienceTo(int targetExp, bool log = true, bool allowForPet = false)
	{
		int num = targetExp - Experience;
		if (num > 0)
		{
			GainExperience(num, log, allowForPet);
		}
	}

	public void ReplaceFeature(BlueprintFeature remove, BlueprintFeature add)
	{
		Feature feature = Features.Get(remove);
		if (feature != null)
		{
			EntityFactSource entityFactSource = feature.Sources.LastOrDefault();
			Features.Remove(feature);
			Feature feature2 = Features.Add(add);
			if (entityFactSource != null && entityFactSource.Path != null && entityFactSource.PathFeatureSource != null && entityFactSource.PathRank.HasValue)
			{
				feature2?.AddSource(entityFactSource.Path, entityFactSource.PathFeatureSource, entityFactSource.PathRank.Value);
			}
			int num = m_Selections.FindLastIndex((FeatureSelectionData i) => i.Feature == remove);
			if (num >= 0)
			{
				FeatureSelectionData featureSelectionData = m_Selections[num];
				FeatureSelectionData value = new FeatureSelectionData(featureSelectionData.Path, featureSelectionData.Level, featureSelectionData.Selection, add, 1);
				m_Selections[num] = value;
			}
		}
	}

	public void FixCharacterLevelAfterCopy()
	{
		CharacterLevel = Features.RawFacts.Where((Feature i) => i.Blueprint is BlueprintCareerPath).Sum((Feature i) => i.GetRank());
	}

	public void CopyFrom(PartUnitProgression other)
	{
		SetRace(other.Race);
		Experience = other.Experience;
		CharacterLevel = other.CharacterLevel;
		foreach (FeatureSelectionData selection in other.m_Selections)
		{
			AddFeatureSelection(selection.Path, selection.Level, selection.Selection, selection.Feature, selection.Rank);
		}
	}

	public void AddFeatureSelection([NotNull] BlueprintPath path, int level, [NotNull] BlueprintSelectionFeature selection, [NotNull] BlueprintFeature feature, int rank)
	{
		m_Selections.Add(new FeatureSelectionData(path, level, selection, feature, rank));
	}

	[CanBeNull]
	public (BlueprintFeature Feature, int Rank)? GetSelectedFeature([NotNull] BlueprintPath path, int level, [NotNull] BlueprintSelectionFeature selection)
	{
		FeatureSelectionData featureSelectionData = m_Selections.FirstItem((FeatureSelectionData i) => i.Path == path && i.Level == level && i.Selection == selection);
		if (featureSelectionData.Feature == null)
		{
			return null;
		}
		return (featureSelectionData.Feature, featureSelectionData.Rank);
	}

	public IEnumerable<FeatureSelectionData> GetSelectionsByPath(BlueprintPath path)
	{
		return m_Selections.Where((FeatureSelectionData i) => i.Path == path);
	}

	public int GetRespecCost()
	{
		if (RespecInfo == null)
		{
			RespecInfo respecInfo2 = (RespecInfo = new RespecInfo());
		}
		return RespecInfo.GetRespecCost();
	}

	public void CountRespecIn()
	{
		RespecInfo.CountRespecIn();
	}

	public static bool CanRespec(BaseUnitEntity ch)
	{
		if (ch == null)
		{
			return false;
		}
		int num = ch.OriginalBlueprint.GetComponent<CharacterLevelLimit>()?.LevelLimit ?? 0;
		if (!ch.LifeState.IsDead && !ch.IsPet)
		{
			return ch.Progression.CharacterLevel > num;
		}
		return false;
	}

	public void GiveExtraRespec()
	{
		if (RespecInfo == null)
		{
			RespecInfo respecInfo2 = (RespecInfo = new RespecInfo());
		}
		RespecInfo.GiveExtraRespec();
	}

	public void Respec()
	{
		var (path, rank) = AllCareerPaths.Last();
		rank--;
		int defaultLevel = base.Owner.Blueprint.GetDefaultLevel();
		while (CharacterLevel > defaultLevel && path != null)
		{
			foreach (BlueprintFeature feature2 in path.RankEntries[rank].Features)
			{
				if (Features.GetRank(feature2) > 1)
				{
					Features.Get(feature2).RemoveRank();
					continue;
				}
				Features.Get(feature2)?.RemoveRank();
				Features.Remove(feature2);
			}
			List<FeatureSelectionData> list = m_Selections.Where((FeatureSelectionData selection) => selection.Path == path && selection.Level == rank + 1).ToList();
			for (int i = 1; i <= list.Count; i++)
			{
				EntityFactsManager facts = base.Owner.Facts;
				int num = i;
				Feature feature = facts.Get<Feature>(list[list.Count - num].Feature);
				if (feature != null && feature.GetRank() > 1)
				{
					EntityFactsManager facts2 = base.Owner.Facts;
					num = i;
					facts2.Get<Feature>(list[list.Count - num].Feature)?.RemoveRank();
				}
				else
				{
					EntityFactsManager facts3 = base.Owner.Facts;
					num = i;
					facts3.Get<Feature>(list[list.Count - num].Feature)?.RemoveRank();
					EntityFactsManager facts4 = base.Owner.Facts;
					num = i;
					facts4.Remove(list[list.Count - num].Feature);
				}
				List<FeatureSelectionData> selections = m_Selections;
				num = i;
				selections.Remove(list[list.Count - num]);
			}
			if (rank > 0)
			{
				Features.Get(path).RemoveRank();
			}
			else
			{
				base.Owner.Facts.Remove(path);
				Features.Remove(path);
				(path, rank) = AllCareerPaths.Last();
			}
			rank--;
			CharacterLevel--;
		}
	}

	public int GetRank(BlueprintFeature feature)
	{
		return Features.GetRank(feature);
	}

	public int GetRank(BlueprintCareerPath careerPath)
	{
		return Features.GetRank(careerPath);
	}

	public void AdvanceExperienceToLevel(int targetCharacterLevel, bool log)
	{
		AdvanceExperienceTo(ExperienceTable.GetBonus(targetCharacterLevel));
	}

	[Obsolete]
	public int GetClassLevel([NotNull] BlueprintCharacterClass characterClass)
	{
		return 0;
	}

	[Obsolete]
	[CanBeNull]
	public ClassData GetClassData([CanBeNull] BlueprintCharacterClass characterClass)
	{
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_Race);
		result.Append(ref val2);
		int val3 = CharacterLevel;
		result.Append(ref val3);
		List<FeatureSelectionData> selections = m_Selections;
		if (selections != null)
		{
			for (int i = 0; i < selections.Count; i++)
			{
				FeatureSelectionData obj = selections[i];
				Hash128 val4 = StructHasher<FeatureSelectionData>.GetHash128(ref obj);
				result.Append(ref val4);
			}
		}
		Hash128 val5 = ClassHasher<RespecInfo>.GetHash128(RespecInfo);
		result.Append(ref val5);
		int val6 = Experience;
		result.Append(ref val6);
		return result;
	}
}
