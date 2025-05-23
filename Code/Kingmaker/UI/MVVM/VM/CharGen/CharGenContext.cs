using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public class CharGenContext : IDisposable, ICharGenDollStateHandler, ISubscriber, ICharGenTextureSelectorTabChangeHandler
{
	public readonly CharGenConfig CharGenConfig;

	public readonly ReactiveProperty<BaseUnitEntity> CurrentUnit = new ReactiveProperty<BaseUnitEntity>();

	public readonly ReactiveProperty<bool> IsCustomCharacter = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<LevelUpManager> LevelUpManager = new ReactiveProperty<LevelUpManager>();

	private readonly List<IDisposable> m_Disposables = new List<IDisposable>();

	public DollState Doll { get; private set; }

	public int CurrentTattooSet { get; private set; }

	private BaseUnitEntity BaseChargenUnit => CharGenConfig.Unit;

	public CharGenContext(CharGenConfig config)
	{
		CharGenConfig = config;
		AddDisposable(EventBus.Subscribe(this));
		void AddDisposable(IDisposable disposable)
		{
			m_Disposables.AddUnique(disposable);
		}
	}

	public void SetPregenUnit(BaseUnitEntity unit)
	{
		bool flag = unit != null;
		BaseUnitEntity baseUnitEntity = (flag ? unit : BaseChargenUnit);
		Doll = new DollState();
		IsCustomCharacter.Value = !flag;
		CurrentUnit.Value = baseUnitEntity;
		if (flag)
		{
			PregenDollSettings component = baseUnitEntity.Blueprint.GetComponent<PregenDollSettings>();
			if (component != null)
			{
				Doll.Setup(unit, component);
			}
		}
		else
		{
			Doll.SetTrackPortrait(state: true);
			Doll.UpdateMechanicsEntities(baseUnitEntity);
		}
		InitLevelUpManager(baseUnitEntity);
	}

	public void InitForChangeAppearance()
	{
		IsCustomCharacter.Value = true;
		BaseUnitEntity baseChargenUnit = BaseChargenUnit;
		CurrentUnit.Value = baseChargenUnit;
		Doll = new DollState();
		Doll.SetupFromUnit(baseChargenUnit);
		InitLevelUpManager(baseChargenUnit);
	}

	private void InitLevelUpManager(BaseUnitEntity unit)
	{
		BlueprintOriginPath originPath = GetOriginPath(!IsCustomCharacter.Value);
		LevelUpManager.Value?.Dispose();
		LevelUpManager.Value = new LevelUpManager(unit, originPath, autoCommit: false);
	}

	private BlueprintOriginPath GetOriginPath(bool isPregen)
	{
		return CharGenConfig.Mode switch
		{
			CharGenConfig.CharGenMode.Appearance => BlueprintCharGenRoot.Instance.ChangeAppearanceChargenPath, 
			CharGenConfig.CharGenMode.NewGame => isPregen ? BlueprintCharGenRoot.Instance.NewGamePregenChargenPath : BlueprintCharGenRoot.Instance.NewGameCustomChargenPath, 
			CharGenConfig.CharGenMode.NewCompanion => CharGenConfig.CompanionType switch
			{
				CharGenConfig.CharGenCompanionType.Common => isPregen ? BlueprintCharGenRoot.Instance.NewCompanionPregenChargenPath : BlueprintCharGenRoot.Instance.NewCompanionCustomChargenPath, 
				CharGenConfig.CharGenCompanionType.Navigator => isPregen ? BlueprintCharGenRoot.Instance.NewCompanionNavigatorPregenChargenPath : BlueprintCharGenRoot.Instance.NewCompanionNavigatorCustomChargenPath, 
				_ => null, 
			}, 
			_ => null, 
		};
	}

	public void RequestSetGender(Gender gender, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetGender(gender, index);
	}

	void ICharGenDollStateHandler.HandleSetGender(Gender gender, int index)
	{
		Doll.SetGender(gender);
	}

	public void RequestSetHead([NotNull] EquipmentEntityLink head, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetHead(head, index);
	}

	void ICharGenDollStateHandler.HandleSetHead(EquipmentEntityLink head, int index)
	{
		Doll.SetHead(head);
	}

	public void RequestSetRace([NotNull] BlueprintRaceVisualPreset blueprint, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetRace(blueprint, index);
	}

	void ICharGenDollStateHandler.HandleSetRace(BlueprintRaceVisualPreset blueprint, int index)
	{
		Doll.SetRacePreset(blueprint);
	}

	public void RequestSetSkinColor(int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetSkinColor(index);
	}

	void ICharGenDollStateHandler.HandleSetSkinColor(int index)
	{
		Doll.SetSkinColor(index);
	}

	public void RequestSetHair([NotNull] EquipmentEntityLink equipmentEntityLink, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetHair(equipmentEntityLink, index);
	}

	void ICharGenDollStateHandler.HandleSetHair(EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetHair(equipmentEntityLink);
	}

	public void RequestSetHairColor(int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetHairColor(index);
	}

	void ICharGenDollStateHandler.HandleSetHairColor(int index)
	{
		Doll.SetHairColor(index);
	}

	public void RequestSetEyebrows([NotNull] EquipmentEntityLink equipmentEntityLink, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetEyebrows(equipmentEntityLink, index);
	}

	void ICharGenDollStateHandler.HandleSetEyebrows(EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetEyebrows(equipmentEntityLink);
	}

	public void RequestSetEyebrowsColor(int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetEyebrowsColor(index);
	}

	void ICharGenDollStateHandler.HandleSetEyebrowsColor(int index)
	{
		Doll.SetEyebrowsColor(index);
	}

	public void RequestSetBeard([NotNull] EquipmentEntityLink equipmentEntityLink, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetBeard(equipmentEntityLink, index);
	}

	void ICharGenDollStateHandler.HandleSetBeard(EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetBeard(equipmentEntityLink);
	}

	public void RequestSetBeardColor(int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetBeardColor(index);
	}

	void ICharGenDollStateHandler.HandleSetBeardColor(int index)
	{
		Doll.SetBeardColor(index);
	}

	public void RequestSetScar([NotNull] EquipmentEntityLink equipmentEntityLink, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetScar(equipmentEntityLink, index);
	}

	void ICharGenDollStateHandler.HandleSetScar(EquipmentEntityLink equipmentEntityLink, int index)
	{
		Doll.SetScar(equipmentEntityLink);
	}

	public void RequestSetTattoo([NotNull] EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		Game.Instance.GameCommandQueue.CharGenSetTattoo(equipmentEntityLink, index, tattooTabIndex);
	}

	void ICharGenDollStateHandler.HandleSetTattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		CurrentTattooSet = tattooTabIndex;
		Doll.SetTattoo(equipmentEntityLink, tattooTabIndex);
		EventBus.RaiseEvent(delegate(ICharGenAppearanceComponentUpdateHandler h)
		{
			h.HandleAppearanceComponentUpdate(CharGenAppearancePageComponent.Tattoo);
		});
	}

	public void RequestSetTattooColor(int rampIndex, int index)
	{
		Game.Instance.GameCommandQueue.CharGenSetTattooColor(rampIndex, index);
	}

	void ICharGenDollStateHandler.HandleSetTattooColor(int rampIndex, int index)
	{
		Doll.SetTattooColor(rampIndex, index);
	}

	public void RequestSetPort([NotNull] EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
		Game.Instance.GameCommandQueue.CharGenSetPort(equipmentEntityLink, index, portNumber);
	}

	void ICharGenDollStateHandler.HandleSetPort(EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
		Doll.SetPort(equipmentEntityLink, portNumber);
	}

	void ICharGenDollStateHandler.HandleSetEquipmentColor(int primaryIndex, int secondaryIndex)
	{
		Doll.SetEquipColors(primaryIndex, secondaryIndex);
	}

	void ICharGenDollStateHandler.HandleShowCloth(bool showCloth)
	{
		Doll.ShowCloth = showCloth;
	}

	public void Dispose()
	{
		LevelUpManager.Value?.Dispose();
		foreach (IDisposable disposable in m_Disposables)
		{
			disposable.Dispose();
		}
		m_Disposables.Clear();
	}

	public void HandleTextureSelectorTabChange(CharGenAppearancePageComponent type, int tabIndex)
	{
		if (type == CharGenAppearancePageComponent.Tattoo)
		{
			CurrentTattooSet = tabIndex;
		}
	}
}
