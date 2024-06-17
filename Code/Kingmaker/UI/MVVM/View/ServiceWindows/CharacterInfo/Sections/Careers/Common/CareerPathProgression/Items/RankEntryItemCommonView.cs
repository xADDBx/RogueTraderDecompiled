using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public class RankEntryItemCommonView : ViewBase<CareerPathRankEntryVM>
{
	[SerializeField]
	private GameObject m_FirstRankDecoration;

	[FormerlySerializedAs("m_FeatureItemPCView")]
	[SerializeField]
	private RankEntryFeatureItemCommonView m_FeatureItemCommonView;

	[FormerlySerializedAs("m_SelectionItemPCView")]
	[SerializeField]
	private RankEntrySelectionItemCommonView m_SelectionItemCommonView;

	[SerializeField]
	private OwlcatMultiSelectable m_EmptyDummy;

	[SerializeField]
	private RectTransform m_Container;

	[SerializeField]
	private bool m_IsVoidshipEntry;

	[SerializeField]
	[ConditionalShow("m_IsVoidshipEntry")]
	private RectTransform m_ForShipContainer;

	[SerializeField]
	private bool m_IsListEntry;

	private readonly List<MonoBehaviour> m_AllItems = new List<MonoBehaviour>();

	private RectTransform m_TooltipPlace;

	private Action<RectTransform> m_EnsureVisibleAction;

	private RectTransform Container
	{
		get
		{
			if (!m_Container)
			{
				return m_ForShipContainer;
			}
			return m_Container;
		}
	}

	public bool IsInSelectionProcess => m_AllItems.Contains((MonoBehaviour a) => a is ICareerPathItem careerPathItem && careerPathItem.IsSelectedForUI());

	public void SetViewParameters(RectTransform tooltipPlace, Action<RectTransform> ensureVisibleAction)
	{
		m_TooltipPlace = tooltipPlace;
		m_EnsureVisibleAction = ensureVisibleAction;
	}

	protected override void BindViewImplementation()
	{
		m_FirstRankDecoration.SetActive(base.ViewModel.Rank == 1);
		m_EmptyDummy.gameObject.SetActive(base.ViewModel.IsEmpty);
		AddDisposable(base.ViewModel.IsSelected.Subscribe(delegate(bool value)
		{
			m_EmptyDummy.SetActiveLayer(value ? "Active" : "Default");
		}));
		DrawItems();
	}

	protected override void DestroyViewImplementation()
	{
		Clear();
	}

	public void SetRotation(float angleDeg)
	{
		base.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
		foreach (MonoBehaviour allItem in m_AllItems)
		{
			allItem.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - angleDeg);
		}
	}

	private void DrawItems()
	{
		foreach (RankEntryFeatureItemVM feature in base.ViewModel.Features)
		{
			RankEntryFeatureItemCommonView widget = WidgetFactory.GetWidget(m_FeatureItemCommonView);
			widget.transform.SetParent(Container, worldPositionStays: false);
			widget.SetViewParameters(m_TooltipPlace, m_EnsureVisibleAction);
			widget.Bind(feature);
			m_AllItems.Add(widget);
		}
		foreach (RankEntrySelectionVM selection in base.ViewModel.Selections)
		{
			RankEntrySelectionItemCommonView widget2 = WidgetFactory.GetWidget(m_SelectionItemCommonView);
			widget2.transform.SetParent(Container, worldPositionStays: false);
			widget2.Initialize();
			widget2.SetViewParameters(m_TooltipPlace, m_EnsureVisibleAction);
			widget2.Bind(selection);
			m_AllItems.Add(widget2);
		}
		IEnumerable<IHasNeighbours> enumerable = m_AllItems.Cast<IHasNeighbours>();
		foreach (IHasNeighbours item in enumerable)
		{
			List<IFloatConsoleNavigationEntity> neighbours = enumerable.Where((IHasNeighbours i) => i != item).Cast<IFloatConsoleNavigationEntity>().ToList();
			item.SetNeighbours(neighbours);
		}
	}

	private void Clear()
	{
		m_AllItems.ForEach(WidgetFactory.DisposeWidget);
		m_AllItems.Clear();
	}

	public List<IFloatConsoleNavigationEntity> GetConsoleEntities()
	{
		return m_AllItems.Cast<IFloatConsoleNavigationEntity>().ToList();
	}

	public IFloatConsoleNavigationEntity TryGetItemByViewModel(IRankEntrySelectItem currentItemVM)
	{
		foreach (MonoBehaviour allItem in m_AllItems)
		{
			IRankEntrySelectItem rankEntrySelectItem = null;
			if (!(allItem is RankEntryFeatureItemCommonView rankEntryFeatureItemCommonView))
			{
				if (allItem is RankEntrySelectionItemCommonView rankEntrySelectionItemCommonView)
				{
					rankEntrySelectItem = rankEntrySelectionItemCommonView.GetViewModel() as IRankEntrySelectItem;
				}
			}
			else
			{
				rankEntrySelectItem = rankEntryFeatureItemCommonView.GetViewModel() as IRankEntrySelectItem;
			}
			if (rankEntrySelectItem == currentItemVM)
			{
				return allItem as IFloatConsoleNavigationEntity;
			}
		}
		return null;
	}
}
