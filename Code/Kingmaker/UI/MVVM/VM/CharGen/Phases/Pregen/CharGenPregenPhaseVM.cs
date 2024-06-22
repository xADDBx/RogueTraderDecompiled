using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.LevelUp;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;

public class CharGenPregenPhaseVM : CharGenPhaseBaseVM, ICharGenPregenHandler, ISubscriber
{
	public readonly ReactiveProperty<CharGenPregenSelectorItemVM> SelectedPregenEntity = new ReactiveProperty<CharGenPregenSelectorItemVM>();

	private readonly ReactiveCollection<CharGenPregenSelectorItemVM> m_PregenEntitiesList = new ReactiveCollection<CharGenPregenSelectorItemVM>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly AutoDisposingList<CareerPathVM> m_PregenCareers = new AutoDisposingList<CareerPathVM>();

	public readonly BoolReactiveProperty CurrentPageIsFirst = new BoolReactiveProperty();

	public readonly BoolReactiveProperty CurrentPageIsLast = new BoolReactiveProperty();

	public SelectionGroupRadioVM<CharGenPregenSelectorItemVM> PregenSelectionGroup { get; }

	public IReadOnlyReactiveProperty<bool> IsCustomCharacter => CharGenContext.IsCustomCharacter;

	private bool IsPregen => SelectedPregenEntity.Value?.ChargenUnit != null;

	public CharGenPregenPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Pregen)
	{
		PregenSelectionGroup = new SelectionGroupRadioVM<CharGenPregenSelectorItemVM>(m_PregenEntitiesList, SelectedPregenEntity);
		AddDisposable(PregenSelectionGroup);
		AddDisposable(SelectedPregenEntity.Subscribe(SetPregen));
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(SelectedPregenEntity.Subscribe(delegate(CharGenPregenSelectorItemVM value)
		{
			CurrentPageIsFirst.Value = m_PregenEntitiesList.FirstOrDefault() == value;
			CurrentPageIsLast.Value = m_PregenEntitiesList.LastOrDefault() == value;
		}));
		CreateTooltipSystem();
		if (CharGenContext.CharGenConfig.Mode == CharGenConfig.CharGenMode.NewGame)
		{
			BlueprintRoot.Instance.CharGenRoot.EnsureNewGamePregens(UnitsCallback);
		}
		else
		{
			BlueprintRoot.Instance.CharGenRoot.EnsureCompanionPregens(UnitsCallback, CharGenContext.CharGenConfig.CompanionType);
		}
		void UnitsCallback(List<ChargenUnit> units)
		{
			m_PregenEntitiesList.Add(AddDisposableAndReturn(new CharGenPregenSelectorItemVM(null, isCustomCharacter: true)));
			foreach (ChargenUnit unit in units)
			{
				m_PregenEntitiesList.Add(AddDisposableAndReturn(new CharGenPregenSelectorItemVM(unit)));
			}
			if (m_PregenEntitiesList.Count > 1)
			{
				PregenSelectionGroup.TrySelectEntity(m_PregenEntitiesList[1]);
			}
			else
			{
				PregenSelectionGroup.TrySelectFirstValidEntity();
			}
		}
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_PregenCareers.Clear();
		BlueprintRoot.Instance.CharGenRoot.DisposeUnitsForChargen();
	}

	protected override bool CheckIsCompleted()
	{
		return true;
	}

	protected override void OnBeginDetailedView()
	{
	}

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	private void SetPregen(CharGenPregenSelectorItemVM pregenVM)
	{
		if (pregenVM == null)
		{
			SetUnit(null);
			return;
		}
		BaseUnitEntity unit = pregenVM.ChargenUnit?.Unit;
		Game.Instance.GameCommandQueue.CharGenSetPregen(unit);
	}

	void ICharGenPregenHandler.HandleSetPregen(BaseUnitEntity unit)
	{
		if (!UINetUtility.IsControlMainCharacter())
		{
			SelectedPregenEntity.Value = m_PregenEntitiesList.FirstOrDefault((CharGenPregenSelectorItemVM item) => unit == item.ChargenUnit?.Unit);
		}
		SetUnit(unit);
		UpdatePhaseName();
	}

	private void SetUnit([CanBeNull] BaseUnitEntity unit)
	{
		CharGenContext.SetPregenUnit(unit);
		base.ShowVisualSettings.Value = unit != null;
		SetupTooltipTemplate();
	}

	private void SetupTooltipTemplate()
	{
		m_ReactiveTooltipTemplate.Value = TooltipTemplate();
	}

	private TooltipBaseTemplate TooltipTemplate()
	{
		if (!IsPregen)
		{
			CharGenConfig charGenConfig = CharGenContext.CharGenConfig;
			return new TooltipTemplateChargenCustomCharacter(charGenConfig.Mode, charGenConfig.CompanionType);
		}
		BaseUnitEntity unit = SelectedPregenEntity.Value.ChargenUnit.Unit;
		m_PregenCareers.Clear();
		m_PregenCareers.AddRange(from BlueprintCareerPath careerBp in from f in unit.Progression.Features.Visible
				where f.Blueprint is BlueprintCareerPath
				select f.Blueprint
			select new CareerPathVM(unit, careerBp, null));
		return new TooltipTemplateChargenUnitInformation(unit, CharGenContext.LevelUpManager.Value, m_PregenCareers, expandedView: true);
	}

	public void UpdatePhaseName()
	{
		PhaseName.Value = (IsPregen ? UIStrings.Instance.CharGen.Pregen : UIStrings.Instance.CharGen.CustomCharacterPregen);
	}

	public bool GoNextPage()
	{
		return PregenSelectionGroup.SelectNextValidEntity();
	}

	public bool GoPrevPage()
	{
		return PregenSelectionGroup.SelectPrevValidEntity();
	}
}
