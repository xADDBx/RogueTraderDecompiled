using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;

public class CharInfoSkillsAndWeaponsConsoleView : CharInfoSkillsAndWeaponsBaseView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	[Header("Console")]
	[SerializeField]
	protected CharInfoAbilityScoresBlockConsoleView m_AbilityScoresBlockConsoleView;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private Action<IConsoleEntity> m_OnFocusChangeAction;

	private List<CharInfoComponentType> m_ViewsOrder = new List<CharInfoComponentType>
	{
		CharInfoComponentType.Abilities,
		CharInfoComponentType.Skills,
		CharInfoComponentType.Weapons
	};

	public override void Initialize()
	{
		base.Initialize();
		m_AbilityScoresBlockConsoleView.Initialize();
		TypeToView.Add(CharInfoComponentType.Abilities, m_AbilityScoresBlockConsoleView);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(base.ViewModel.Unit.Subscribe(delegate(BaseUnitEntity u)
		{
			m_ViewsOrder = new List<CharInfoComponentType>
			{
				CharInfoComponentType.Abilities,
				CharInfoComponentType.Skills,
				CharInfoComponentType.Weapons
			};
			UnitPartPetOwner unitPartPetOwner = (u.IsPet ? u.Master.GetOptional<UnitPartPetOwner>() : null);
			if (unitPartPetOwner != null)
			{
				PetType petType = unitPartPetOwner.PetType;
				if (petType != PetType.Mastiff && petType != PetType.Eagle)
				{
					m_ViewsOrder = new List<CharInfoComponentType>
					{
						CharInfoComponentType.Abilities,
						CharInfoComponentType.Skills
					};
				}
				CurrentSection.Value = m_ViewsOrder.First();
			}
		}));
		AddDisposable(CurrentSection.Subscribe(UpdateNavigation));
		CurrentSection.Value = m_ViewsOrder.First();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_OnFocusChangeAction = null;
	}

	private void OnNext(InputActionEventData data)
	{
		int index = (m_ViewsOrder.IndexOf(CurrentSection.Value) + 1) % m_ViewsOrder.Count;
		CurrentSection.Value = m_ViewsOrder[index];
	}

	private void UpdateNavigation(CharInfoComponentType type)
	{
		bool isFocused = m_NavigationBehaviour.IsFocused;
		m_NavigationBehaviour.Clear();
		switch (type)
		{
		case CharInfoComponentType.Abilities:
			m_NavigationBehaviour.AddEntityVertical(m_AbilityScoresBlockConsoleView.GetNavigation(1));
			break;
		case CharInfoComponentType.Weapons:
			m_NavigationBehaviour.AddEntityVertical(m_WeaponsBlockPCView.GetNavigation());
			break;
		case CharInfoComponentType.Skills:
			if (m_SkillsBlockPCView is CharInfoSkillsBlockConsoleView charInfoSkillsBlockConsoleView)
			{
				m_NavigationBehaviour.AddEntityVertical(charInfoSkillsBlockConsoleView.GetConsoleEntity(1));
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (isFocused)
		{
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			m_OnFocusChangeAction?.Invoke(m_NavigationBehaviour.DeepestNestedFocus);
		}
	}

	protected override void InternalBindSection(CharInfoComponentType section)
	{
		switch (section)
		{
		case CharInfoComponentType.Abilities:
			m_SkillsBlockPCView.UnbindSection();
			m_WeaponsBlockPCView.UnbindSection();
			m_AbilityScoresBlockConsoleView.BindSection(base.ViewModel.AbilityScoresBlockVM);
			break;
		case CharInfoComponentType.Skills:
			m_AbilityScoresBlockConsoleView.UnbindSection();
			m_WeaponsBlockPCView.UnbindSection();
			m_SkillsBlockPCView.BindSection(base.ViewModel.SkillsBlockVM);
			break;
		case CharInfoComponentType.Weapons:
			m_AbilityScoresBlockConsoleView.UnbindSection();
			m_SkillsBlockPCView.UnbindSection();
			m_WeaponsBlockPCView.BindSection(base.ViewModel.WeaponsBlockVM);
			break;
		default:
			throw new ArgumentOutOfRangeException("section", section, null);
		}
	}

	void ICharInfoComponentConsoleView.AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.FocusOnFirstValidEntity();
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}

	public CompositeDisposable AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		CompositeDisposable compositeDisposable = new CompositeDisposable();
		InputBindStruct inputBindStruct = inputLayer.AddButton(OnNext, 11, enabledHints);
		compositeDisposable.Add(inputBindStruct);
		compositeDisposable.Add(hintsWidget.BindHint(inputBindStruct, UIStrings.Instance.InventoryScreen.ToggleStats));
		AddDisposable(compositeDisposable);
		return compositeDisposable;
	}

	public void SetFocusChangeAction(Action<IConsoleEntity> onFocusChange)
	{
		m_OnFocusChangeAction = onFocusChange;
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		return m_NavigationBehaviour;
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
