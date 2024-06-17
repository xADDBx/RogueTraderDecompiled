using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Exploration;

public class AnomalyCharacterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public List<CharInfoFeatureVM> FeatureList;

	public string Label;

	public string TooltipKey;

	private readonly BaseUnitEntity m_Unit;

	private readonly AnomalyView m_Anomaly;

	private readonly int m_Index;

	public readonly StatType RequiredSkill;

	public readonly AnomalyEntityData Data;

	public readonly string SkillValue;

	public readonly string SkillValueModificator;

	public readonly string RequiredSkillName;

	public readonly string CharacterName;

	public readonly Sprite CharacterPortrait;

	private readonly Action m_DestroyCallback;

	public readonly ReactiveProperty<bool> NeedUpdate = new ReactiveProperty<bool>(initialValue: false);

	private string LabelFeats => string.Empty;

	public AnomalyCharacterVM(BaseUnitEntity unit, AnomalyView anomaly, int index, Action destroyCallback)
	{
		m_Unit = unit;
		m_Anomaly = anomaly;
		m_Index = index;
		m_DestroyCallback = destroyCallback;
		Data = anomaly.Data;
		CharacterPortrait = unit.Portrait?.SmallPortrait;
		CharacterName = unit.CharacterName;
		if (!Data.IsInteracted)
		{
			AnomalyResearch component = Data.Blueprint.GetComponent<AnomalyResearch>();
			RequiredSkill = component.Stats[index].Stat;
			RequiredSkillName = LocalizedTexts.Instance.Stats.GetText(RequiredSkill);
			SkillValue = unit.Stats.GetStat<ModifiableValue>(RequiredSkill).ModifiedValue.ToString();
			List<CharInfoFeatureVM> list = new List<CharInfoFeatureVM>();
			if (list.Count != 0)
			{
				FillCharacterFeatures(list, LabelFeats, "Feat");
			}
		}
	}

	public void FillCharacterFeatures(List<CharInfoFeatureVM> featuresListGroup, string label = null, string tooltipKey = null)
	{
		FeatureList?.ForEach(delegate(CharInfoFeatureVM f)
		{
			f.Dispose();
		});
		FeatureList?.Clear();
		FeatureList = featuresListGroup;
		Label = label;
		TooltipKey = tooltipKey;
		NeedUpdate.Value = !NeedUpdate.Value;
	}

	public void ScanAnomaly()
	{
		if (Data.Blueprint.GetComponent<AnomalyResearch>() != null)
		{
			m_DestroyCallback();
			EventBus.RaiseEvent(delegate(IAnomalyUIHandler h)
			{
				h.UpdateAnomalyScreen(m_Anomaly);
			});
		}
	}

	protected override void DisposeImplementation()
	{
		FeatureList?.ForEach(delegate(CharInfoFeatureVM f)
		{
			f.Dispose();
		});
		FeatureList?.Clear();
	}
}
