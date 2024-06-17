using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.SpaceCombat.Base;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Console;

public class ShipPostConsoleView : ShipPostBaseView
{
	[Header("Console")]
	[SerializeField]
	private CanvasGroup m_PortraitBlock;

	[SerializeField]
	private AbilitiesGroupConsoleView m_AbilitiesGroupConsoleView;

	private Coroutine m_FXRoutine;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_PortraitBlock.alpha = 0f;
		m_AbilitiesGroupConsoleView.Bind(base.ViewModel.AbilitiesGroup);
	}

	public void SetHighlighted(bool isHighlighted)
	{
		if (!(m_Portrait.sprite == null))
		{
			m_PortraitBlock.alpha = (isHighlighted ? 1f : 0f);
		}
	}

	public List<IConsoleNavigationEntity> GetConsoleEntities()
	{
		return m_AbilitiesGroupConsoleView.GetConsoleEntities();
	}
}
