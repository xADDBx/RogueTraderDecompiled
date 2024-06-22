using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.Utility.Attributes;
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
	[ConditionalShow("m_IsVoidshipEntry")]
	private RectTransform m_ForShipContainer;

	[Header("Size values")]
	[SerializeField]
	private float m_DefaultRadius = 50f;

	[SerializeField]
	private float m_SizeReduceCoeff = 0.9f;

	private readonly List<IRankEntryElement> m_AllItems = new List<IRankEntryElement>();

	private RectTransform m_TooltipPlace;

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

	public void SetViewParameters(RectTransform tooltipPlace)
	{
		m_TooltipPlace = tooltipPlace;
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
		for (int i = 0; i < m_AllItems.Count; i++)
		{
			m_AllItems[i].SetRotation(angleDeg, i > 0);
		}
	}

	public void SetHighlightStateToItemsWithKey(string key, bool state)
	{
		if (state)
		{
			m_AllItems.ForEach(delegate(IRankEntryElement i)
			{
				i.StartHighlight(key);
			});
		}
		else
		{
			m_AllItems.ForEach(delegate(IRankEntryElement i)
			{
				i.StopHighlight();
			});
		}
	}

	private void DrawItems()
	{
		int num = base.ViewModel.Selections.Count + base.ViewModel.Features.Count - 1;
		for (int num2 = base.ViewModel.Selections.Count - 1; num2 >= 0; num2--)
		{
			RankEntrySelectionVM viewModel = base.ViewModel.Selections[num2];
			RankEntrySelectionItemCommonView widget = WidgetFactory.GetWidget(m_SelectionItemCommonView);
			widget.transform.SetParent(Container, worldPositionStays: false);
			widget.SetViewParameters(m_TooltipPlace);
			widget.Bind(viewModel);
			m_AllItems.Add(widget);
			CorrectItemSize(widget.gameObject, num);
			num--;
		}
		for (int num3 = base.ViewModel.Features.Count - 1; num3 >= 0; num3--)
		{
			RankEntryFeatureItemVM viewModel2 = base.ViewModel.Features[num3];
			RankEntryFeatureItemCommonView widget2 = WidgetFactory.GetWidget(m_FeatureItemCommonView);
			widget2.transform.SetParent(Container, worldPositionStays: false);
			widget2.SetViewParameters(m_TooltipPlace);
			widget2.Bind(viewModel2);
			m_AllItems.Add(widget2);
			CorrectItemSize(widget2.gameObject, num);
			num--;
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
		m_AllItems.ForEach(delegate(IRankEntryElement item)
		{
			WidgetFactory.DisposeWidget(item.MonoBehaviour);
		});
		m_AllItems.Clear();
	}

	public List<IFloatConsoleNavigationEntity> GetConsoleEntities()
	{
		return m_AllItems.Cast<IFloatConsoleNavigationEntity>().ToList();
	}

	public IFloatConsoleNavigationEntity TryGetItemByViewModel(IRankEntrySelectItem currentItemVM)
	{
		foreach (IRankEntryElement allItem in m_AllItems)
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

	private void CorrectItemSize(GameObject item, float itemId)
	{
		RectTransform component = item.GetComponent<RectTransform>();
		if (!(component == null))
		{
			float num = (float)Math.Pow(m_SizeReduceCoeff, itemId);
			component.sizeDelta = new Vector2(m_DefaultRadius, m_DefaultRadius) * num;
		}
	}
}
