using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterName;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Summary;

public class CharGenSummaryPhaseVM : CharGenPhaseBaseVM, ICharGenSummaryPhaseHandler, ISubscriber
{
	public CharGenNameVM CharGenNameVM;

	public CharInfoLevelClassScoresVM LevelClassScoresVM;

	public CharInfoSkillsBlockVM CharInfoSkillsBlockVM;

	public readonly ReactiveCommand<Action> InterruptHandler = new ReactiveCommand<Action>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly AutoDisposingList<CareerPathVM> m_UnitCareers = new AutoDisposingList<CareerPathVM>();

	private SelectionStateCharacterName m_SelectionStateCharacterName;

	private bool m_CharacterNameWasEdited;

	private bool m_Subscribed;

	public CharGenSummaryPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Summary)
	{
		base.HasPantograph = false;
		base.CanInterruptChargen = true;
		CreateTooltipSystem();
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_UnitCareers.Clear();
	}

	protected override bool CheckIsCompleted()
	{
		LevelUpManager value = CharGenContext.LevelUpManager.Value;
		if (value != null && value.IsAllSelectionsMadeAndValid)
		{
			return m_CharacterNameWasEdited;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		SetupTooltipTemplate();
		if (m_Subscribed)
		{
			SetDefaultNameIfNeeded();
			return;
		}
		AddDisposable(CharGenNameVM = new CharGenNameVM(CharGenContext.CurrentUnit, CharGenContext.LevelUpManager, GetRandomName, delegate(string characterName)
		{
			SetName(characterName, isManual: true);
		}));
		AddDisposable(LevelClassScoresVM = new CharInfoLevelClassScoresVM(CharGenNameVM.PreviewUnit));
		AddDisposable(CharInfoSkillsBlockVM = new CharInfoSkillsBlockVM(CharGenNameVM.PreviewUnit, null));
		AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
		AddDisposable(CharGenContext.CurrentUnit.Subscribe(delegate(BaseUnitEntity unit)
		{
			PregenUnitComponent component = unit.Blueprint.GetComponent<PregenUnitComponent>();
			if (component != null)
			{
				SetName(component.PregenName, isManual: false, force: true);
			}
			else
			{
				SetNameUI(string.Empty);
			}
		}));
		AddDisposable(EventBus.Subscribe(this));
		SetDefaultNameIfNeeded();
		m_Subscribed = true;
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager != null)
		{
			m_CharacterNameWasEdited = false;
			BlueprintCharacterNameSelection selectionByType = CharGenUtility.GetSelectionByType<BlueprintCharacterNameSelection>(manager.Path);
			if (selectionByType != null)
			{
				m_SelectionStateCharacterName = manager.GetSelectionState(manager.Path, selectionByType, 0) as SelectionStateCharacterName;
				UpdateIsCompleted();
			}
		}
	}

	private void SetName(string characterName, bool isManual)
	{
		SetName(characterName, isManual, force: false);
	}

	private void SetName(string characterName, bool isManual, bool force)
	{
		Game.Instance.GameCommandQueue.CharGenSetName(characterName, force);
		if (isManual)
		{
			m_CharacterNameWasEdited = true;
		}
	}

	void ICharGenSummaryPhaseHandler.HandleSetName(string characterName)
	{
		if (!CharGenNameVM.UnitName.Value.Equals(characterName, StringComparison.Ordinal))
		{
			CharGenNameVM.SetName(characterName);
		}
		SetNameUI(characterName);
	}

	private void SetNameUI(string characterName)
	{
		m_SelectionStateCharacterName?.SelectName(characterName);
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
		UpdateIsCompleted();
	}

	private string GetRandomName()
	{
		if (CharGenContext.Doll.Race == null)
		{
			return string.Empty;
		}
		return BlueprintCharGenRoot.Instance.PregenCharacterNames.GetRandomName(CharGenContext.Doll.Race.RaceId, CharGenContext.Doll.Gender, CharGenContext.CharGenConfig.Mode, CharGenNameVM.UnitName.Value);
	}

	private void SetDefaultNameIfNeeded()
	{
		if (CharGenContext.IsCustomCharacter.Value && CharGenContext.LevelUpManager.Value.PreviewUnit.GetDescriptionOptional()?.CustomName == null && CharGenContext.Doll?.Race != null)
		{
			string defaultName = BlueprintCharGenRoot.Instance.PregenCharacterNames.GetDefaultName(CharGenContext.Doll.Race.RaceId, CharGenContext.Doll.Gender, CharGenContext.CharGenConfig.Mode, CharGenNameVM.UnitName.Value);
			SetName(defaultName, isManual: false);
		}
	}

	public override void InterruptChargen(Action onComplete)
	{
		InterruptHandler.Execute(onComplete);
	}

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	private void SetupTooltipTemplate()
	{
		m_ReactiveTooltipTemplate.Value = GetTooltipTemplate();
	}

	private TooltipBaseTemplate GetTooltipTemplate()
	{
		LevelUpManager value = CharGenContext.LevelUpManager.Value;
		m_UnitCareers.Clear();
		if (value?.PreviewUnit != null)
		{
			BaseUnitEntity unit = value.PreviewUnit;
			m_UnitCareers.AddRange(from BlueprintCareerPath careerBp in from f in unit.Progression.Features.Visible
					where f.Blueprint is BlueprintCareerPath
					select f.Blueprint
				select new CareerPathVM(unit, careerBp, null));
			return new TooltipTemplateChargenUnitInformation(unit, value, m_UnitCareers);
		}
		return null;
	}
}
