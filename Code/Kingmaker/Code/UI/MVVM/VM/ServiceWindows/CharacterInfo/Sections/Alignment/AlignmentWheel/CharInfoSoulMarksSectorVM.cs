using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;

public sealed class CharInfoSoulMarksSectorVM : CharInfoComponentVM
{
	public readonly SoulMarkDirection Direction;

	public readonly AutoDisposingList<CharInfoAlignmentAbilitySlotVM> AbilitySlotVms = new AutoDisposingList<CharInfoAlignmentAbilitySlotVM>();

	public TooltipBaseTemplate Tooltip;

	public readonly BlueprintSoulMark BaseBlueprint;

	public readonly List<BlueprintSoulMark> SoulMarks = new List<BlueprintSoulMark>();

	private List<int> m_RankThresholds = new List<int>();

	private int m_MaxRank;

	private int m_CurrentRank;

	private int m_CurrentLevel;

	public int CurrentRank => m_CurrentRank;

	public int CurrentLevel => m_CurrentLevel;

	public int MaxRank => m_MaxRank;

	public List<int> RankThresholds => m_RankThresholds;

	public CharInfoSoulMarksSectorVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, SoulMarkDirection direction)
		: base(unit)
	{
		Direction = direction;
		BaseBlueprint = SoulMarkShiftExtension.GetBaseSoulMarkFor(direction);
		if (BaseBlueprint == null)
		{
			return;
		}
		SoulMarks = (from f in BaseBlueprint.ComponentsArray.Select(delegate(BlueprintComponent c)
			{
				RankChangedTrigger obj = c as RankChangedTrigger;
				return (obj == null) ? null : obj.Facts.FirstOrDefault((BlueprintMechanicEntityFact f) => f is BlueprintSoulMark);
			})
			select f as BlueprintSoulMark).ToList();
		for (int i = 0; i < SoulMarks.Count; i++)
		{
			BlueprintSoulMark soulMark = SoulMarks[i];
			AbilitySlotVms.Add(new CharInfoAlignmentAbilitySlotVM(Unit, soulMark, direction, i + 1));
		}
		UpdateSoulMarkInfo();
	}

	public void UpdateSoulMarkInfo()
	{
		SoulMarkTooltipExtensions.GetSoulMarkInfo(BaseBlueprint, Unit.Value, out m_RankThresholds, out m_MaxRank, out m_CurrentRank, out m_CurrentLevel);
		Tooltip = new TooltipTemplateSoulMarkHeader(Unit.Value, Direction);
	}

	protected override void DisposeImplementation()
	{
		AbilitySlotVms.Clear();
		RankThresholds.Clear();
	}

	public void UpdateMainDirection(SoulMarkDirection? direction)
	{
		foreach (CharInfoAlignmentAbilitySlotVM abilitySlotVm in AbilitySlotVms)
		{
			abilitySlotVm.UpdateMainDirection(direction);
		}
	}
}
