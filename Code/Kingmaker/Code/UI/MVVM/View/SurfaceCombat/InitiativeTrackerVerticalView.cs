using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public abstract class InitiativeTrackerVerticalView : InitiativeTrackerView, IHideUIWhileActionCameraHandler, ISubscriber
{
	private ReactiveProperty<bool> m_HasScroll = new ReactiveProperty<bool>();

	private int m_HasScrollPaddingForVirtualList;

	[SerializeField]
	private RectTransform m_VirtualListViewport;

	[SerializeField]
	protected TextMeshProUGUI m_StateValue;

	private IDisposable m_ScrollBackTask;

	[SerializeField]
	protected SurfaceCombatUnitOrderVerticalView m_CurrentUnit;

	[SerializeField]
	[Header("Condition Part")]
	protected GameObject m_CombatConditionContainer;

	private Vector2 m_BaseSize;

	private float m_FixedNormalizedPosition;

	private bool CanShow
	{
		get
		{
			if (Game.Instance.CurrentMode != GameModeType.Dialog)
			{
				return Game.Instance.CurrentMode != GameModeType.Cutscene;
			}
			return false;
		}
	}

	protected override void OnInitialize()
	{
		VirtualList.Padding.right = 0;
		m_BaseSize = OrderContainer.rect.size;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_HasScroll.Value = false;
		AddDisposable(base.ViewModel.NeedShowEtudeCounter.Subscribe(m_CombatConditionContainer.SetActive));
	}

	protected override void OnUnitHovered()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		InitiativeTrackerUnitVM value = base.ViewModel.CurrentUnit.Value;
		if (value == null || !value.Unit.IsPlayerFaction || base.ViewModel.HoveredUnit.Value == null)
		{
			return;
		}
		ITurnVirtualItemData turnVirtualItemData = VirtualEntries.FirstOrDefault((ITurnVirtualItemData data) => data.ViewModel == base.ViewModel.HoveredUnit.Value);
		if (turnVirtualItemData != null && !VirtualList.IsVisible(turnVirtualItemData))
		{
			VirtualList.ScrollTo(turnVirtualItemData);
		}
		else if (base.ViewModel.SquadLeaderUnit.Value != null)
		{
			ITurnVirtualItemData turnVirtualItemData2 = VirtualEntries.FirstOrDefault((ITurnVirtualItemData data) => data.ViewModel == base.ViewModel.SquadLeaderUnit.Value);
			if (turnVirtualItemData2 != null && !VirtualList.IsVisible(turnVirtualItemData2))
			{
				VirtualList.ScrollTo(turnVirtualItemData2);
			}
		}
	}

	protected override void PrepareInitiativeTracker()
	{
		VirtualEntries.Clear();
		TrackerContainer.sizeDelta = new Vector2(TrackerContainer.sizeDelta.x, m_BaseSize.y);
		m_VirtualListViewport.sizeDelta = new Vector2(m_VirtualListViewport.sizeDelta.x, m_BaseSize.y);
		VirtualList.Content.sizeDelta = new Vector2(VirtualList.Content.sizeDelta.x, m_BaseSize.y);
		VirtualList.Content.anchoredPosition = Vector2.zero;
	}

	protected override void UpdateUnits()
	{
		VirtualEntries.Clear();
		List<InitiativeTrackerUnitVM> units = base.ViewModel.Units;
		if (units.Count <= 0)
		{
			Hide();
			return;
		}
		Show();
		Vector2 nextPosition = Vector2.zero;
		for (int num = units.Count - 1; num >= 0; num--)
		{
			if (num == base.ViewModel.RoundIndex)
			{
				nextPosition = AddRoundToVirtualDataView(nextPosition);
			}
			if (units[num].IsCurrent.Value)
			{
				m_CurrentUnit.Bind(units[num]);
			}
			else if (units[num].IsEnemy.Value || units[num].IsNeutral.Value)
			{
				if (units[num].IsInSquad.Value)
				{
					if (units[num].IsSquadLeader.Value || units[num].NeedToShow.Value)
					{
						nextPosition = AddUnitToVirtualDataView(units[num], nextPosition);
					}
				}
				else
				{
					nextPosition = AddUnitToVirtualDataView(units[num], nextPosition);
				}
			}
			else
			{
				nextPosition = AddUnitToVirtualDataView(units[num], nextPosition);
			}
		}
		m_FixedNormalizedPosition = VirtualList.ScrollRect.verticalNormalizedPosition;
		float y = Mathf.Min(VirtualList.GetContentSize(VirtualEntries).y, OrderContainer.rect.height);
		Sequence sequence = DOTween.Sequence().Pause().SetUpdate(isIndependentUpdate: true);
		Tweener t = m_VirtualListViewport.DOSizeDelta(new Vector2(m_VirtualListViewport.sizeDelta.x, y), (!base.ViewModel.SkipScroll) ? VirtualList.m_AnimationTime : (VirtualList.m_AnimationTime - 0.1f)).Pause();
		sequence.Append(t);
		if (!base.ViewModel.SkipScroll)
		{
			VirtualList.UpdateData(VirtualEntries, sequence, ScrollBasePosition.Bottom, delegate
			{
				VirtualList.ScrollRect.ScrollToBottom();
			});
		}
		else
		{
			VirtualList.UpdateData(VirtualEntries, sequence, ScrollBasePosition.None, delegate
			{
				VirtualList.ScrollRect.verticalNormalizedPosition = m_FixedNormalizedPosition;
			});
		}
		base.ViewModel.SkipScroll = false;
		List<InitiativeTrackerUnitVM> list = new List<InitiativeTrackerUnitVM>();
		foreach (InitiativeTrackerUnitVM unit in units)
		{
			if (unit.IsEnemy.Value && !list.Contains((InitiativeTrackerUnitVM uniqUnit) => uniqUnit.Unit == unit.Unit))
			{
				list.Add(unit);
			}
		}
		m_StateValue.text = list.Count().ToString();
	}

	private IEnumerator UpdateNormalizedPositionCo()
	{
		float time = 2f;
		while (time > 0f)
		{
			yield return null;
			time -= Time.deltaTime;
			VirtualList.ScrollRect.verticalNormalizedPosition = m_FixedNormalizedPosition;
		}
	}

	private Vector2 AddUnitToVirtualDataView(InitiativeTrackerUnitVM unitVM, Vector2 nextPosition)
	{
		TurnVirtualUnitData turnVirtualUnitData = new TurnVirtualUnitData
		{
			ViewModel = unitVM
		};
		Vector2 size = new Vector2(CombatUnitPrefab.RectTransform.rect.size.x, CombatUnitPrefab.SizeWithPortrait.y);
		turnVirtualUnitData.SetViewParameters(nextPosition, size);
		VirtualEntries.Add(turnVirtualUnitData);
		nextPosition.y += size.y + VirtualList.Spacing.y;
		return nextPosition;
	}

	private Vector2 AddRoundToVirtualDataView(Vector2 nextPosition)
	{
		Vector2 size = new Vector2(CombatUnitPrefab.RectTransform.rect.size.x, CombatUnitPrefab.SizeRound.y);
		TurnVirtualUnitData turnVirtualUnitData = new TurnVirtualUnitData
		{
			ViewModel = base.ViewModel.RoundVM
		};
		turnVirtualUnitData.SetViewParameters(nextPosition, size);
		VirtualEntries.Add(turnVirtualUnitData);
		nextPosition.y += size.y + VirtualList.Spacing.y;
		return nextPosition;
	}

	protected override void Show()
	{
		if (CanShow)
		{
			base.Show();
		}
	}

	protected override void Hide()
	{
		VirtualList.Content.sizeDelta = new Vector2(VirtualList.Content.sizeDelta.x, m_BaseSize.y);
		VirtualList.Content.anchoredPosition = Vector2.zero;
		base.Hide();
	}

	public void HandleHideUI()
	{
		base.gameObject.SetActive(value: false);
	}

	public void HandleShowUI()
	{
		DelayedInvoker.InvokeInTime(delegate
		{
			base.gameObject.SetActive(value: true);
		}, 2.5f);
	}
}
