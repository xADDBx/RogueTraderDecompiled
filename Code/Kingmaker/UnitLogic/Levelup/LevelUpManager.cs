using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.UnitLogic.Levelup;

public class LevelUpManager : LevelUpManager.IOnSelectionChangedAccess, IDisposable
{
	internal interface IOnSelectionChangedAccess
	{
		void OnSelectionChanged(SelectionState selection);
	}

	private readonly List<SelectionState> m_Selections = new List<SelectionState>();

	[NotNull]
	public BaseUnitEntity TargetUnit { get; }

	public bool AutoCommit { get; }

	public BaseUnitEntity PreviewUnit { get; private set; }

	[CanBeNull]
	public BlueprintPath Path { get; private set; }

	public bool IsCommitted { get; private set; }

	public ReadonlyList<SelectionState> Selections => m_Selections;

	public bool IsAllSelectionsMadeAndValid => m_Selections.AllItems((SelectionState i) => (i.IsMade || !i.CanSelectAny) && i.IsValid);

	public LevelUpManager([NotNull] BaseUnitEntity targetUnit, [NotNull] BlueprintPath careerPath, bool autoCommit, int targetCharacterLevel = 0)
	{
		TargetUnit = targetUnit;
		IsCommitted = (AutoCommit = autoCommit);
		if (targetCharacterLevel == 0)
		{
			targetCharacterLevel = TargetUnit.Progression.ExperienceLevel;
		}
		PreviewUnit = (AutoCommit ? TargetUnit : CreatePreviewUnit());
		SelectCareerPath(careerPath, targetCharacterLevel);
	}

	[CanBeNull]
	public SelectionState GetSelectionState(BlueprintPath path, BlueprintSelection selection, int pathRank)
	{
		return m_Selections.FirstItem((SelectionState i) => i.Path == path && i.Blueprint == selection && i.PathRank == pathRank);
	}

	private void SelectCareerPath(BlueprintPath path, int targetCharacterLevel)
	{
		if (AutoCommit && Path != null)
		{
			throw new InvalidOperationException("Can't change selected Path when AutoCommit == true");
		}
		if (path.Ranks != path.RankEntries.Length)
		{
			throw new Exception($"Invalid path {path} settings (Ranks != RankEntries.Length)");
		}
		Path = path;
		m_Selections.RemoveAll((SelectionState i) => i.Path is BlueprintCareerPath);
		m_Selections.AddRange(CreatePathSelections(targetCharacterLevel));
		AdvancePathRankTo(PreviewUnit, path, GetTargetPathRankOfSelectPath(targetCharacterLevel));
	}

	private int GetTargetPathRankOfSelectPath(int targetCharacterLevel)
	{
		if (Path is BlueprintOriginPath)
		{
			return Path.Ranks;
		}
		int currentPathRank = TargetUnit.Progression.GetPathRank(Path);
		int val = m_Selections.SkipWhile((SelectionState i) => i.PathRank <= currentPathRank || !i.CanSelectAny).FirstOrDefault()?.PathRank ?? Path?.Ranks ?? 0;
		return Math.Min(Math.Max(0, targetCharacterLevel - TargetUnit.Progression.CharacterLevel), Math.Min(val, GetMaxAvailablePathRank()));
	}

	private int GetMaxAvailablePathRank()
	{
		if (Path is BlueprintOriginPath)
		{
			return Path.Ranks;
		}
		PartUnitProgression progression = TargetUnit.Progression;
		return progression.GetPathRank(Path) + progression.ExperienceLevel - progression.CharacterLevel;
	}

	public void Commit()
	{
		if (AutoCommit)
		{
			throw new InvalidOperationException("Can't commit levelup: AutoCommit == true");
		}
		if (IsCommitted)
		{
			throw new InvalidOperationException("Can't commit levelup: already committed");
		}
		if (!IsAllSelectionsMadeAndValid)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (SelectionState item in m_Selections.Where((SelectionState i) => (!i.IsMade && i.CanSelectAny) || !i.IsValid))
			{
				stringBuilder.AppendLine($"\t{item.Blueprint} - IsMade: {item.IsMade}, IsValid: {item.IsValid}, CanSelectAny: {item.CanSelectAny}");
			}
			throw new InvalidOperationException($"Can't commit levelup: not all selections made and valid\n{stringBuilder}");
		}
		IsCommitted = true;
		AdvancePathRankTo(TargetUnit, Path, TargetUnit.Progression.GetRank(Path) + 1);
		ApplySelections(TargetUnit, invalidate: false);
	}

	private IEnumerable<SelectionState> CreatePathSelections(int targetCharacterLevel)
	{
		if (Path == null)
		{
			yield break;
		}
		(int From, int To) range = GetRanksRangeForLevelUp(targetCharacterLevel);
		var (i, _) = range;
		for (; i <= range.To; i++)
		{
			BlueprintPath.RankEntry rankEntry = Path.GetRankEntry(i);
			if (rankEntry == null)
			{
				PFLog.LevelUp.ErrorWithReport($"Can't get level {i} of career path {Path}");
				break;
			}
			int characterLevel = ((!(Path is BlueprintOriginPath)) ? i : 0);
			foreach (BlueprintSelection selection2 in rankEntry.Selections)
			{
				if (!(selection2 is BlueprintSelectionFeature selection) || !TargetUnit.Progression.GetSelectedFeature(Path, characterLevel, selection).HasValue)
				{
					yield return SelectionStateFactory.Create(this, selection2, Path, characterLevel);
				}
			}
		}
	}

	private (int From, int To) GetRanksRangeForLevelUp(int targetCharacterLevel)
	{
		BlueprintPath path = Path;
		if (path != null)
		{
			if (path is BlueprintOriginPath)
			{
				return (From: 1, To: Path.Ranks);
			}
			PartUnitProgression progression = TargetUnit.Progression;
			int characterLevel = progression.CharacterLevel;
			int rank = progression.Features.GetRank(Path);
			int max = Math.Max(0, targetCharacterLevel - characterLevel);
			int num = Math.Clamp(Path.Ranks - rank, 0, max);
			return (From: rank + 1, To: rank + num);
		}
		return (From: 0, To: 0);
	}

	private BaseUnitEntity CreatePreviewUnit()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.DoNotCreateItems>.Request())
			{
				using (ContextData<UnitHelper.PreviewUnit>.Request())
				{
					BaseUnitEntity baseUnitEntity = TargetUnit.CreatePreview(createView: false);
					baseUnitEntity.Progression.AdvanceToNextLevel();
					return baseUnitEntity;
				}
			}
		}
	}

	void IOnSelectionChangedAccess.OnSelectionChanged(SelectionState selection)
	{
		if (AutoCommit || !(selection is SelectionStateFeature))
		{
			ApplySelection(PreviewUnit, selection, invalidateNext: true);
			return;
		}
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.DoNotCreateItems>.Request())
			{
				using (ContextData<UnitHelper.PreviewUnit>.Request())
				{
					PreviewUnit?.Dispose();
					PreviewUnit = CreatePreviewUnit();
					ApplySelections(PreviewUnit, invalidate: true);
				}
			}
		}
	}

	private void AdvancePathRankTo(BaseUnitEntity unit, BlueprintPath path, int rank)
	{
		if (IsPreview(unit))
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				Apply();
				return;
			}
		}
		Apply();
		static void AddPathRank(BaseUnitEntity u, BlueprintPath p)
		{
			int num = u.Progression.GetPathRank(p) + 1;
			BlueprintPath.RankEntry obj = p.GetRankEntry(num) ?? throw new Exception($"Rank entry is null: path {p}, rank {num}");
			u.Progression.AddPathRank(p);
			foreach (BlueprintFeature feature in obj.Features)
			{
				u.Progression.Features.Add(feature)?.AddSource(p, p, num);
			}
		}
		void Apply()
		{
			int rank2 = unit.Progression.Features.GetRank(path);
			if (rank2 < rank && rank <= GetMaxAvailablePathRank())
			{
				if (rank > path.Ranks)
				{
					throw new Exception($"Can't advance {path} rank to {rank} (max rank is {path.Ranks})");
				}
				for (int i = 0; i < rank - rank2; i++)
				{
					AddPathRank(unit, path);
				}
			}
		}
	}

	private void ApplySelections(BaseUnitEntity unit, bool invalidate)
	{
		int num = m_Selections.FindLastIndex((SelectionState i) => i.IsMade);
		for (int j = 0; j < m_Selections.Count; j++)
		{
			SelectionState selection = m_Selections[j];
			if (invalidate)
			{
				InvalidateSelection(selection);
			}
			if (j <= num)
			{
				ApplySelection(unit, selection, invalidateNext: false);
			}
		}
	}

	private void ApplySelection(BaseUnitEntity unit, SelectionState selection, bool invalidateNext)
	{
		if (IsPreview(unit))
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				Apply();
				return;
			}
		}
		Apply();
		void Apply()
		{
			int num = m_Selections.IndexOf(selection);
			if (num < 0)
			{
				throw new InvalidOperationException("Can't apply selection which doesn't belong to m_Selections");
			}
			int pathRank = unit.Progression.GetPathRank(selection.Path);
			int num2 = num + 1;
			SelectionState selectionState = m_Selections.Get(num2);
			while (selectionState != null && !selectionState.CanSelectAny)
			{
				selectionState = m_Selections.Get(++num2);
			}
			int num3 = ((selectionState == null) ? selection.Path.Ranks : ((selectionState.Path == selection.Path && selectionState.PathRank > selection.PathRank) ? selectionState.PathRank : selection.PathRank));
			for (int i = pathRank + 1; i <= num3; i++)
			{
				AdvancePathRankTo(unit, selection.Path, i);
			}
			EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IChangeUnitLevel>)delegate(IChangeUnitLevel h)
			{
				h.HandleChangeUnitLevel();
			}, isCheckRuntime: true);
			selection.Apply(unit);
			if (invalidateNext)
			{
				m_Selections.Skip(num + 1).ForEach(InvalidateSelection);
			}
		}
	}

	private static void InvalidateSelection(SelectionState selection)
	{
		((SelectionState.IInvalidateAccess)selection).Invalidate();
	}

	private bool IsPreview(BaseUnitEntity unit)
	{
		if (unit == PreviewUnit)
		{
			return PreviewUnit != TargetUnit;
		}
		return false;
	}

	public void Dispose()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.PreviewUnit>.Request())
			{
				using (ContextData<UnitHelper.DoNotCreateItems>.Request())
				{
					if (PreviewUnit != TargetUnit)
					{
						PreviewUnit?.Dispose();
					}
				}
			}
		}
	}
}
