using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeatureGroupConsoleView : CharInfoFeatureGroupPCView
{
	[Header("Console")]
	[SerializeField]
	private int m_ItemsInRow = 3;

	public CharInfoFeatureGroupVM.FeatureGroupType GroupType => m_GroupType;

	public void SetupChooseModeActions(Action<CharInfoFeatureConsoleView> onClick, Action<CharInfoFeatureConsoleView> onFocus)
	{
		m_WidgetList.Entries?.ForEach(delegate(IWidgetView e)
		{
			(e as CharInfoFeatureConsoleView)?.SetupChooseModeActions(onClick, onFocus);
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
		gridConsoleNavigationBehaviour.SetEntitiesGrid(m_WidgetList.Entries.Select((IWidgetView e) => e as CharInfoFeatureConsoleView).ToList(), m_ItemsInRow);
		gridConsoleNavigationBehaviour.InsertRow<ExpandableCollapseMultiButtonPC>(0, m_ExpandableElement);
		return gridConsoleNavigationBehaviour;
	}

	public IConsoleEntity GetFirstFeature()
	{
		return m_WidgetList.Entries?.FirstOrDefault((IWidgetView e) => e is CharInfoFeatureConsoleView) as IConsoleEntity;
	}
}
