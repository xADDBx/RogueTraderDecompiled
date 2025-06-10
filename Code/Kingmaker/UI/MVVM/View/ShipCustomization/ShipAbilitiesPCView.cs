using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipAbilitiesPCView : ShipAbilitiesBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private CharInfoFeaturePCView m_CharInfoFeaturePCView;

	protected override void DrawEntitiesImpl()
	{
		base.DrawEntitiesImpl();
		m_ActiveAbilitiesWidgetList.DrawEntries(base.ViewModel.ActiveAbilities.ToArray(), m_CharInfoFeaturePCView);
		m_PassiveAbilitiesWidgetList.DrawEntries(base.ViewModel.PassiveAbilities.ToArray(), m_CharInfoFeaturePCView);
	}
}
