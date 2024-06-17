using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeatureGroupConsoleView : CharInfoFeatureGroupPCView
{
	private Action<Ability> m_OnClick;

	[SerializeField]
	private int m_NumberOfRows = 3;

	public void SetupClickAction(Action<Ability> onClick)
	{
		m_OnClick = onClick;
		m_WidgetList.Entries?.ForEach(delegate(IWidgetView e)
		{
			(e as CharInfoFeatureConsoleView)?.SetupClickAction(m_OnClick);
		});
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_WidgetList.Entries?.ForEach(delegate(IWidgetView e)
		{
			(e as CharInfoFeatureConsoleView)?.SetupClickAction(m_OnClick);
		});
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		if (m_WidgetList.Entries == null)
		{
			gridConsoleNavigationBehaviour.AddRow<ExpandableCollapseMultiButtonPC>(m_ExpandableElement);
			return gridConsoleNavigationBehaviour;
		}
		gridConsoleNavigationBehaviour.SetEntitiesGrid(m_WidgetList.Entries.Select((IWidgetView e) => e as CharInfoFeatureConsoleView).ToList(), m_NumberOfRows);
		gridConsoleNavigationBehaviour.InsertRow<ExpandableCollapseMultiButtonPC>(0, m_ExpandableElement);
		return gridConsoleNavigationBehaviour;
	}
}
