using System.Linq;
using Kingmaker.Enums;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;

public class CharGenDefaultPortraitGroupView : ViewBase<CharGenPortraitGroupVM>, IWidgetView, IConsoleEntityProxy, IConsoleEntity
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharGenPortraitSelectorItemView m_Prefab;

	[SerializeField]
	private ExpandableElement m_ExpandableElement;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private int m_PortraitsInRow = 7;

	private GridConsoleNavigationBehaviour m_Navigation;

	public bool IsExpanded
	{
		get
		{
			if (base.ViewModel != null)
			{
				return base.ViewModel.Expanded.Value;
			}
			return false;
		}
	}

	public PortraitCategory PortraitCategory => base.ViewModel?.PortraitCategory ?? PortraitCategory.None;

	public MonoBehaviour MonoBehaviour => this;

	public IConsoleEntity ConsoleEntityProxy => m_Navigation;

	protected override void BindViewImplementation()
	{
		m_ExpandableElement.Or(null)?.Initialize(delegate
		{
			base.ViewModel.Expanded.Value = true;
		}, delegate
		{
			base.ViewModel.Expanded.Value = false;
		});
		AddDisposable(m_ExpandableElement);
		if (m_Label != null)
		{
			m_Label.text = UIUtility.GetCharGenPortraitCategoryLabel(base.ViewModel.PortraitCategory);
		}
		AddDisposable(base.ViewModel.PortraitCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}));
		AddDisposable(base.ViewModel.Expanded.Subscribe(delegate(bool expanded)
		{
			if (expanded)
			{
				m_ExpandableElement.Or(null)?.Expand();
			}
			else
			{
				m_ExpandableElement.Or(null)?.Collapse();
			}
		}));
		DrawEntities();
	}

	private void UpdateNavigation()
	{
		if (m_Navigation == null)
		{
			AddDisposable(m_Navigation = new GridConsoleNavigationBehaviour());
		}
		m_Navigation.Clear();
		m_Navigation.SetEntitiesGrid(m_WidgetList.Entries.Cast<IConsoleEntity>().ToList(), m_PortraitsInRow);
		m_Navigation.InsertRow<ExpandableElement>(0, m_ExpandableElement);
	}

	public void FocusOnSelectedEntityOrFirst()
	{
		IConsoleNavigationEntity selectedEntity = GetSelectedEntity();
		if (selectedEntity == null)
		{
			m_Navigation.FocusOnFirstValidEntity();
		}
		else
		{
			m_Navigation.FocusOnEntityManual(selectedEntity);
		}
	}

	private void DrawEntities()
	{
		AddDisposable(m_WidgetList.DrawEntries(base.ViewModel.PortraitCollection, m_Prefab));
		UpdateNavigation();
	}

	private IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries?.Cast<CharGenPortraitSelectorItemView>().FirstOrDefault((CharGenPortraitSelectorItemView i) => i.IsSelected);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharGenPortraitGroupVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharGenPortraitGroupVM;
	}
}
