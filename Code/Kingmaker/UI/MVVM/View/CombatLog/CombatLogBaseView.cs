using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.Log;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.MVVM.VM.CombatLog;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CombatLog;

public abstract class CombatLogBaseView : ViewBase<CombatLogVM>, IResizeElement, IGameModeHandler, ISubscriber, IHideUIWhileActionCameraHandler, ISettingsFontSizeUIHandler
{
	[SerializeField]
	protected VirtualListVertical m_VirtualList;

	[SerializeField]
	protected CombatLogItemBaseView m_LogItemView;

	[SerializeField]
	protected CombatLogSeparatorView m_LogSeparatorView;

	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("PinnedContainer")]
	[SerializeField]
	protected RectTransform m_PinnedContainer;

	[SerializeField]
	protected MoveAnimator m_ContainerMoveAnimator;

	[SerializeField]
	protected FadeAnimator m_ContainerFadeAnimator;

	[Header("Channels")]
	[Space]
	[SerializeField]
	protected List<CombatLogToggleWithCustomHint> m_Toggles;

	[SerializeField]
	protected List<TextMeshProUGUI> m_ToggleTexts;

	[SerializeField]
	protected OwlcatToggleGroup m_ToggleGroup;

	protected readonly BoolReactiveProperty IsPinned = new BoolReactiveProperty(initialValue: false);

	protected int CurrentSelectedIndex;

	protected bool CombatLogVisible = true;

	protected readonly List<Tweener> StartedTweeners = new List<Tweener>();

	public virtual void Initialize()
	{
		m_VirtualList.Initialize(new VirtualListElementTemplate<CombatLogItemVM>(m_LogItemView), new VirtualListElementTemplate<CombatLogSeparatorVM>(m_LogSeparatorView));
		SetContainerState(state: false);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(m_VirtualList.Subscribe(base.ViewModel.Items));
		AddDisposable((from list in base.ViewModel.Items.ObserveAdd().Buffer(MainThreadDispatcher.LateUpdateAsObservable())
			select list.Count into count
			where count > 0
			select count).Subscribe(OnItemsAdded));
		AddDisposable(base.ViewModel.CurrentChannel.Subscribe(OnChannelUpdated));
		AddDisposable(IsPinned.Subscribe(SwitchPinnedState));
		AddDisposable(EventBus.Subscribe(this));
		base.gameObject.SetActive(value: true);
	}

	protected override void DestroyViewImplementation()
	{
		StartedTweeners.ForEach(delegate(Tweener t)
		{
			t.Kill();
		});
		StartedTweeners.Clear();
	}

	private void OnItemsAdded(int count)
	{
		if (count >= base.ViewModel.Items.Count || base.ViewModel.Items[base.ViewModel.Items.Count - count - 1].HasView)
		{
			m_VirtualList.ScrollController.ForceScrollToBottom();
		}
	}

	protected void OnChannelUpdated(CombatLogChannel channel)
	{
		if (IsPinned.Value)
		{
			CombatLogBaseVM lastVisibleItemForChannel = base.ViewModel.GetLastVisibleItemForChannel(channel);
			if (lastVisibleItemForChannel != null)
			{
				m_VirtualList.ScrollController.ForceScrollToElement(lastVisibleItemForChannel);
			}
			else
			{
				m_VirtualList.ScrollController.ForceScrollToBottom();
			}
		}
	}

	public void SetSizeDelta(Vector2 size)
	{
		SetSizeDeltaImpl(size);
		m_VirtualList.ScrollController.ForceScrollToBottom();
	}

	protected virtual void SetSizeDeltaImpl(Vector2 size)
	{
		m_PinnedContainer.sizeDelta = size;
		Game.Instance.Player.UISettings.LogSize = size;
	}

	public Vector2 GetSize()
	{
		return m_PinnedContainer.sizeDelta;
	}

	public RectTransform GetTransform()
	{
		return m_PinnedContainer;
	}

	private void Show()
	{
		m_Animator.AppearAnimation(delegate
		{
			CombatLogVisible = true;
		});
	}

	private void Hide()
	{
		m_Animator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
			CombatLogVisible = false;
		});
	}

	public virtual void SwitchPinnedState(bool pinned)
	{
		if (pinned)
		{
			OnChannelUpdated(base.ViewModel.CurrentChannel.Value);
			UISounds.Instance.Sounds.CombatLog.CombatLogOpen.Play();
		}
		else
		{
			SetContainerState(state: false);
			UISounds.Instance.Sounds.CombatLog.CombatLogClose.Play();
		}
	}

	protected void SetContainerState(bool state)
	{
		if (state)
		{
			m_ContainerMoveAnimator.Or(null)?.AppearAnimation();
			m_ContainerFadeAnimator.Or(null)?.AppearAnimation();
		}
		else
		{
			m_ContainerMoveAnimator.Or(null)?.DisappearAnimation();
			m_ContainerFadeAnimator.Or(null)?.DisappearAnimation();
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.Dialog)
		{
			Hide();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.Dialog)
		{
			Show();
		}
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

	public void HandleChangeFontSizeSettings(float size)
	{
		m_VirtualList.Elements.ForEach(delegate(VirtualListElement e)
		{
			(e.View as CombatLogItemBaseView).Or(null)?.UpdateTextSize(size);
		});
	}
}
