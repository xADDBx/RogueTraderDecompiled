using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipRankEntryItemPCView : ViewBase<CareerPathRankEntryVM>
{
	[SerializeField]
	private OwlcatMultiSelectable m_ContainerFrame;

	[FormerlySerializedAs("m_FeatureItemPCView")]
	[SerializeField]
	private RankEntryFeatureItemCommonView m_FeatureItemCommonView;

	[FormerlySerializedAs("m_SelectionItemPCView")]
	[SerializeField]
	private RankEntrySelectionItemCommonView m_SelectionItemCommonView;

	private readonly List<RankEntryFeatureItemCommonView> m_Features = new List<RankEntryFeatureItemCommonView>();

	private readonly List<RankEntrySelectionItemCommonView> m_Selections = new List<RankEntrySelectionItemCommonView>();

	private RectTransform m_Container;

	protected override void BindViewImplementation()
	{
		m_ContainerFrame.gameObject.SetActive(base.ViewModel.Features.Count + base.ViewModel.Selections.Count > 1);
		AddDisposable(base.ViewModel.IsFirstSelectable.Subscribe(delegate(bool value)
		{
			m_ContainerFrame.SetActiveLayer(value ? "Active" : "Default");
		}));
		DrawFeatures();
		DrawSelections();
	}

	protected override void DestroyViewImplementation()
	{
		Clear();
	}

	private void DrawFeatures()
	{
		foreach (RankEntryFeatureItemVM feature in base.ViewModel.Features)
		{
			RankEntryFeatureItemCommonView widget = WidgetFactory.GetWidget(m_FeatureItemCommonView);
			widget.transform.SetParent(m_Container, worldPositionStays: false);
			widget.Bind(feature);
			m_Features.Add(widget);
		}
	}

	private void DrawSelections()
	{
		foreach (RankEntrySelectionVM selection in base.ViewModel.Selections)
		{
			RankEntrySelectionItemCommonView widget = WidgetFactory.GetWidget(m_SelectionItemCommonView);
			widget.transform.SetParent(m_Container, worldPositionStays: false);
			widget.Initialize();
			widget.Bind(selection);
			m_Selections.Add(widget);
		}
	}

	private void Clear()
	{
		m_Features.ForEach(WidgetFactory.DisposeWidget);
		m_Features.Clear();
		m_Selections.ForEach(WidgetFactory.DisposeWidget);
		m_Selections.Clear();
	}
}
