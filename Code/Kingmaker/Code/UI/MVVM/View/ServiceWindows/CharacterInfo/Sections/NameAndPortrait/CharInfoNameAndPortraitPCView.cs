using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait.HitPoints;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Sound;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;

public class CharInfoNameAndPortraitPCView : CharInfoComponentWithLevelUpView<CharInfoNameAndPortraitVM>
{
	private enum PortraitSize
	{
		Small,
		Middle,
		Full
	}

	[SerializeField]
	private OwlcatButton m_PetButton;

	[SerializeField]
	private MoveAnimator m_PetButtonMoveAnimator;

	[SerializeField]
	protected FadeAnimator m_CommonWeaponSet;

	[SerializeField]
	protected FadeAnimator m_LeftCommonSlots;

	[SerializeField]
	protected FadeAnimator m_PetSlots;

	[SerializeField]
	private ScrambledTMP m_NameFieldScrambled;

	[SerializeField]
	private CharBPortraitChanger m_Portrait;

	[SerializeField]
	private CharInfoHitPointsPCView m_HitPointsView;

	[SerializeField]
	protected OwlcatButton m_NextButton;

	[SerializeField]
	protected OwlcatButton m_PrevButton;

	[SerializeField]
	private PortraitSize m_Size;

	public override void Initialize()
	{
		base.Initialize();
		m_HitPointsView.Or(null)?.Initialize();
		m_Portrait.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_NextButton, UISounds.ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_PrevButton, UISounds.ButtonSoundsEnum.PlastickSound);
		AddDisposable(m_NextButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SelectNextCharacter();
		}));
		AddDisposable(m_PrevButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SelectPrevCharacter();
		}));
		AddDisposable(base.ViewModel.UnitName.Subscribe(delegate
		{
			SetName();
		}));
		AddDisposable(base.ViewModel.GroupCount.Subscribe(delegate(int count)
		{
			m_NextButton.SetInteractable(count > 1);
			m_PrevButton.SetInteractable(count > 1);
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevCharacter.name, base.ViewModel.SelectPrevCharacter));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextCharacter.name, base.ViewModel.SelectNextCharacter));
		AddDisposable(base.ViewModel.Unit.Subscribe(delegate(BaseUnitEntity u)
		{
			if (Game.Instance.RootUiContext.CurrentServiceWindow != ServiceWindowsType.Inventory)
			{
				return;
			}
			UnitPartPetOwner unitPartPetOwner = (u.IsPet ? u.Master.GetOptional<UnitPartPetOwner>() : null);
			UnitPartPetOwner optional2 = u.GetOptional<UnitPartPetOwner>();
			bool flag = u.IsPet || optional2 != null;
			m_PetButtonMoveAnimator?.PlayAnimation(flag);
			m_LeftCommonSlots.PlayAnimation(value: true);
			m_CommonWeaponSet.PlayAnimation(value: true);
			m_PetSlots.PlayAnimation(value: false);
			if (!flag)
			{
				return;
			}
			if (!u.IsPet)
			{
				if (optional2 != null)
				{
					m_PetButton.SetHint(UIStrings.Instance.Tooltips.PetButtonHoverTooltip, "Interactable");
				}
				return;
			}
			m_PetButton?.SetHint(UIStrings.Instance.Tooltips.MasterButtonHoverTooltip, "Interactable");
			m_LeftCommonSlots.PlayAnimation(value: false);
			FadeAnimator commonWeaponSet = m_CommonWeaponSet;
			bool value;
			if (unitPartPetOwner != null)
			{
				PetType petType = unitPartPetOwner.PetType;
				if ((uint)(petType - 1) <= 1u)
				{
					value = true;
					goto IL_00eb;
				}
			}
			value = false;
			goto IL_00eb;
			IL_00eb:
			commonWeaponSet.PlayAnimation(value);
			m_PetSlots.PlayAnimation(value: true);
		}));
		AddDisposable(m_PetButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			UnitPartPetOwner optional = base.ViewModel.Unit.Value.GetOptional<UnitPartPetOwner>();
			if (base.ViewModel.Unit.Value.IsPet)
			{
				Game.Instance.SelectionCharacter.SetSelected(base.ViewModel.Unit.Value.Master);
			}
			else if (optional != null)
			{
				Game.Instance.SelectionCharacter.SetSelected(optional.PetUnit);
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Portrait.Dispose();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		SetName();
		SetPortrait();
		SetHP();
	}

	private void SetName()
	{
		string value = base.ViewModel.UnitName.Value;
		if (m_NameFieldScrambled != null && m_NameFieldScrambled.Text != value)
		{
			m_NameFieldScrambled.SetText(string.Empty, value);
		}
	}

	private void SetPortrait()
	{
		switch (m_Size)
		{
		case PortraitSize.Small:
			m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitSmall);
			break;
		case PortraitSize.Middle:
			m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitHalf);
			break;
		case PortraitSize.Full:
			m_Portrait.SetNewPortrait(base.ViewModel.UnitPortraitFull);
			break;
		}
	}

	private void SetHP()
	{
		m_HitPointsView.Or(null)?.Bind(base.ViewModel.HitPoints);
	}

	protected override void OnShow()
	{
		m_Portrait.gameObject.SetActive(value: true);
	}

	protected override void OnHide()
	{
		UISounds.Instance.Sounds.Character.CharacterStatsHide.Play();
		m_Portrait.gameObject.SetActive(value: false);
		m_PetSlots.gameObject.SetActive(value: false);
		m_LeftCommonSlots.gameObject.SetActive(value: false);
	}
}
