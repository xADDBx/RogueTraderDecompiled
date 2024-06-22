using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.FactionsReputation;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.PagesMenu;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Stories;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Summary;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;

public class CharacterInfoVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly Dictionary<CharInfoComponentType, ReactiveProperty<CharInfoComponentVM>> ComponentVMs = new Dictionary<CharInfoComponentType, ReactiveProperty<CharInfoComponentVM>>();

	public readonly SelectionGroupRadioVM<CharInfoPagesMenuEntityVM> PagesSelectionGroupRadioVM;

	public readonly ReactiveCommand BiographyUpdated = new ReactiveCommand();

	public readonly ReactiveProperty<CharInfoPageType> PageType = new ReactiveProperty<CharInfoPageType>();

	private readonly IReactiveProperty<BaseUnitEntity> m_Unit;

	private readonly ReactiveProperty<BaseUnitEntity> m_PreviewUnit = new ReactiveProperty<BaseUnitEntity>();

	private List<CharInfoComponentType> m_CreatedVMs = new List<CharInfoComponentType>();

	private readonly CharInfoPagesPC m_CharInfoPages;

	private readonly ReactiveCollection<CharInfoPagesMenuEntityVM> m_Pages = new ReactiveCollection<CharInfoPagesMenuEntityVM>();

	private readonly ReactiveProperty<CharInfoPagesMenuEntityVM> m_CurrentPage = new ReactiveProperty<CharInfoPagesMenuEntityVM>();

	private readonly ReactiveProperty<LevelUpManager> m_LevelUpManager = new ReactiveProperty<LevelUpManager>();

	public bool CanCloseWindow
	{
		get
		{
			if (m_LevelUpManager.Value != null)
			{
				return m_LevelUpManager.Value.IsCommitted;
			}
			return true;
		}
	}

	public bool PageCanHaveNoEntities
	{
		get
		{
			CharInfoPagesMenuEntityVM value = m_CurrentPage.Value;
			if (value == null)
			{
				return false;
			}
			return value.PageType == CharInfoPageType.Biography;
		}
	}

	public CharacterInfoVM(CharInfoPageType selectedPageType = CharInfoPageType.Summary)
	{
		AddDisposable(m_CharInfoPages = new CharInfoPagesPC());
		AddDisposable(m_CurrentPage.Subscribe(delegate(CharInfoPagesMenuEntityVM value)
		{
			if (value != null)
			{
				PageType.Value = value.PageType;
			}
		}));
		ReactiveProperty<BaseUnitEntity> selectedUnitInUI = Game.Instance.SelectionCharacter.SelectedUnitInUI;
		if (selectedUnitInUI.Value == null)
		{
			BaseUnitEntity baseUnitEntity = (selectedUnitInUI.Value = Game.Instance.Player.MainCharacterEntity);
		}
		m_Unit = Game.Instance.SelectionCharacter.SelectedUnitInUI;
		m_PreviewUnit.Value = m_Unit.Value;
		foreach (CharInfoComponentType value in Enum.GetValues(typeof(CharInfoComponentType)))
		{
			ComponentVMs[value] = new ReactiveProperty<CharInfoComponentVM>();
		}
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(PagesSelectionGroupRadioVM = new SelectionGroupRadioVM<CharInfoPagesMenuEntityVM>(m_Pages));
		AddDisposable(PagesSelectionGroupRadioVM.SelectedEntity.Subscribe(delegate(CharInfoPagesMenuEntityVM value)
		{
			if (value != m_CurrentPage.Value)
			{
				if (ComponentVMs[CharInfoComponentType.Progression].Value is UnitProgressionVM unitProgressionVM)
				{
					unitProgressionVM.TryClose(delegate
					{
						m_CurrentPage.Value = value;
					}, delegate
					{
						PagesSelectionGroupRadioVM.TrySelectEntity(m_CurrentPage.Value);
					});
				}
				else
				{
					m_CurrentPage.Value = value;
				}
			}
		}));
		foreach (CharInfoPageType item in m_CharInfoPages.PagesOrder)
		{
			CharInfoPagesMenuEntityVM charInfoPagesMenuEntityVM = new CharInfoPagesMenuEntityVM(item, m_Unit);
			m_Pages.Add(charInfoPagesMenuEntityVM);
			AddDisposable(charInfoPagesMenuEntityVM);
			if (item == selectedPageType)
			{
				m_CurrentPage.Value = charInfoPagesMenuEntityVM;
				PagesSelectionGroupRadioVM.TrySelectEntity(charInfoPagesMenuEntityVM);
			}
		}
		AddDisposable(m_CurrentPage.Subscribe(delegate
		{
			UpdateData();
		}));
		AddDisposable(m_Unit.Subscribe(delegate
		{
			if (m_CurrentPage.Value.PageType == CharInfoPageType.Biography)
			{
				UpdateData();
				BiographyUpdated.Execute();
			}
		}));
	}

	protected override void DisposeImplementation()
	{
		foreach (CharInfoComponentType value in Enum.GetValues(typeof(CharInfoComponentType)))
		{
			ComponentVMs[value].Value?.Dispose();
			ComponentVMs[value].Value = null;
		}
		m_LevelUpManager.Value = null;
	}

	public void SetCurrentPage(CharInfoPageType pageType)
	{
		CharInfoPagesMenuEntityVM value = m_CurrentPage.Value;
		if (value == null || value.PageType != pageType)
		{
			m_Pages.FirstOrDefault((CharInfoPagesMenuEntityVM p) => p.PageType == pageType)?.SetSelected(state: true);
		}
	}

	private void UpdateData()
	{
		if (m_CurrentPage.Value == null || !m_CurrentPage.Value.IsAvailable.Value)
		{
			PagesSelectionGroupRadioVM.TrySelectFirstValidEntity();
		}
		CreateVMs(m_CharInfoPages.GetComponentsList(m_CurrentPage.Value.PageType, GetUnitType(m_Unit.Value)));
		UIEventType eventType = m_CurrentPage.Value.PageType switch
		{
			CharInfoPageType.Summary => UIEventType.CharacterInfoSummaryOpen, 
			CharInfoPageType.Features => UIEventType.CharacterInfoFeaturesOpen, 
			CharInfoPageType.PsykerPowers => UIEventType.CharacterInfoPsykerPowersOpen, 
			CharInfoPageType.LevelProgression => UIEventType.CharacterInfoLevelProgressionOpen, 
			CharInfoPageType.Biography => UIEventType.CharacterInfoBiographyOpen, 
			CharInfoPageType.FactionsReputation => UIEventType.CharacterInfoFactionsReputationOpen, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		EventBus.RaiseEvent(delegate(IUIEventHandler h)
		{
			h.HandleUIEvent(eventType);
		});
	}

	private void CreateVMs(List<CharInfoComponentType> types)
	{
		if (types == null || m_CreatedVMs.SequenceEqual(types))
		{
			return;
		}
		foreach (CharInfoComponentType item in m_CreatedVMs.Except(types))
		{
			ComponentVMs[item].Value?.Dispose();
			ComponentVMs[item].Value = null;
		}
		foreach (CharInfoComponentType item2 in types.Except(m_CreatedVMs))
		{
			ComponentVMs[item2].Value = CreateVM(item2);
		}
		m_CreatedVMs = types;
	}

	private CharInfoComponentVM CreateVM(CharInfoComponentType type)
	{
		return type switch
		{
			CharInfoComponentType.NameAndPortrait => new CharInfoNameAndPortraitVM(m_Unit), 
			CharInfoComponentType.LevelClassScores => new CharInfoLevelClassScoresVM(m_Unit, m_LevelUpManager), 
			CharInfoComponentType.Skills => new CharInfoSkillsBlockVM(m_Unit, m_LevelUpManager), 
			CharInfoComponentType.Abilities => new CharInfoAbilitiesVM(m_Unit), 
			CharInfoComponentType.AlignmentWheel => new CharInfoAlignmentVM(m_Unit), 
			CharInfoComponentType.AlignmentHistory => new CharInfoAlignmentHistoryVM(m_Unit), 
			CharInfoComponentType.Stories => new CharInfoStoriesVM(m_Unit), 
			CharInfoComponentType.NameFullPortrait => new CharInfoNameAndPortraitVM(m_Unit), 
			CharInfoComponentType.BiographyStories => new CharInfoStoriesVM(m_Unit), 
			CharInfoComponentType.Progression => new UnitProgressionVM(m_Unit, m_LevelUpManager, UnitProgressionMode.LevelUp), 
			CharInfoComponentType.Weapons => new CharInfoWeaponsBlockVM(m_Unit), 
			CharInfoComponentType.SkillsAndWeapons => new CharInfoSkillsAndWeaponsVM(m_Unit, m_LevelUpManager), 
			CharInfoComponentType.FactionsReputation => new CharInfoFactionsReputationVM(m_Unit), 
			CharInfoComponentType.Summary => new CharInfoSummaryVM(m_Unit), 
			_ => null, 
		};
	}

	private UnitType GetUnitType(BaseUnitEntity unit)
	{
		if (unit.IsMainCharacter)
		{
			return UnitType.MainCharacter;
		}
		if (Game.Instance.Player.AllCharacters.Contains(unit))
		{
			return UnitType.Companion;
		}
		if (!unit.IsPet)
		{
			return UnitType.Unknown;
		}
		return UnitType.Pet;
	}

	public void ClearProgressionIfNeeded(BaseUnitEntity newUnitEntity)
	{
		(ComponentVMs[CharInfoComponentType.Progression].Value as UnitProgressionVM)?.ClearLevelupManagerIfNeeded(newUnitEntity);
	}
}
