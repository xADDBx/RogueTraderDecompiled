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
				m_GrayScale.EffectAmount = ((state == RankFeatureState.NotActive) ? 1f : 0f);
				m_Mask.enabled = true;
				m_GrayScale.gameObject.SetActive(value: true);
			});
		}
		base.BindViewImplementation();
	}
}
