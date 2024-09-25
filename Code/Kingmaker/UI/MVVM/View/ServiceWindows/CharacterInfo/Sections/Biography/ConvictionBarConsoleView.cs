using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Biography;

public class ConvictionBarConsoleView : ConvictionBarBaseView
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_RightLabelButton;

	[SerializeField]
	private OwlcatMultiButton m_LeftLabelButton;

	private const float ButtonFocusCoeff = 0.5f;

	public List<SimpleConsoleNavigationEntity> GetEntities()
	{
		float x = m_Container.sizeDelta.x;
		float num = (x - m_Cursor.sizeDelta.x * 2.5f) / x;
		OwlcatMultiButton button = ((base.ViewModel.CurrentRelativeValue.Value > 0f - num) ? m_LeftButtonPuritan : m_LeftLabelButton);
		OwlcatMultiButton button2 = ((base.ViewModel.CurrentRelativeValue.Value < num) ? m_RightButtonRadical : m_RightLabelButton);
		SimpleConsoleNavigationEntity item = new SimpleConsoleNavigationEntity(button, base.ViewModel.PuritanTooltip);
		SimpleConsoleNavigationEntity item2 = new SimpleConsoleNavigationEntity(m_CurrentCursor);
		SimpleConsoleNavigationEntity item3 = new SimpleConsoleNavigationEntity(button2, base.ViewModel.RadicalTooltip);
		return new List<SimpleConsoleNavigationEntity> { item, item2, item3 };
	}
}
