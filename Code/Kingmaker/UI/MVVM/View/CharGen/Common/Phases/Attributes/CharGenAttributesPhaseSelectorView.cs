using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Stats;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Attributes;

public class CharGenAttributesPhaseSelectorView : ViewBase<SelectionGroupRadioVM<CharGenAttributesItemVM>>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharGenAttributesPhaseSelectorItemView m_ItemViewPrefab;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	protected override void BindViewImplementation()
	{
		DrawEntities();
		AddDisposable(base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}));
		m_Label.text = UIStrings.Instance.CharGen.CharacterSkillPoints;
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemViewPrefab, strictMatching: true);
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetList.GetNavigationEntities();
	}

	public IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<CharGenAttributesPhaseSelectorItemView>().FirstOrDefault((CharGenAttributesPhaseSelectorItemView i) => (i.GetViewModel() as CharGenAttributesItemVM)?.IsSelected.Value ?? false);
	}
}
