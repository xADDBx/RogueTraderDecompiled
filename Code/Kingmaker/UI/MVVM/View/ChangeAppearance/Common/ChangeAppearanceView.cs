using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.DollRoom;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.ChangeAppearance;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.CharGen.Common;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance;
using Kingmaker.UI.MVVM.View.CharGen.Common.Portrait;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ChangeAppearance.Common;

public class ChangeAppearanceView : ViewBase<ChangeAppearanceVM>, IInitializable, IFullScreenUIHandler, ISubscriber
{
	public static readonly string InputLayerContextName = "Appearance";

	[SerializeField]
	protected CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_Header;

	[SerializeField]
	protected CharGenAppearancePhaseDetailedView m_AppearancePhaseDetailedView;

	[Header("Portrait")]
	[SerializeField]
	private CharGenPortraitView m_PortraitView;

	[SerializeField]
	private CharGenPortraitView m_PortraitFullView;

	[Header("Pantograph")]
	[SerializeField]
	private PantographView m_PantographView;

	[SerializeField]
	private RectTransform m_PantographPosition;

	[Header("Doll")]
	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[SerializeField]
	private FadeAnimator m_CharacterDollTexture;

	[SerializeField]
	private RectTransform m_DollTransform;

	[SerializeField]
	private DollPosition[] m_DollPositions;

	[SerializeField]
	private PaperHints m_PaperHints;

	protected InputLayer InputLayer;

	protected GridConsoleNavigationBehaviour Navigation;

	private CharGenDollRoom CharacterRoom => UIDollRooms.Instance.Or(null)?.CharGenDollRoom;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_AppearancePhaseDetailedView.Initialize();
		m_AppearancePhaseDetailedView.SetPaperHints(m_PaperHints);
		m_PortraitView.Initialize(SetFullPortraitVisible);
		m_PortraitFullView.Initialize();
		m_CharacterDollTexture.Initialize();
		m_Header.text = UIStrings.Instance.CharGen.GetPhaseName(CharGenPhaseType.Appearance);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		CharacterRoom.Initialize(m_CharacterController);
		AddDisposable(m_PantographView);
		if (m_PantographPosition != null)
		{
			m_PantographView.transform.position = m_PantographPosition.position;
		}
		m_PantographView.Show();
		m_PortraitView.SetVisibility(base.ViewModel.CharGenAppearancePhaseVM.HasPortrait);
		UpdateDollRooms();
		base.ViewModel.CharGenAppearancePhaseVM.BeginDetailedView();
		TooltipHelper.HideInfo();
		base.ViewModel.HideVisualSettings();
		m_AppearancePhaseDetailedView.Bind(base.ViewModel.CharGenAppearancePhaseVM);
		AddDisposable(base.ViewModel.PortraitVM.Subscribe(m_PortraitView.Bind));
		AddDisposable(base.ViewModel.CharGenContext.CurrentUnit.Subscribe(delegate
		{
			SetDoll();
		}));
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Chargen);
		});
		AddDisposable(EventBus.Subscribe(this));
		CreateNavigation();
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(delegate
		{
			m_AppearancePhaseDetailedView.SetPaperHints(m_PaperHints);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
		CharacterRoom.Hide();
		DestroyInputLayer();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Chargen);
		});
	}

	private void DestroyInputLayer()
	{
		if (InputLayer != null)
		{
			GamePad.Instance.PopLayer(InputLayer);
			InputLayer = null;
		}
	}

	private void CreateNavigation()
	{
		AddDisposable(Navigation = new GridConsoleNavigationBehaviour());
		CreateInputLayer();
	}

	private void CreateInputLayer()
	{
		InputLayer = GetInputLayer();
		GamePad.Instance.PushLayer(InputLayer);
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = Navigation.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		CreateInputImpl(inputLayer, base.ViewModel?.IsMainCharacter ?? new BoolReactiveProperty(UINetUtility.IsControlMainCharacter()));
		return inputLayer;
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer, BoolReactiveProperty isMainCharacter)
	{
	}

	protected void SetFullPortraitVisible(bool visible)
	{
		if (visible)
		{
			m_PortraitFullView.Bind(base.ViewModel.PortraitVM.Value);
		}
		else
		{
			m_PortraitFullView.Unbind();
		}
	}

	private void SetDoll()
	{
		CharacterRoom.BindDollState(base.ViewModel.CharGenContext.Doll);
	}

	protected virtual void OnClose()
	{
		if (!UINetUtility.IsControlMainCharacter())
		{
			return;
		}
		UIUtility.ShowMessageBox(UIStrings.Instance.ChangeAppearance.CancelWarning, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (base.ViewModel != null && button == DialogMessageBoxBase.BoxButton.Yes)
			{
				base.ViewModel.Close();
			}
		});
	}

	protected virtual void OnConfirm()
	{
		if (!UINetUtility.IsControlMainCharacter())
		{
			return;
		}
		UIUtility.ShowMessageBox(UIStrings.Instance.ChangeAppearance.ConfirmWarning, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (base.ViewModel != null && button == DialogMessageBoxBase.BoxButton.Yes)
			{
				base.ViewModel.Complete();
			}
		});
	}

	private void UpdateDollRooms()
	{
		SetDoll();
		CharacterRoom.SetVisibility(isVisible: true);
		m_CharacterDollTexture.PlayAnimation(value: true);
		DollPosition dollPosition = m_DollPositions.First((DollPosition i) => i.Position == base.ViewModel.CharGenAppearancePhaseVM.DollPosition);
		m_DollTransform.transform.position = dollPosition.Transform.position;
	}

	public void HandleFullScreenUiChanged(bool windowShowed, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType == FullScreenUIType.Encyclopedia)
		{
			m_CanvasGroup.alpha = (windowShowed ? 0f : 1f);
			m_CanvasGroup.interactable = !windowShowed;
			m_CanvasGroup.blocksRaycasts = !windowShowed;
			if (windowShowed)
			{
				m_PantographView.Hide();
			}
			else
			{
				m_PantographView.Show();
			}
		}
	}
}
