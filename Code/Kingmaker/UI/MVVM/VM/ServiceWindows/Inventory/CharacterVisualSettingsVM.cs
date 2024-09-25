using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.TextureSelector;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;

public class CharacterVisualSettingsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICharGenVisualHandler, ISubscriber
{
	private class TextureIndexPair
	{
		public Texture2D Texture;

		public RampColorPreset.IndexSet TexturesIndexSet;
	}

	public readonly BaseUnitEntity Unit;

	private readonly DollState m_DollState;

	public readonly CharacterVisualSettingsEntityVM Cloth;

	public readonly CharacterVisualSettingsEntityVM Helmet;

	public readonly CharacterVisualSettingsEntityVM HelmetAboveAll;

	public readonly CharacterVisualSettingsEntityVM Backpack;

	public readonly TextureSelectorVM OutfitMainColorSelector;

	private readonly Action m_DisposeAction;

	private CharacterVisualSettingsVM(Action disposeAction)
	{
		m_DisposeAction = disposeAction;
		UISounds.Instance.Sounds.Inventory.InventoryVisualSettingsShow.Play();
	}

	public CharacterVisualSettingsVM(DollState dollState, Action disposeAction)
		: this(disposeAction)
	{
		m_DollState = dollState;
		if (dollState != null)
		{
			UISounds.Instance.Sounds.Inventory.InventoryVisualSettingsShow.Play();
			OutfitMainColorSelector = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Paged);
			CreateOutfitColorSelector();
			AddDisposable(EventBus.Subscribe(this));
			AddDisposable(Cloth = new CharacterVisualSettingsEntityVM(m_DollState.ShowCloth, SwitchCloth));
			AddDisposable(Helmet = new CharacterVisualSettingsEntityVM(m_DollState.ShowHelm, SwitchHelmet));
			AddDisposable(HelmetAboveAll = new CharacterVisualSettingsEntityVM(m_DollState.ShowHelmAboveAll, SwitchHelmetAboveAll));
			AddDisposable(Backpack = new CharacterVisualSettingsEntityVM(m_DollState.ShowBackpack, SwitchBackpack));
			Helmet.SetValue(m_DollState.ShowHelm && m_DollState.ShowCloth);
			HelmetAboveAll.SetValue(m_DollState.ShowHelmAboveAll);
			Backpack.SetValue(m_DollState.ShowBackpack && m_DollState.ShowCloth);
			Helmet.SetLock(!m_DollState.ShowCloth);
			Backpack.SetLock(!m_DollState.ShowCloth && Unit.HasMechadendrites());
		}
	}

	public CharacterVisualSettingsVM(BaseUnitEntity unit, Action disposeAction)
		: this(disposeAction)
	{
		Unit = unit;
		if (unit != null)
		{
			UISounds.Instance.Sounds.Inventory.InventoryVisualSettingsShow.Play();
			OutfitMainColorSelector = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Paged);
			CreateOutfitColorSelector();
			AddDisposable(Helmet = new CharacterVisualSettingsEntityVM(Unit.UISettings.ShowHelm, SwitchHelmet));
			AddDisposable(HelmetAboveAll = new CharacterVisualSettingsEntityVM(Unit.UISettings.ShowHelmAboveAll, SwitchHelmetAboveAll));
			AddDisposable(Backpack = new CharacterVisualSettingsEntityVM(Unit.UISettings.ShowBackpack, SwitchBackpack));
			Backpack.SetLock(Unit.HasMechadendrites());
		}
	}

	protected override void DisposeImplementation()
	{
		UISounds.Instance.Sounds.Inventory.InventoryVisualSettingsHide.Play();
	}

	public void Close()
	{
		m_DisposeAction?.Invoke();
	}

	private void SwitchCloth()
	{
		bool showCloth = !m_DollState.ShowCloth;
		Game.Instance.GameCommandQueue.CharGenSwitchCloth(showCloth);
	}

	void ICharGenVisualHandler.HandleShowCloth(bool showCloth)
	{
		if (m_DollState != null)
		{
			Helmet.SetValue(m_DollState.ShowCloth);
			Backpack.SetValue(m_DollState.ShowCloth);
			Helmet.SetLock(!m_DollState.ShowCloth);
			Backpack.SetLock(!m_DollState.ShowCloth && Unit.HasMechadendrites());
			UpdateClothesPrimaryPaintColorList(OutfitMainColorSelector, secondary: false);
			OutfitMainColorSelector.SetActiveState(m_DollState.ShowCloth);
		}
	}

	private void SwitchHelmet()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowHelm = !m_DollState.ShowHelm;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowHelm = !Unit.UISettings.ShowHelm;
		}
	}

	private void SwitchHelmetAboveAll()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowHelmAboveAll = !m_DollState.ShowHelmAboveAll;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowHelmAboveAll = !Unit.UISettings.ShowHelmAboveAll;
		}
	}

	private void SwitchBackpack()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowBackpack = !m_DollState.ShowBackpack;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowBackpack = !Unit.UISettings.ShowBackpack;
		}
	}

	private void CreateOutfitColorSelector(bool secondary = false)
	{
		UpdateClothesPrimaryPaintColorList(OutfitMainColorSelector, secondary);
		string title = (secondary ? UIStrings.Instance.CharGen.SecondaryClothColor : UIStrings.Instance.CharGen.PrimaryClothColor);
		OutfitMainColorSelector.SetTitle(title);
		OutfitMainColorSelector.SetNoItemsDescription((m_DollState == null) ? UIStrings.Instance.CharacterSheet.VisualSettingsDisabledForCharacter : UIStrings.Instance.CharacterSheet.VisualSettingsEnableClothes);
	}

	private void UpdateClothesPrimaryPaintColorList(TextureSelectorVM selector, bool secondary)
	{
		List<EquipmentEntityLink> list = m_DollState?.Clothes;
		if (list == null && Unit != null)
		{
			IEnumerable<KingmakerEquipmentEntity> unitEquipmentEntities = CharGenUtility.GetUnitEquipmentEntities(Unit);
			if (unitEquipmentEntities.Any())
			{
				Race race = Unit.Progression.Race?.RaceId ?? Race.Human;
				list = CharGenUtility.GetClothes(unitEquipmentEntities, Unit.Gender, race).ToList();
			}
		}
		if (list == null)
		{
			return;
		}
		RampColorPreset colorPreset;
		CharacterColorsProfile clothesColorsProfile = CharGenUtility.GetClothesColorsProfile(list, out colorPreset, secondary);
		if (colorPreset == null)
		{
			return;
		}
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<TextureIndexPair> listTexIndexPrimary = new List<TextureIndexPair>();
		foreach (RampColorPreset.IndexSet indexPair in colorPreset.IndexPairs)
		{
			TextureIndexPair item = new TextureIndexPair
			{
				Texture = clothesColorsProfile.Ramps[indexPair.PrimaryIndex],
				TexturesIndexSet = indexPair
			};
			listTexIndexPrimary.Add(item);
		}
		for (int j = 0; j < listTexIndexPrimary.Count; j++)
		{
			int i1 = j;
			TextureIndexPair textureIndexPair = listTexIndexPrimary[j];
			CharGenAppearanceComponentFactory.GetTextureSelectorItemVM(entitiesCollection, j, textureIndexPair.Texture, delegate
			{
				SetEquipmentColor(listTexIndexPrimary[i1].TexturesIndexSet, secondary: false);
			});
			if (GetCurrentEquipmentRampIndex(secondary: false) == textureIndexPair.TexturesIndexSet.PrimaryIndex && GetCurrentEquipmentRampIndex(secondary: true) == textureIndexPair.TexturesIndexSet.SecondaryIndex)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(listTexIndexPrimary.Count);
	}

	private int GetCurrentEquipmentRampIndex(bool secondary)
	{
		if (m_DollState != null)
		{
			if (!secondary)
			{
				return m_DollState.EquipmentRampIndex;
			}
			return m_DollState.EquipmentRampIndexSecondary;
		}
		if (Unit?.ViewSettings.Doll != null)
		{
			if (!secondary)
			{
				return Unit.ViewSettings.Doll.ClothesPrimaryIndex;
			}
			return Unit.ViewSettings.Doll.ClothesSecondaryIndex;
		}
		return -1;
	}

	private void SetEquipmentColor(RampColorPreset.IndexSet rampIndex, bool secondary)
	{
		if (m_DollState != null)
		{
			if (m_DollState.EquipmentRampIndex != rampIndex.PrimaryIndex || m_DollState.EquipmentRampIndexSecondary != rampIndex.SecondaryIndex)
			{
				Game.Instance.GameCommandQueue.CharGenSetEquipmentColor(rampIndex.PrimaryIndex, rampIndex.SecondaryIndex);
			}
		}
		else if (Unit != null)
		{
			Game.Instance.GameCommandQueue.SetEquipmentColor(Unit, rampIndex);
		}
	}
}
