using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.GroupChanger;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.GroupChanger;

public class GroupChangerBaseView : ViewBase<GroupChangerVM>, IGameModeHandler, ISubscriber, IInitializable
{
	[Header("Remote Content")]
	[SerializeField]
	private GroupChangerCharacterBaseView m_RemoteCharacterView;

	[SerializeField]
	private RectTransform m_RemoteContainer;

	[Header("Party Content")]
	[SerializeField]
	private GroupChangerCharacterBaseView m_PartyCharacterView;

	[SerializeField]
	private RectTransform m_PartyContainer;

	[Header("Window")]
	[SerializeField]
	private WindowAnimator m_WindowAnimator;

	[Header("ScrollBar")]
	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	private readonly List<GroupChangerCharacterBaseView> m_PartyCharacterViews = new List<GroupChangerCharacterBaseView>();

	protected readonly List<GroupChangerCharacterBaseView> RemoteCharacterViews = new List<GroupChangerCharacterBaseView>();

	public void Initialize()
	{
		m_WindowAnimator.Initialize();
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		m_WindowAnimator.AppearAnimation();
		UISounds.Instance.Sounds.GroupChanger.GroupChangerOpen.Play();
		ScrollToTop();
		foreach (GroupChangerCharacterVM item in base.ViewModel.PartyCharacter.Concat(base.ViewModel.RemoteCharacter))
		{
			CreateCharacterView(item, m_PartyCharacterView, AddToParty);
			CreateCharacterView(item, m_RemoteCharacterView, AddToReserve);
		}
		base.gameObject.SetActive(value: true);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.GroupChanger);
		});
		AddDisposable(base.ViewModel.IsMainCharacter.Subscribe(CheckCoopButtons));
		CheckCoopButtons(base.ViewModel.IsMainCharacter.Value);
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		m_PartyCharacterViews.ForEach(WidgetFactory.DisposeWidget);
		m_PartyCharacterViews.Clear();
		RemoteCharacterViews.ForEach(WidgetFactory.DisposeWidget);
		RemoteCharacterViews.Clear();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.GroupChanger);
		});
		base.gameObject.SetActive(value: false);
	}

	private void CheckCoopButtons(bool isMainCharacter)
	{
		CheckCoopButtonsImpl(isMainCharacter);
	}

	protected virtual void CheckCoopButtonsImpl(bool isMainCharacter)
	{
	}

	protected void OnAccept()
	{
		if (UINetUtility.IsControlMainCharacterWithWarning())
		{
			Game.Instance.GameCommandQueue.AcceptChangeGroup(base.ViewModel.PartyCharacterRef.ToList(), base.ViewModel.RemoteCharacterRef.ToList(), base.ViewModel.RequiredCharactersRef.ToList(), base.ViewModel.IsCapital, base.ViewModel is GroupChangerCommonVM);
		}
	}

	protected void OnCancel()
	{
		if (UINetUtility.IsControlMainCharacterWithWarning() && base.ViewModel.CloseCondition())
		{
			m_WindowAnimator.DisappearAnimation(delegate
			{
				Game.Instance.GameCommandQueue.AddCommand(new CloseChangeGroupGameCommand());
			});
			UISounds.Instance.Sounds.GroupChanger.GroupChangerClose.Play();
		}
	}

	protected void OnSelectedClick()
	{
		base.ViewModel.OnSelectedClick();
	}

	private void CreateCharacterView(GroupChangerCharacterVM vm, GroupChangerCharacterBaseView prefab, Action<GroupChangerCharacterBaseView> addMethod)
	{
		GroupChangerCharacterBaseView widget = WidgetFactory.GetWidget(prefab);
		widget.Bind(vm);
		addMethod(widget);
	}

	private void AddToParty(GroupChangerCharacterBaseView pcView)
	{
		m_PartyCharacterViews.Add(pcView);
		pcView.transform.SetParent(m_PartyContainer, worldPositionStays: false);
	}

	private void AddToReserve(GroupChangerCharacterBaseView pcView)
	{
		RemoteCharacterViews.Add(pcView);
		pcView.transform.SetParent(m_RemoteContainer, worldPositionStays: false);
	}

	private void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!(gameMode != GameModeType.Dialog))
		{
			OnCancel();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
