using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Retrain;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Respec;

public class RespecCharactersSelectorView : ViewBase<SelectionGroupRadioVM<RespecCharacterVM>>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private RespecCharacterCommonView m_RespecCharacterCommonView;

	protected override void BindViewImplementation()
	{
		DrawEntities();
	}

	protected override void DestroyViewImplementation()
	{
		m_WidgetList.Clear();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_RespecCharacterCommonView);
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetList.GetNavigationEntities();
	}
}
