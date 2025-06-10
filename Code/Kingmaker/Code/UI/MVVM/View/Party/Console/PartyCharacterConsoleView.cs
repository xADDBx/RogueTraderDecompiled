using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Party.Console;

public class PartyCharacterConsoleView : ViewBase<PartyCharacterVM>, IScrollHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	[Header("Parts Block")]
	[SerializeField]
	private UnitHealthPartProgressPCView m_HealthProgressView;

	[SerializeField]
	private UnitHealthPartTextPCView m_HealthTextView;

	[SerializeField]
	private UnitPortraitPartPCView m_PortraitView;

	[SerializeField]
	private UnitBuffPartPCView m_BuffView;

	[SerializeField]
	private UnitBarkPartView m_BarkBlockView;

	[SerializeField]
	private MoveAnimator m_SelectorMoveAnimator;

	[SerializeField]
	private List<Image> m_LinksImages = new List<Image>();

	[Header("Buttons Part")]
	[SerializeField]
	private OwlcatButton m_LevelUpButton;

	[SerializeField]
	private OwlcatMultiButton m_CharacterButton;

	[Header("Net Block")]
	[SerializeField]
	private Image m_NetLock;

	[Header("Encumbrance Part")]
	[SerializeField]
	private GameObject m_EncumbranceIndicator;

	[SerializeField]
	private Color m_EncumbranceHeavyLoad = Color.yellow;

	[SerializeField]
	private Color m_EncumbranceOverload = Color.red;

	[SerializeField]
	private Image m_PersonalEncumbranceIcon;

	[SerializeField]
	private GameObject m_PersonalEncumbranceObject;

	[Header("PetLable")]
	[SerializeField]
	private GameObject m_PetLabel;

	[SerializeField]
	private GameObject m_PetLabelHologram;

	[SerializeField]
	private Image m_FrameSelectorImage;

	[SerializeField]
	private Image m_PetNumberIconOnRoundLabel;

	[SerializeField]
	private Image m_PetNumberIconOnHologram;

	[SerializeField]
	private GameObject m_AscendLabel;

	private RectTransform m_RectTransform;

	private RectTransform m_ParentTransform;

	private Vector2 m_OriginalLocalPointerPosition;

	private Vector3 m_OriginalPanelLocalPosition;

	private bool m_IsDragging;

	public float BasePositionX;

	private Action<PartyCharacterPCView> m_SwitchAction;

	public BaseUnitEntity UnitEntityData => base.ViewModel?.UnitEntityData;

	public void Initialize(Action<PartyCharacterPCView> switchAction)
	{
		m_SwitchAction = switchAction;
		base.gameObject.SetActive(value: false);
		m_HealthTextView.gameObject.SetActive(value: false);
		m_SelectorMoveAnimator.Initialize();
		m_RectTransform = base.transform as RectTransform;
		m_ParentTransform = base.transform.parent as RectTransform;
	}

	protected override void BindViewImplementation()
	{
		m_HealthProgressView.Bind(base.ViewModel.HealthPartVM);
		m_HealthTextView.Bind(base.ViewModel.HealthPartVM);
		m_PortraitView.Bind(base.ViewModel.PortraitPartVM);
		m_BuffView.Bind(base.ViewModel.BuffPartVM);
		m_BarkBlockView.Bind(base.ViewModel.BarkPartVM);
		AddDisposable(base.ViewModel.CharacterEncumbrance.CombineLatest(base.ViewModel.PartyEncumbrance, (Encumbrance _, Encumbrance _) => true).Subscribe(delegate
		{
			UpdateEncumbrance();
		}));
		AddDisposable(base.ViewModel.AscendedLabel.Subscribe(UpdateAscendedLabel));
		AddDisposable(base.ViewModel.HasPet.Subscribe(PetLabelSetActiveHandler));
		AddDisposable(base.ViewModel.IsEnable.Subscribe(base.gameObject.SetActive));
		AddDisposable(base.ViewModel.FakeSelected.Subscribe(SetSelected));
		AddDisposable(base.ViewModel.SelectorFrameIcon.Subscribe(delegate(Sprite s)
		{
			m_FrameSelectorImage.sprite = s;
		}));
		AddDisposable(base.ViewModel.PetMasterNumberIcon.Subscribe(delegate(Sprite s)
		{
			m_PetNumberIconOnRoundLabel.sprite = s;
			m_PetNumberIconOnHologram.sprite = s;
		}));
		AddDisposable(base.ViewModel.IsLevelUp.CombineLatest(base.ViewModel.IsServiceWindowAvailable, (bool levelup, bool serviceWindowAvailable) => levelup && serviceWindowAvailable).Subscribe(m_LevelUpButton.gameObject.SetActive));
		AddDisposable(base.ViewModel.IsSingleSelected.Subscribe(SetSelected));
		AddDisposable(base.ViewModel.IsLinked.Subscribe(delegate(bool value)
		{
			Color white = Color.white;
			white.a = (value ? 1f : 0f);
			foreach (Image linksImage in m_LinksImages)
			{
				linksImage.color = white;
			}
		}));
		if (m_NetLock != null)
		{
			AddDisposable(base.ViewModel.NetAvatar.Subscribe(delegate(Sprite value)
			{
				m_NetLock.gameObject.SetActive(value != null);
				m_NetLock.sprite = value;
			}));
		}
		AddDisposable(base.ViewModel.IsHudActive.Subscribe(delegate(bool v)
		{
			if (base.ViewModel.HasPet.Value)
			{
				if (v)
				{
					m_PetLabel.SetActive(value: true);
					m_PetLabelHologram.SetActive(value: false);
				}
				else
				{
					m_PetLabel.SetActive(value: false);
					m_PetLabelHologram.SetActive(value: true);
				}
			}
		}));
	}

	public void UpdateBasePosition()
	{
		BasePositionX = m_RectTransform.localPosition.x;
	}

	private void SetSelected(bool value)
	{
		m_CharacterButton.SetActiveLayer(value ? 1 : 0);
		m_SelectorMoveAnimator.PlayAnimation(value);
		if (value)
		{
			UISounds.Instance.Sounds.Character.CharacterSelect.Play();
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void UpdateEncumbrance()
	{
		bool flag = base.ViewModel.CharacterEncumbrance.Value == Encumbrance.Overload;
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		bool flag2 = (loadedAreaState == null || !loadedAreaState.Settings.CapitalPartyMode) && base.ViewModel.PartyEncumbrance.Value == Encumbrance.Overload;
		if (!flag2)
		{
			if (base.ViewModel.CharacterEncumbrance.Value == Encumbrance.Heavy)
			{
				m_PersonalEncumbranceIcon.color = m_EncumbranceHeavyLoad;
			}
			if (base.ViewModel.CharacterEncumbrance.Value == Encumbrance.Overload)
			{
				m_PersonalEncumbranceIcon.color = m_EncumbranceOverload;
			}
		}
		m_PersonalEncumbranceObject.SetActive(!flag2);
		m_EncumbranceIndicator.SetActive(flag2 || flag);
	}

	public void OnScroll(PointerEventData eventData)
	{
		float y = eventData.scrollDelta.y;
		if (!(Mathf.Abs(y) < Mathf.Epsilon))
		{
			UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
			base.ViewModel.NextPrev(y < 0f);
		}
	}

	public void OnBeginDrag(PointerEventData data)
	{
		UpdateBasePosition();
		if (base.ViewModel.CanSwitch)
		{
			m_IsDragging = true;
			m_OriginalPanelLocalPosition = m_RectTransform.localPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentTransform, data.position, data.pressEventCamera, out m_OriginalLocalPointerPosition);
			m_RectTransform.transform.SetAsLastSibling();
		}
	}

	public void OnEndDrag(PointerEventData data)
	{
		m_IsDragging = false;
		m_RectTransform.DOLocalMoveX(BasePositionX, 0.2f).SetUpdate(isIndependentUpdate: true);
	}

	public void OnDrag(PointerEventData data)
	{
		if (m_IsDragging && base.ViewModel.CanSwitch)
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentTransform, data.position, data.pressEventCamera, out var localPoint))
			{
				SetTargetAnchoredPosition(localPoint);
			}
			ClampToWindow();
		}
	}

	private void SetTargetAnchoredPosition(Vector2 localPointerPosition)
	{
		Vector3 vector = localPointerPosition - m_OriginalLocalPointerPosition;
		vector.y = 0f;
		m_RectTransform.localPosition = m_OriginalPanelLocalPosition + vector;
	}

	private void ClampToWindow()
	{
		Vector3 localPosition = m_RectTransform.localPosition;
		Vector3 vector = m_ParentTransform.rect.min - m_RectTransform.rect.min;
		Vector3 vector2 = m_ParentTransform.rect.max - m_RectTransform.rect.max;
		localPosition.x = Mathf.Clamp(m_RectTransform.localPosition.x, vector.x, vector2.x);
		localPosition.y = Mathf.Clamp(m_RectTransform.localPosition.y, vector.y, vector2.y);
		m_RectTransform.localPosition = localPosition;
	}

	private void PetLabelSetActiveHandler(bool value)
	{
		if (base.ViewModel.IsHudActive.Value)
		{
			if (m_PetLabelHologram.activeInHierarchy)
			{
				m_PetLabelHologram.SetActive(value: false);
			}
			m_PetLabel.SetActive(value);
		}
		else
		{
			if (m_PetLabel.activeInHierarchy)
			{
				m_PetLabel.SetActive(value: false);
			}
			m_PetLabelHologram.SetActive(value);
		}
	}

	private void UpdateAscendedLabel(bool value)
	{
		m_AscendLabel.SetActive(value);
	}

	public void ShowInspect()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitConsoleInvoke(UnitEntityData);
		});
	}
}
