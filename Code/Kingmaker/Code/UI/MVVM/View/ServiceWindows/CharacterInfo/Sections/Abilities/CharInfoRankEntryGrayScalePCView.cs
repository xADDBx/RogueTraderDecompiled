using System;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoRankEntryGrayScalePCView : CharInfoRankEntryPCView
{
	[SerializeField]
	private Image m_Mask;

	[SerializeField]
	private _2dxFX_GrayScale m_GrayScale;

	protected override void BindViewImplementation()
	{
		if (base.ViewModel is BaseRankEntryFeatureVM baseRankEntryFeatureVM)
		{
			baseRankEntryFeatureVM.FeatureState.Subscribe(delegate(RankFeatureState state)
			{
				m_GrayScale.gameObject.SetActive(value: false);
				m_Mask.enabled = false;
				m_GrayScale.EffectAmount = GetGreyEffectAmount(state);
				m_Mask.enabled = true;
				m_GrayScale.gameObject.SetActive(value: true);
			});
		}
		base.BindViewImplementation();
	}

	private float GetGreyEffectAmount(RankFeatureState state)
	{
		return state switch
		{
			RankFeatureState.Selectable => 0.75f, 
			RankFeatureState.NotActive => 1f, 
			RankFeatureState.Selected => 0.25f, 
			RankFeatureState.NotValid => 0f, 
			RankFeatureState.Committed => 0f, 
			RankFeatureState.NotSelectable => 0.5f, 
			_ => throw new ArgumentOutOfRangeException("state", state, null), 
		};
	}
}
