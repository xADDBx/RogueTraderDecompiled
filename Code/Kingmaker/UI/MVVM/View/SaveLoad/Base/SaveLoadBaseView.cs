using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SaveLoad.Base;

public class SaveLoadBaseView : ViewBase<SaveLoadVM>, IInitializable
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private SaveLoadMenuBaseView m_Menu;

	[SerializeField]
	protected SaveSlotBaseView m_NewSaveSlotBaseView;

	[SerializeField]
	protected SaveSlotCollectionVirtualBaseView m_SlotCollectionView;

	[SerializeField]
	private SaveSlotBaseView m_DetailedSaveSlotView;

	[SerializeField]
	private SaveFullScreenshotBaseView m_FullScreenshotBaseView;

	[SerializeField]
	private RectTransform m_WaitingForSaveListUpdatingAnimation;

	[SerializeField]
	private RectTransform m_SavesRect;

	[SerializeField]
	private TextMeshProUGUI m_YouHaveNoSavesLabel;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	private bool m_IsInit;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected InputLayer InputLayer;

	public const string InputLayerContextName = "SaveLoad";

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_Menu.Initialize();
			m_FadeAnimator.Initialize();
			m_FullScreenshotBaseView.Initialize();
			m_NewSaveSlotBaseView.Initialize();
			m_YouHaveNoSavesLabel.Or(null)?.transform.parent.gameObject.SetActive(value: false);
			DoInitialize();
			m_IsInit = true;
		}
	}

	protected virtual void DoInitialize()
	{
	}

	protected override void BindViewImplementation()
	{
		m_FadeAnimator.AppearAnimation();
		m_Menu.Bind(base.ViewModel.SaveLoadMenuVM);
		m_SlotCollectionView.Bind(base.ViewModel.SaveSlotCollectionVm);
		m_NewSaveSlotBaseView.Bind(base.ViewModel.NewSaveSlotVM);
		m_YouHaveNoSavesLabel.text = UIStrings.Instance.SaveLoadTexts.EmptySaveListHint;
		AddDisposable(base.ViewModel.SelectedSaveSlot.Subscribe(delegate(SaveSlotVM value)
		{
			m_DetailedSaveSlotView.Bind(value);
			ScrollToTop();
		}));
		AddDisposable(base.ViewModel.SaveFullScreenshot.Subscribe(m_FullScreenshotBaseView.Bind));
		AddDisposable(base.ViewModel.SaveListUpdating.Subscribe(SaveListUpdatingAnimation));
		AddDisposable(m_SlotCollectionView.Saves.ObserveCountChanged().Subscribe(delegate(int count)
		{
			m_YouHaveNoSavesLabel.Or(null)?.transform.parent.gameObject.SetActive(count <= 0 && !base.ViewModel.SaveListUpdating.Value);
		}));
		SaveListUpdatingAnimation(state: true);
		Game.Instance.RequestPauseUi(isPaused: true);
		UISounds.Instance.Sounds.LocalMap.MapOpen.Play();
		DelayedInvoker.InvokeInFrames(delegate
		{
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.SaveLoad);
			});
		}, 1);
		CreateInput();
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Sounds.LocalMap.MapClose.Play();
		m_FadeAnimator.DisappearAnimation();
		SaveListUpdatingAnimation(state: false);
		Game.Instance.RequestPauseUi(isPaused: false);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.SaveLoad);
		});
	}

	private void CreateInput()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_NavigationBehaviour.AddEntityVertical(m_NewSaveSlotBaseView);
		m_NavigationBehaviour.AddEntityVertical(m_SlotCollectionView.NavigationBehaviour);
		InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "SaveLoad"
		}, null, leftStick: true, rightStick: false, null, new List<NavigationInputEventTypeConfig>
		{
			new NavigationInputEventTypeConfig
			{
				Action = 10,
				InputActionEventType = InputActionEventType.ButtonJustReleased
			}
		});
		AddDisposable(m_SlotCollectionView.AttachedFirstValidView.Subscribe(FocusOnFirstValidSaveSlot));
		AddDisposable(base.ViewModel.Mode.Subscribe(delegate
		{
			FocusOnFirstValidSaveSlot();
		}));
		AddDisposable(base.ViewModel.SaveListUpdating.Subscribe(delegate(bool value)
		{
			if (!value)
			{
				if (!m_NavigationBehaviour.Entities.Any())
				{
					m_NavigationBehaviour.AddEntityVertical(m_NewSaveSlotBaseView);
					m_NavigationBehaviour.AddEntityVertical(m_SlotCollectionView.NavigationBehaviour);
				}
				FocusOnFirstValidSaveSlot();
			}
			else
			{
				m_SlotCollectionView.Or(null)?.NavigationBehaviour?.UnFocusCurrentEntity();
				m_NavigationBehaviour?.UnFocusCurrentEntity();
				m_NavigationBehaviour?.Clear();
			}
		}));
		CreateInputImpl(InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(InputLayer));
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
	}

	private void FocusOnFirstValidSaveSlot()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			if (base.ViewModel.Mode.Value == SaveLoadMode.Save)
			{
				m_NavigationBehaviour.FocusOnEntityManual(m_NewSaveSlotBaseView);
				m_SlotCollectionView.NavigationBehaviour.ResetCurrentEntity();
			}
			else
			{
				foreach (IConsoleEntity entity in m_SlotCollectionView.NavigationBehaviour.Entities)
				{
					if (entity is VirtualListElement { View: SaveSlotBaseView view } && view.IsValid())
					{
						m_SlotCollectionView.NavigationBehaviour.FocusOnEntityManual(entity);
						m_NavigationBehaviour.FocusOnEntityManual(m_SlotCollectionView.NavigationBehaviour);
						return;
					}
				}
				m_SlotCollectionView.NavigationBehaviour.FocusOnFirstValidEntity();
				m_NavigationBehaviour.FocusOnEntityManual(m_SlotCollectionView.NavigationBehaviour);
			}
		}, 1);
	}

	private void SaveListUpdatingAnimation(bool state)
	{
		m_SavesRect.Or(null)?.gameObject.SetActive(!state);
		m_WaitingForSaveListUpdatingAnimation.Or(null)?.gameObject.SetActive(state);
		m_YouHaveNoSavesLabel.Or(null)?.transform.parent.gameObject.SetActive(!m_SlotCollectionView.Saves.Any() && !state);
	}

	public void ScrollToTop()
	{
		m_ScrollRect.ScrollToTop();
	}

	protected void SelectPrev()
	{
		base.ViewModel.SaveLoadMenuVM.SelectionGroup.SelectPrevValidEntity();
		m_SlotCollectionView.ScrollToTop();
	}

	protected void SelectNext()
	{
		base.ViewModel.SaveLoadMenuVM.SelectionGroup.SelectNextValidEntity();
		m_SlotCollectionView.ScrollToTop();
	}
}
