using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UI.Common;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public class LevelupFeaturePCView : CharInfoFeatureSimpleBaseView
{
	[SerializeField]
	protected TextMeshProUGUI m_DisplayName;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_DisplayName);
		}
		base.BindViewImplementation();
		SetupName();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}

	public void SetupName()
	{
		if (!(m_DisplayName == null))
		{
			m_DisplayName.text = base.ViewModel.DisplayName;
		}
	}

	public void SetCommonState(bool state)
	{
		if (state)
		{
			m_Icon.color = UIConfig.Instance.LevelupColors.CommonTalentColor;
			m_Icon.sprite = Game.Instance.BlueprintRoot.UIConfig.UIIcons.AbilityPlaceholderIcon.LastOrDefault();
		}
	}
}
