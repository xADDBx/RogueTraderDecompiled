using DG.Tweening;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Twitch;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;

public abstract class OvertipMapObjectTwitchDropsView : BaseOvertipMapObjectView
{
	[SerializeField]
	protected OwlcatButton m_Button;

	[Header("Images")]
	[SerializeField]
	private Image m_MainImage;

	[SerializeField]
	protected Image m_HighlightImage;

	[SerializeField]
	protected CanvasGroup m_HighlightImageCanvasGroup;

	[SerializeField]
	private Image m_ActiveImage;

	[SerializeField]
	private CanvasGroup m_ActiveImageCanvasGroup;

	[SerializeField]
	protected Image m_DisabledImage;

	[SerializeField]
	protected CanvasGroup m_DisabledImageCanvasGroup;

	[SerializeField]
	private UIInteractionTypeSprites m_Sprites;

	[Header("Common Block")]
	[SerializeField]
	private CanvasGroup m_InnerCanvasGroup;

	[SerializeField]
	private float m_OnHoverVisibility = 1f;

	[SerializeField]
	private float m_NotHoverVisibility = 0.65f;

	[SerializeField]
	private float m_FarDistance = 120f;

	[SerializeField]
	private float m_FarObjectVisibility = 0.3f;

	[SerializeField]
	private Vector3 m_FarScaleVisibility = new Vector3(0.5f, 0.5f, 0.5f);

	private Sequence m_Animator;

	private bool m_IsActiveFlag;

	private bool m_TwitchDropsAvailable;

	private bool CheckVisibleTrigger
	{
		get
		{
			if (!base.ViewModel.IsMouseOverUI.Value && !base.ViewModel.MapObjectIsHighlited.Value && !base.ViewModel.ForceHotKeyPressed.Value && !base.ViewModel.ActiveCharacterIsNear && !base.ViewModel.IsBarkActive.Value)
			{
				return base.ViewModel.IsInCombat;
			}
			return true;
		}
	}

	protected override bool CheckVisibility
	{
		get
		{
			if (base.CheckCanBeVisible && CheckVisibleTrigger)
			{
				return m_TwitchDropsAvailable;
			}
			return false;
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(ObservableExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.InteractTwitchDrops();
		}));
		base.gameObject.name = base.ViewModel.MapObjectEntity.View.gameObject.name + "_TwitchDropsOvertip";
		m_ActiveImageCanvasGroup.alpha = 0f;
		SetupSprites();
		SetActiveImage(active: false, force: true);
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.TwitchStatus.Subscribe(delegate(TwitchDropsLinkStatus status)
		{
			m_TwitchDropsAvailable = status != 0 && status != TwitchDropsLinkStatus.NotSupported;
		}));
		base.ViewModel.CheckTwitchLinkedStatus();
		AddDisposable(UniRxExtensionMethods.Subscribe(base.ViewModel.VisibilityChanged, delegate
		{
			if (CheckVisibility)
			{
				UpdateVisibility();
			}
		}));
		AddDisposable(base.ViewModel.CanInteract.Subscribe(SetInteractable));
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			base.ViewModel.CheckTwitchLinkedStatus();
		}
	}

	protected override void SetActiveInternal(bool isActive)
	{
		base.SetActiveInternal(isActive);
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		bool flag = base.ViewModel.CameraDistance.Value.sqrMagnitude < m_FarDistance;
		Vector3 one = Vector3.one;
		float endValue;
		if (CheckVisibleTrigger)
		{
			endValue = (flag ? m_OnHoverVisibility : m_NotHoverVisibility);
			one = ((flag || base.ViewModel.IsMouseOverUI.Value || base.ViewModel.MapObjectIsHighlited.Value || base.ViewModel.ActiveCharacterIsNear || base.ViewModel.IsBarkActive.Value) ? Vector3.one : m_FarScaleVisibility);
		}
		else
		{
			endValue = ((!flag) ? m_FarObjectVisibility : (base.ViewModel.ActiveCharacterIsNear ? m_OnHoverVisibility : m_NotHoverVisibility));
			one = Vector3.one;
		}
		m_Animator?.Kill();
		m_Animator = DOTween.Sequence().SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_Animator = m_Animator.Join(m_InnerCanvasGroup.DOFade(endValue, 0.2f));
		m_Animator = m_Animator.Join((base.transform as RectTransform).DOScale(one, 0.2f));
		m_Animator.Play();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Animator?.Kill();
		m_Animator = null;
	}

	protected override void UpdateActive(bool isActive)
	{
		m_InnerCanvasGroup.blocksRaycasts = isActive;
		if ((bool)base.ViewModel.FirstInteractionPart && CheckVisibility)
		{
			SetActiveImage(base.ViewModel.IsInteract);
		}
	}

	private void SetupSprites()
	{
		m_MainImage.sprite = m_Sprites.Main;
		m_ActiveImage.sprite = m_Sprites.Active;
		m_HighlightImage.sprite = m_Sprites.Hover;
		m_DisabledImage.sprite = m_Sprites.Disabled;
	}

	private void SetActiveImage(bool active, bool force = false)
	{
		if (force || m_IsActiveFlag != active)
		{
			m_IsActiveFlag = active;
			if (active)
			{
				m_ActiveImageCanvasGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true);
			}
			else
			{
				m_ActiveImageCanvasGroup.DOFade(0f, 0.2f).SetUpdate(isIndependentUpdate: true);
			}
		}
	}

	protected void SetHighlightImage(bool active)
	{
		if (active)
		{
			m_HighlightImageCanvasGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			m_HighlightImageCanvasGroup.DOFade(0f, 0.2f).SetUpdate(isIndependentUpdate: true);
		}
	}

	protected void SetInteractable(bool interactable)
	{
		m_Button.Interactable = interactable;
		if (interactable)
		{
			m_DisabledImageCanvasGroup.DOFade(0f, 0.2f).SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			m_DisabledImageCanvasGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true);
		}
	}
}
