using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoRankEntryPCView : CharInfoFeatureSimpleBaseView
{
	[SerializeField]
	private Color32 m_FeatureIconColorOverride = new Color32(92, 128, 43, byte.MaxValue);

	[SerializeField]
	private Color32 m_AcronymColorOverride = new Color32(142, 214, 138, 191);

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetTooltip();
		if (base.ViewModel.Icon == null && (!(m_GroupsView != null) || !base.ViewModel.TalentIconsInfo.HasGroups))
		{
			m_FeatureIcon.color = m_FeatureIconColorOverride;
			m_AcronymText.color = m_AcronymColorOverride;
		}
	}

	private void SetTooltip()
	{
		if (base.ViewModel.Tooltip != null)
		{
			AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
		}
	}
}
