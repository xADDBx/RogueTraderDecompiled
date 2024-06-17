using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Common;

public class TempPregenSelectorCommonView : ViewBase<SelectionGroupRadioVM<CharGenPregenSelectorItemVM>>, IConsoleNavigationEntity, IConsoleEntity, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, INavigationRightDirectionHandler
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private TempPregenSelectorItemCommonView m_ItemPrefab;

	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	protected override void BindViewImplementation()
	{
		DrawEntities();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemPrefab);
	}

	public bool HandleLeft()
	{
		return base.ViewModel.SelectPrevValidEntity();
	}

	public bool HandleRight()
	{
		return base.ViewModel.SelectNextValidEntity();
	}

	public void SetFocus(bool value)
	{
		if (m_Selectable != null)
		{
			m_Selectable.SetActiveLayer(value ? "Selected" : "Normal");
		}
	}

	public bool IsValid()
	{
		return true;
	}
}
