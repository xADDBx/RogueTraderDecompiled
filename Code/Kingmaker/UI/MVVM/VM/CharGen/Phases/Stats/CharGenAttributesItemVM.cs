using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Owlcat.Runtime.UI.Tooltips;
using Photon.Realtime;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Stats;

public class CharGenAttributesItemVM : CharGenBackgroundBaseItemVM, INetLobbyPlayersHandler, ISubscriber
{
	public readonly StatType StatType;

	public readonly int ValuePerRank;

	public readonly ReactiveProperty<int> StatValue = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> CanAdvance = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> CanRetreat = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> DiffValue = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> StatRanks = new ReactiveProperty<int>();

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveProperty<bool> IsRecommended = new ReactiveProperty<bool>();

	private readonly Action<StatType, bool> m_AdvanceAction;

	private readonly Action<CharGenAttributesItemVM> m_OnHovered;

	public readonly ReactiveCommand<bool> CheckCoopControls = new ReactiveCommand<bool>();

	public readonly BoolReactiveProperty IsMainCharacter = new BoolReactiveProperty();

	public CharGenAttributesItemVM(FeatureSelectionItem selectionItem, Action<StatType, bool> advanceAction, Action<CharGenAttributesItemVM> onHovered, CharGenPhaseType phaseType, ReactiveProperty<CharGenPhaseBaseVM> currentPhase)
		: base(selectionItem, null, phaseType, currentPhase)
	{
		if (selectionItem.Feature is BlueprintStatAdvancement blueprintStatAdvancement)
		{
			StatType = blueprintStatAdvancement.Stat;
			ValuePerRank = blueprintStatAdvancement.ValuePerRank;
			base.DisplayName = LocalizedTexts.Instance.Stats.GetText(StatType);
			m_AdvanceAction = advanceAction;
			m_OnHovered = onHovered;
			IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
			AddDisposable(EventBus.Subscribe(this));
		}
	}

	protected override void DoSelectMe()
	{
	}

	public void AdvanceStat()
	{
		if (CanAdvance.Value)
		{
			m_AdvanceAction?.Invoke(StatType, arg2: true);
		}
	}

	public void RetreatStat()
	{
		if (CanRetreat.Value)
		{
			m_AdvanceAction?.Invoke(StatType, arg2: false);
		}
	}

	public void UpdateTooltip(ModifiableValue unitStat)
	{
		StatTooltipData statTooltipData;
		if (!(unitStat is ModifiableValueAttributeStat attribute))
		{
			if (!(unitStat is ModifiableValueSkill skill))
			{
				if (!(unitStat is ModifiableValueSavingThrow savingThrow))
				{
					if (unitStat == null)
					{
						throw new SwitchExpressionException(unitStat);
					}
					statTooltipData = new StatTooltipData(unitStat);
				}
				else
				{
					statTooltipData = new StatTooltipData(savingThrow);
				}
			}
			else
			{
				statTooltipData = new StatTooltipData(skill);
			}
		}
		else
		{
			statTooltipData = new StatTooltipData(attribute);
		}
		StatTooltipData statData = statTooltipData;
		Tooltip.Value = new TooltipTemplateStat(statData);
	}

	public void OnHovered(bool state)
	{
		m_OnHovered?.Invoke(state ? this : null);
	}

	public void UpdateRecommendedMark(List<StatType> recommendedStats)
	{
		IsRecommended.Value = recommendedStats.Contains(StatType);
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		CheckCoopControls.Execute(UINetUtility.IsControlMainCharacter());
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}
}
