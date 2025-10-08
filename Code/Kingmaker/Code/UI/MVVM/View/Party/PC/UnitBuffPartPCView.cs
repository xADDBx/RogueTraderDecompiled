using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.Other;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Party.PC;

public class UnitBuffPartPCView : ViewBase<UnitBuffPartVM>
{
	[SerializeField]
	private BuffPCView m_BuffView;

	[SerializeField]
	private RectTransform m_MainContainer;

	[SerializeField]
	private RectTransform m_AdditionalContainer;

	[SerializeField]
	private OwlcatSelectable m_AdditionalTrigger;

	[SerializeField]
	private bool m_HasAdditionalCount;

	[ConditionalShow("m_HasAdditionalCount")]
	[SerializeField]
	private TextMeshProUGUI m_AdditionalCount;

	[FormerlySerializedAs("m_HasDetailedContainers")]
	[SerializeField]
	private bool m_HasGroupContainers;

	[ConditionalShow("m_HasGroupContainers")]
	[SerializeField]
	private RectTransform m_DOTContainer;

	[ConditionalShow("m_HasGroupContainers")]
	[SerializeField]
	private RectTransform m_EnemyContainer;

	[ConditionalShow("m_HasGroupContainers")]
	[SerializeField]
	private RectTransform m_AllyContainer;

	[SerializeField]
	private bool m_HasAdditionalTriggerOverlay;

	[ConditionalShow("m_HasAdditionalTriggerOverlay")]
	[SerializeField]
	private Image m_AdditionalTriggerOverlay;

	[ConditionalShow("m_HasAdditionalTriggerOverlay")]
	[SerializeField]
	private Color m_TriggerPositiveColor;

	[ConditionalShow("m_HasAdditionalTriggerOverlay")]
	[SerializeField]
	private Color m_TriggerNeutralColor;

	[ConditionalShow("m_HasAdditionalTriggerOverlay")]
	[SerializeField]
	private Color m_TriggerNegativeColor;

	private readonly List<BuffPCView> m_BuffList = new List<BuffPCView>();

	private readonly List<BuffPCView> m_BuffVisibleList = new List<BuffPCView>();

	[SerializeField]
	private int m_BuffsLimit = 5;

	[SerializeField]
	private bool m_PartyCharacter;

	[HideInInspector]
	public BoolReactiveProperty IsHovered = new BoolReactiveProperty();

	public bool HasBuffs => m_BuffVisibleList.Any();

	protected override void BindViewImplementation()
	{
		m_AdditionalContainer.gameObject.SetActive(value: false);
		DrawBuffs();
		AddDisposable(base.ViewModel.Buffs.ObserveAdd().Subscribe(delegate
		{
			DrawBuffs();
		}));
		AddDisposable(base.ViewModel.Buffs.ObserveRemove().Subscribe(delegate
		{
			DrawBuffs();
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.Buffs.ObserveReset(), delegate
		{
			Clear();
		}));
		AddDisposable(m_AdditionalTrigger.OnPointerClickAsObservable().Subscribe(delegate
		{
			SetAdditionalBuffsVisible(!m_AdditionalContainer.gameObject.activeSelf);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		Clear();
	}

	private void DrawBuffs()
	{
		if (!m_PartyCharacter)
		{
			if (base.ViewModel.EntityIsDeadOrUnconscious())
			{
				m_MainContainer.gameObject.SetActive(value: false);
				m_AdditionalTrigger.gameObject.SetActive(value: false);
				return;
			}
			m_MainContainer.gameObject.SetActive(value: true);
		}
		else
		{
			m_MainContainer.gameObject.SetActive(value: true);
		}
		base.ViewModel.SortBuffs();
		DrawVisibleBuffs();
		DrawAllBuffs();
	}

	private void DrawVisibleBuffs()
	{
		List<BuffVM> buffs = base.ViewModel.Buffs.Take(m_BuffsLimit).ToList();
		DrawBuffsInternal(buffs, m_BuffVisibleList, isVisibleBuffs: true);
		m_AdditionalTrigger.transform.SetAsLastSibling();
	}

	private void DrawAllBuffs()
	{
		bool flag = base.ViewModel.Buffs.Count > m_BuffsLimit;
		if (m_AdditionalTrigger.gameObject.activeSelf != flag)
		{
			m_AdditionalTrigger.gameObject.SetActive(flag);
		}
		if (!flag)
		{
			SetAdditionalBuffsVisible(visible: false);
			return;
		}
		DrawBuffsInternal(base.ViewModel.Buffs.ToList(), m_BuffList, isVisibleBuffs: false);
		if (m_HasAdditionalCount)
		{
			m_AdditionalCount.text = m_BuffList.Count.ToString();
		}
		if (m_HasAdditionalTriggerOverlay)
		{
			SetTriggerColor();
		}
	}

	private void DrawBuffsInternal(List<BuffVM> buffs, List<BuffPCView> views, bool isVisibleBuffs)
	{
		Dictionary<BlueprintBuff, List<BuffVM>> dictionary = new Dictionary<BlueprintBuff, List<BuffVM>>();
		for (int i = 0; i < views.Count; i++)
		{
			IViewModel viewModel = views[i].GetViewModel();
			bool flag = false;
			foreach (BuffVM buff in buffs)
			{
				if (buff == viewModel)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				WidgetFactory.DisposeWidget(views[i]);
				views.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < views.Count; j++)
		{
			BuffVM oldVM = views[j].GetViewModel() as BuffVM;
			if (oldVM != null && buffs.Where((BuffVM b) => b.Buff.Blueprint == oldVM.Buff.Blueprint && b.Buff.Blueprint.NeedCollapseStack).ToList().Count != 0)
			{
				WidgetFactory.DisposeWidget(views[j]);
				views.RemoveAt(j);
				j--;
			}
		}
		foreach (BuffVM buff2 in buffs)
		{
			if (buff2.Buff.Blueprint.NeedCollapseStack)
			{
				dictionary.TryAdd(buff2.Buff.Blueprint, new List<BuffVM>());
			}
		}
		foreach (KeyValuePair<BlueprintBuff, List<BuffVM>> kvp in dictionary)
		{
			List<BuffVM> collection = buffs.Where((BuffVM b) => b.Buff.Blueprint == kvp.Key).ToList();
			if (dictionary.TryGetValue(kvp.Key, out var value))
			{
				value.AddRange(collection);
			}
		}
		for (int k = 0; k < buffs.Count; k++)
		{
			if (buffs[k].Buff.Blueprint.NeedCollapseStack)
			{
				continue;
			}
			BuffVM buffVM2 = buffs[k];
			bool flag2 = false;
			foreach (BuffPCView view in views)
			{
				if (view.GetViewModel() == buffVM2)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				BuffPCView widget = WidgetFactory.GetWidget(m_BuffView);
				widget.SetHoverProperty(IsHovered);
				widget.Bind(buffVM2);
				RectTransform parent = (isVisibleBuffs ? m_MainContainer : GetAdditionalBuffParent(buffVM2));
				widget.transform.SetParent(parent, worldPositionStays: false);
				if (isVisibleBuffs)
				{
					widget.transform.SetSiblingIndex(k);
				}
				views.Add(widget);
			}
		}
		foreach (KeyValuePair<BlueprintBuff, List<BuffVM>> item in dictionary)
		{
			BuffVM viewModelBuff = item.Value.FirstOrDefault();
			if (viewModelBuff == null)
			{
				continue;
			}
			viewModelBuff.AdditionalSources.Clear();
			foreach (BuffVM item2 in item.Value.Where((BuffVM buffVM) => !viewModelBuff.AdditionalSources.Contains(buffVM)))
			{
				viewModelBuff.AdditionalSources.Add(item2);
			}
			BuffPCView widget2 = WidgetFactory.GetWidget(m_BuffView);
			widget2.SetHoverProperty(IsHovered);
			widget2.Bind(viewModelBuff);
			RectTransform parent2 = (isVisibleBuffs ? m_MainContainer : GetAdditionalBuffParent(viewModelBuff));
			widget2.transform.SetParent(parent2, worldPositionStays: false);
			if (isVisibleBuffs)
			{
				widget2.transform.SetSiblingIndex(buffs.IndexOf(viewModelBuff));
			}
			views.Add(widget2);
		}
	}

	private RectTransform GetAdditionalBuffParent(BuffVM vm)
	{
		if (!m_HasGroupContainers)
		{
			return m_AdditionalContainer;
		}
		return vm.Group switch
		{
			BuffUIGroup.Ally => m_AllyContainer, 
			BuffUIGroup.Enemy => m_EnemyContainer, 
			BuffUIGroup.DOT => m_DOTContainer, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void SetTriggerColor()
	{
		int num = base.ViewModel.Buffs.Count((BuffVM b) => b.Group != BuffUIGroup.Ally);
		Image additionalTriggerOverlay = m_AdditionalTriggerOverlay;
		Color color = ((num > 1) ? m_TriggerNegativeColor : ((num != 1) ? m_TriggerPositiveColor : m_TriggerNeutralColor));
		additionalTriggerOverlay.color = color;
	}

	public void SetAdditionalBuffsVisible(bool visible)
	{
		m_AdditionalContainer.gameObject.SetActive(visible);
	}

	private void Clear()
	{
		m_BuffList.ForEach(WidgetFactory.DisposeWidget);
		m_BuffList.Clear();
		m_BuffVisibleList.ForEach(WidgetFactory.DisposeWidget);
		m_BuffVisibleList.Clear();
	}
}
