using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.Utils;
using Kingmaker.Code.UI.MVVM.View.Other;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
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

	[ShowIf("m_HasAdditionalCount")]
	[SerializeField]
	private TextMeshProUGUI m_AdditionalCount;

	[FormerlySerializedAs("m_HasDetailedContainers")]
	[SerializeField]
	private bool m_HasGroupContainers;

	[ShowIf("m_HasGroupContainers")]
	[SerializeField]
	private RectTransform m_DOTContainer;

	[ShowIf("m_HasGroupContainers")]
	[SerializeField]
	private RectTransform m_EnemyContainer;

	[ShowIf("m_HasGroupContainers")]
	[SerializeField]
	private RectTransform m_AllyContainer;

	[SerializeField]
	private bool m_HasAdditionalTriggerOverlay;

	[ShowIf("m_HasAdditionalTriggerOverlay")]
	[SerializeField]
	private Image m_AdditionalTriggerOverlay;

	[ShowIf("m_HasAdditionalTriggerOverlay")]
	[SerializeField]
	private Color m_TriggerPositiveColor;

	[ShowIf("m_HasAdditionalTriggerOverlay")]
	[SerializeField]
	private Color m_TriggerNeutralColor;

	[ShowIf("m_HasAdditionalTriggerOverlay")]
	[SerializeField]
	private Color m_TriggerNegativeColor;

	private readonly List<BuffPCView> m_BuffList = new List<BuffPCView>();

	private readonly List<BuffPCView> m_BuffVisibleList = new List<BuffPCView>();

	[SerializeField]
	private int m_BuffsLimit = 5;

	[SerializeField]
	private bool m_PartyCharacter;

	[Header("Important buffs")]
	[SerializeField]
	private bool m_HasImportantContainer;

	[SerializeField]
	private CanvasGroup m_ImportantBuffsContainer;

	[SerializeField]
	[ShowIf("m_HasImportantContainer")]
	private RectTransform m_ImportantBuffViewPlace1;

	[SerializeField]
	[ShowIf("m_HasImportantContainer")]
	private RectTransform m_ImportantBuffViewPlace2;

	[SerializeField]
	[ShowIf("m_HasImportantContainer")]
	private RectTransform m_ImportantBuffViewPlace3;

	private int m_ImportantBuffsCounter;

	private int m_BuffViewSize = 34;

	private readonly List<BuffVM> m_ActiveHunterMarks = new List<BuffVM>();

	[HideInInspector]
	public BoolReactiveProperty IsHovered = new BoolReactiveProperty();

	private bool m_NeedUpdate;

	private int m_preferredSizePC = 38;

	private int m_preferredSizeConsole = 40;

	private int m_preferredSizeDefault = 20;

	public bool HasBuffs => m_BuffVisibleList.Any();

	protected override void BindViewImplementation()
	{
		m_AdditionalContainer.gameObject.SetActive(value: false);
		DrawBuffs();
		AddDisposable(base.ViewModel.Buffs.ObserveAdd().ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			m_NeedUpdate = true;
		}));
		AddDisposable(base.ViewModel.Buffs.ObserveRemove().ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			m_NeedUpdate = true;
		}));
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.LateUpdateAsObservable().PauseDuringCutscene(), delegate
		{
			TryUpdateBuffs();
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.Buffs.ObserveReset(), delegate
		{
			Clear();
		}));
		AddDisposable(base.ViewModel.DrawUniqueBuff.Subscribe(DrawHuntThePreyBuff));
		AddDisposable(base.ViewModel.RedrawBuffs.Subscribe(RedrawHuntThePreyBuffs));
		AddDisposable(m_AdditionalTrigger.OnPointerClickAsObservable().Subscribe(delegate
		{
			SetAdditionalBuffsVisible(!m_AdditionalContainer.gameObject.activeSelf);
		}));
	}

	private void TryUpdateBuffs()
	{
		if ((RootUIContext.Instance.IsSurface || Game.Instance.IsSpaceCombat) && m_NeedUpdate)
		{
			DrawBuffs();
		}
	}

	protected override void DestroyViewImplementation()
	{
		Clear();
	}

	private void DrawBuffs()
	{
		if (m_MainContainer == null || m_AdditionalTrigger == null)
		{
			return;
		}
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
		if (m_ActiveHunterMarks.Count > 0)
		{
			BlueprintCareerPath careerPath = base.ViewModel.GetCareerPath();
			foreach (BuffVM activeHunterMark in m_ActiveHunterMarks)
			{
				DrawUniqueBuffs(careerPath, activeHunterMark, m_BuffVisibleList, cameraFocus: true);
			}
			if (m_ImportantBuffsContainer != null)
			{
				m_ImportantBuffsContainer.alpha = ((m_ImportantBuffsCounter > 0) ? 1 : 0);
			}
		}
		m_NeedUpdate = false;
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
		if (m_HasImportantContainer)
		{
			if (m_ImportantBuffsContainer != null)
			{
				m_ImportantBuffsContainer.alpha = 0f;
			}
			if (m_ImportantBuffViewPlace1 != null)
			{
				m_ImportantBuffViewPlace1.gameObject.SetActive(value: false);
			}
			if (m_ImportantBuffViewPlace2 != null)
			{
				m_ImportantBuffViewPlace2.gameObject.SetActive(value: false);
			}
			if (m_ImportantBuffViewPlace3 != null)
			{
				m_ImportantBuffViewPlace3.gameObject.SetActive(value: false);
			}
		}
		m_ImportantBuffsCounter = 0;
		BlueprintCareerPath careerPath = base.ViewModel.GetCareerPath();
		Dictionary<BlueprintBuff, List<BuffVM>> dictionary = new Dictionary<BlueprintBuff, List<BuffVM>>();
		foreach (BuffVM buff in buffs)
		{
			if (buff.Buff.Blueprint.NeedCollapseStack)
			{
				dictionary.TryAdd(buff.Buff.Blueprint, new List<BuffVM>());
			}
		}
		for (int i = 0; i < views.Count; i++)
		{
			IViewModel viewModel = views[i].GetViewModel();
			bool flag = false;
			foreach (BuffVM buff2 in buffs)
			{
				if (buff2 == viewModel)
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
			if (oldVM != null && buffs.Any((BuffVM b) => b.Buff.Blueprint == oldVM.Buff.Blueprint && b.Buff.Blueprint.NeedCollapseStack))
			{
				WidgetFactory.DisposeWidget(views[j]);
				views.RemoveAt(j);
				j--;
			}
		}
		foreach (KeyValuePair<BlueprintBuff, List<BuffVM>> kvp in dictionary)
		{
			IEnumerable<BuffVM> collection = buffs.Where((BuffVM b) => b.Buff.Blueprint == kvp.Key);
			if (dictionary.TryGetValue(kvp.Key, out var value))
			{
				value.AddRange(collection);
			}
		}
		if (m_HasImportantContainer)
		{
			foreach (BuffPCView view in views)
			{
				if (view.GetViewModel() is BuffVM vm && IsImportantBuff(careerPath, vm) && (!(view.transform.parent != m_ImportantBuffViewPlace1) || !(view.transform.parent != m_ImportantBuffViewPlace2) || !(view.transform.parent != m_ImportantBuffViewPlace3)))
				{
					RectTransform importantBuffVIew = GetImportantBuffVIew(m_ImportantBuffsCounter);
					if (importantBuffVIew == null)
					{
						break;
					}
					view.transform.SetParent(importantBuffVIew, worldPositionStays: false);
					view.transform.localPosition = Vector3.zero;
					importantBuffVIew.gameObject.SetActive(value: true);
					m_ImportantBuffsCounter++;
				}
			}
		}
		for (int k = 0; k < buffs.Count; k++)
		{
			if (buffs[k].Buff.Blueprint.NeedCollapseStack)
			{
				continue;
			}
			BuffVM buffVM = buffs[k];
			bool flag2 = false;
			foreach (BuffPCView view2 in views)
			{
				if (view2.GetViewModel() == buffVM)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				BuffPCView widget = WidgetFactory.GetWidget(m_BuffView);
				if (m_HasImportantContainer)
				{
					DrawUniqueBuffs(careerPath, buffVM, views);
				}
				widget.SetHoverProperty(IsHovered);
				widget.Bind(buffVM);
				RectTransform parent = (isVisibleBuffs ? m_MainContainer : GetAdditionalBuffParent(buffVM));
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
			BuffVM buffVM2 = item.Value.FirstOrDefault();
			if (buffVM2 != null)
			{
				BuffPCView widget2 = WidgetFactory.GetWidget(m_BuffView);
				widget2.SetHoverProperty(IsHovered);
				widget2.Bind(buffVM2);
				RectTransform parent2 = (isVisibleBuffs ? m_MainContainer : GetAdditionalBuffParent(buffVM2));
				widget2.transform.SetParent(parent2, worldPositionStays: false);
				if (isVisibleBuffs)
				{
					widget2.transform.SetSiblingIndex(buffs.IndexOf(buffVM2));
				}
				views.Add(widget2);
				if (m_HasImportantContainer)
				{
					DrawUniqueBuffs(careerPath, buffVM2, views);
				}
			}
		}
		if (m_ImportantBuffsContainer != null)
		{
			m_ImportantBuffsContainer.alpha = ((m_ImportantBuffsCounter > 0) ? 1 : 0);
		}
	}

	private void DrawUniqueBuffs(BlueprintCareerPath careerPath, BuffVM viewModelBuff, List<BuffPCView> views, bool cameraFocus = false)
	{
		if (IsImportantBuff(careerPath, viewModelBuff))
		{
			AddUniqueBuff(viewModelBuff, views, GetImportantBuffVIew(m_ImportantBuffsCounter), cameraFocus);
		}
	}

	private bool IsImportantBuff(BlueprintCareerPath careerPath, BuffVM vm)
	{
		if (careerPath?.ImportantCareerBuffs != null)
		{
			if (!careerPath.ImportantCareerBuffs.Any((BlueprintBuffReference uB) => uB.Get() == vm.Buff.Blueprint))
			{
				return vm.Buff.Blueprint.IsImportantBuff;
			}
			return true;
		}
		return vm.Buff.Blueprint.IsImportantBuff;
	}

	private void DrawHuntThePreyBuff(((BlueprintCareerPath, BuffVM), bool) data)
	{
		BuffVM item = data.Item1.Item2;
		if (!m_ActiveHunterMarks.Contains(item))
		{
			m_ActiveHunterMarks.Add(item);
		}
		DrawUniqueBuffs(data.Item1.Item1, item, m_BuffVisibleList, data.Item2);
		if (m_ImportantBuffsContainer != null)
		{
			m_ImportantBuffsContainer.alpha = ((m_ImportantBuffsCounter > 0) ? 1 : 0);
		}
	}

	private void RedrawHuntThePreyBuffs(List<BuffVM> huntThePreyMarks)
	{
		m_ActiveHunterMarks.Clear();
		m_ActiveHunterMarks.AddRange(huntThePreyMarks);
		DrawBuffs();
		if (m_ImportantBuffsContainer != null)
		{
			m_ImportantBuffsContainer.alpha = ((m_ImportantBuffsCounter > 0) ? 1 : 0);
		}
	}

	private void AddUniqueBuff(BuffVM viewModelBuff, List<BuffPCView> views, RectTransform place, bool cameraFocus)
	{
		if (place == null || m_ImportantBuffsContainer == null || m_ImportantBuffsCounter > 2)
		{
			return;
		}
		BuffPCView widget = WidgetFactory.GetWidget(m_BuffView);
		RectTransform component = widget.GetComponent<RectTransform>();
		component.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_BuffViewSize);
		component.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_BuffViewSize);
		component.anchorMin = new Vector2(0.5f, 0.5f);
		component.anchorMax = new Vector2(0.5f, 0.5f);
		component.SetLocalPositionAndRotation(new Vector3(0f, 0f, 0f), Quaternion.identity);
		widget.SetHoverProperty(IsHovered);
		widget.Bind(viewModelBuff);
		widget.transform.SetParent(place, worldPositionStays: false);
		widget.transform.parent.gameObject.SetActive(value: true);
		widget.transform.localPosition = new Vector3(0f, 0f, 0f);
		views.Add(widget);
		if (cameraFocus)
		{
			AddDisposable(widget.OnPointerClickAsObservable().Subscribe(delegate
			{
				CameraRig.Instance.ScrollTo(viewModelBuff.Buff.Owner.Position);
			}));
		}
		m_ImportantBuffsCounter++;
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
		m_ActiveHunterMarks.Clear();
	}

	private RectTransform GetImportantBuffVIew(int counter)
	{
		return counter switch
		{
			0 => m_ImportantBuffViewPlace1, 
			1 => m_ImportantBuffViewPlace2, 
			2 => m_ImportantBuffViewPlace3, 
			_ => null, 
		};
	}
}
