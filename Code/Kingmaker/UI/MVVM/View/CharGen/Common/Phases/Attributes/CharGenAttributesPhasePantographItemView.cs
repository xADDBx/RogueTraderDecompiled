using System.Collections.Generic;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Stats;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Attributes;

public class CharGenAttributesPhasePantographItemView : SelectionGroupEntityView<CharGenAttributesItemVM>, IWidgetView
{
	[SerializeField]
	private ScrambledTMP m_DisplayName;

	[SerializeField]
	private CharGenAttributesPhasePantographItemRankWidget m_RankWidget;

	[SerializeField]
	private RectTransform m_RanksContainer;

	[SerializeField]
	private ScrambledTMP m_StatValue;

	[SerializeField]
	private ScrambledTMP m_ShortLabel;

	[SerializeField]
	private Image m_StatImage;

	[SerializeField]
	private GameObject m_RecommendedMark;

	private readonly List<CharGenAttributesPhasePantographItemRankWidget> m_RankWidgets = new List<CharGenAttributesPhasePantographItemRankWidget>();

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateRankWidgets();
		m_DisplayName.SetText(string.Empty, base.ViewModel.DisplayName);
		AddDisposable(base.ViewModel.StatValue.Subscribe(delegate(int value)
		{
			m_StatValue.SetText(m_StatValue.Text, value.ToString());
		}));
		AddDisposable(base.ViewModel.StatRanks.Subscribe(delegate(int value)
		{
			for (int i = 0; i < m_RankWidgets.Count; i++)
			{
				m_RankWidgets[i].SetState(i < value);
			}
		}));
		if ((bool)m_RecommendedMark)
		{
			AddDisposable(base.ViewModel.IsRecommended.Subscribe(delegate(bool value)
			{
				m_RecommendedMark.SetActive(value);
			}));
		}
		m_ShortLabel.SetText(m_ShortLabel.Text, UIUtilityTexts.GetStatShortName(base.ViewModel.StatType));
	}

	private void CreateRankWidgets()
	{
		if (m_RankWidgets.Count <= 0)
		{
			for (int i = 0; i < 2; i++)
			{
				CharGenAttributesPhasePantographItemRankWidget item = Object.Instantiate(m_RankWidget, m_RanksContainer, worldPositionStays: false);
				m_RankWidgets.Add(item);
			}
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharGenAttributesItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharGenAttributesItemVM;
	}
}
