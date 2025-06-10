using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;

public class CharInfoNameAndPortraitConsoleView : CharInfoNameAndPortraitPCView
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_PreviousFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	public void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			base.ViewModel.SelectPrevCharacter();
		}, 14, enabledHints);
		AddDisposable(m_PreviousFilterHint.Bind(inputBindStruct));
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			base.ViewModel.SelectNextCharacter();
		}, 15, enabledHints);
		AddDisposable(m_NextFilterHint.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		AddDisposable(enabledHints.Subscribe(delegate(bool value)
		{
			m_NextButton.gameObject.SetActive(!value);
			m_PrevButton.gameObject.SetActive(!value);
		}));
		AddDisposable(base.ViewModel.Unit.Subscribe(delegate(BaseUnitEntity u)
		{
			m_LeftCommonSlots.PlayAnimation(value: true);
			m_CommonWeaponSet.PlayAnimation(value: true);
			m_PetSlots.PlayAnimation(value: false);
			if (!u.IsPet)
			{
				return;
			}
			UnitPartPetOwner unitPartPetOwner = (u.IsPet ? u.Master.GetOptional<UnitPartPetOwner>() : null);
			m_LeftCommonSlots.PlayAnimation(value: false);
			FadeAnimator commonWeaponSet = m_CommonWeaponSet;
			bool value2;
			if (unitPartPetOwner != null)
			{
				PetType petType = unitPartPetOwner.PetType;
				if ((uint)(petType - 1) <= 1u)
				{
					value2 = true;
					goto IL_0071;
				}
			}
			value2 = false;
			goto IL_0071;
			IL_0071:
			commonWeaponSet.PlayAnimation(value2);
			m_PetSlots.PlayAnimation(value: true);
		}));
	}
}
