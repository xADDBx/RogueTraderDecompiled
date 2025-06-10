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

	public bool IsPet
	{
		get
		{
			if (m_DollState == null)
			{
				return Unit?.IsPet ?? false;
			}
			return false;
		}
	}

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
			TryCreateOutfitColorSelector();
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
			TryCreateOutfitColorSelector();
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
			TryUpdateClothesPrimaryPaintColorList(OutfitMainColorSelector, secondary: false);
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

	private void TryCreateOutfitColorSelector(bool secondary = false)
	{
		if (!TryUpdateClothesPrimaryPaintColorList(OutfitMainColorSelector, secondary) && !TryUpdatePetColorListFromPet(OutfitMainColorSelector))
		{
			TryUpdatePetColorListFromMaster(OutfitMainColorSelector);
		}
		string title = (secondary ? UIStrings.Instance.CharGen.SecondaryClothColor : UIStrings.Instance.CharGen.PrimaryClothColor);
		OutfitMainColorSelector.SetTitle(title);
		OutfitMainColorSelector.SetNoItemsDescription((m_DollState == null) ? UIStrings.Instance.CharacterSheet.VisualSettingsDisabledForCharacter : UIStrings.Instance.CharacterSheet.VisualSettingsEnableClothes);
	}

	private bool TryUpdatePetColorListFromPet(TextureSelectorVM selector)
	{
		if (!IsPet)
		{
			return false;
		}
		if (m_DollState != null)
		{
			return false;
		}
		_ = Unit;
		return false;
	}

	private Texture2D CreateColorTexture(Color color)
	{
		Texture2D texture2D = new Texture2D(32, 32);
		Color[] array = new Color[1024];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = color;
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	private Color GetFallbackColorForIndex(int index)
	{
		Color[] array = new Color[8]
		{
			Color.red,
			Color.green,
			Color.blue,
			Color.yellow,
			Color.magenta,
			Color.cyan,
			Color.white,
			Color.gray
		};
		return array[index % array.Length];
	}

	private bool TryUpdatePetColorListFromMaster(TextureSelectorVM selector)
	{
		if (!IsPet)
		{
			return false;
		}
		if (m_DollState != null)
		{
			return false;
		}
		if (Unit == null)
		{
			return false;
		}
		PetCharacter componentInChildren = Unit.View.gameObject.GetComponentInChildren<PetCharacter>();
		if (componentInChildren == null)
		{
			PFLog.TechArt.Warning("TryUpdatePetColorListFromMaster: PetCharacter component not found on pet: " + Unit.CharacterName);
			return false;
		}
		if (componentInChildren.RampColorPresetFile == null || componentInChildren.RampColorPresetFile.IndexPairs == null || componentInChildren.RampColorPresetFile.IndexPairs.Count == 0)
		{
			PFLog.TechArt.Log("TryUpdatePetColorListFromMaster: No ramps found in pet's RampColorPresetFile for pet: " + Unit.CharacterName);
			return false;
		}
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		int num = 0;
		PFLog.TechArt.Log($"TryUpdatePetColorListFromMaster: Adding {componentInChildren.RampColorPresetFile.IndexPairs.Count} ramps from pet");
		for (int i = 0; i < componentInChildren.RampColorPresetFile.IndexPairs.Count; i++)
		{
			int petPresetIndex = i;
			RampColorPreset.IndexSet petPreset = componentInChildren.RampColorPresetFile.IndexPairs[i];
			Texture2D texture2D = componentInChildren.GetRampTextureByIndex(petPreset.PrimaryIndex);
			if (texture2D == null)
			{
				texture2D = CreateColorTexture(GetFallbackColorForIndex(petPresetIndex));
			}
			RampColorPreset.IndexSet petRampIndexSet = new RampColorPreset.IndexSet
			{
				PrimaryIndex = petPreset.PrimaryIndex,
				SecondaryIndex = petPreset.SecondaryIndex,
				Name = (petPreset.Name ?? $"Pet Ramp {petPreset.PrimaryIndex}")
			};
			CharGenAppearanceComponentFactory.GetTextureSelectorItemVM(entitiesCollection, num, texture2D, delegate
			{
				PFLog.TechArt.Log($"TryUpdatePetColorListFromMaster: Selected pet preset {petPresetIndex} (ramp {petPreset.PrimaryIndex}) for pet: {Unit.CharacterName}");
				SetEquipmentColor(petRampIndexSet);
			});
			if (GetCurrentPetRampIndex() == petPreset.PrimaryIndex)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[num]);
			}
			num++;
		}
		selector.SelectionGroup.ClearFromIndex(num);
		return true;
	}

	private bool TryUpdateClothesPrimaryPaintColorList(TextureSelectorVM selector, bool secondary)
	{
		if (IsPet)
		{
			return false;
		}
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
			return false;
		}
		RampColorPreset colorPreset;
		CharacterColorsProfile clothesColorsProfile = CharGenUtility.GetClothesColorsProfile(list, out colorPreset, secondary);
		if (colorPreset == null)
		{
			return false;
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
				SetEquipmentColor(listTexIndexPrimary[i1].TexturesIndexSet);
			});
			if (GetCurrentEquipmentRampIndex(secondary: false) == textureIndexPair.TexturesIndexSet.PrimaryIndex && GetCurrentEquipmentRampIndex(secondary: true) == textureIndexPair.TexturesIndexSet.SecondaryIndex)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(listTexIndexPrimary.Count);
		return true;
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

	private int GetCurrentPetColorIndex()
	{
		if (Unit?.ViewSettings.Doll != null)
		{
			return Unit.ViewSettings.Doll.PetColorRampIndex;
		}
		return -1;
	}

	private int GetCurrentPetRampIndex()
	{
		if (Unit?.ViewSettings.Doll != null)
		{
			return Unit.ViewSettings.Doll.PetRamp01Index;
		}
		return -1;
	}

	private void SetEquipmentColor(RampColorPreset.IndexSet rampIndex)
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
