using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.CombinedSelector;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.TextureSelector;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Voice;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components;

public static class CharGenAppearanceComponentFactory
{
	public static BaseCharGenAppearancePageComponentVM GetComponent(CharGenAppearancePageComponent componentType, CharGenContext ctx)
	{
		return componentType switch
		{
			CharGenAppearancePageComponent.Portraits => GetPortraitsSelectorVM(ctx), 
			CharGenAppearancePageComponent.Gender => GetGenderSelectorVM(ctx), 
			CharGenAppearancePageComponent.FaceType => GetFaceSelectorVM(ctx), 
			CharGenAppearancePageComponent.BodyType => GetBodySelectorVM(ctx), 
			CharGenAppearancePageComponent.SkinColour => GetBodyColorSelectorVM(ctx), 
			CharGenAppearancePageComponent.HairType => GetHairSelectorVM(ctx), 
			CharGenAppearancePageComponent.HairColour => GetHairColorSelectorVM(ctx), 
			CharGenAppearancePageComponent.EyebrowType => GetEyebrowsSelectorVM(ctx), 
			CharGenAppearancePageComponent.EyebrowColour => GetEyebrowsColorSelectorVM(ctx), 
			CharGenAppearancePageComponent.BeardType => GetBeardSelectorVM(ctx), 
			CharGenAppearancePageComponent.BeardColour => GetBeardColorSelectorVM(ctx), 
			CharGenAppearancePageComponent.ScarsType => GetScarsSelectorVM(ctx), 
			CharGenAppearancePageComponent.FacePaint => GetFacePaintSelectorVM(ctx), 
			CharGenAppearancePageComponent.Tattoo => GetTattooSelectorVM(ctx), 
			CharGenAppearancePageComponent.TattooColor => GetTattooColorSelector(ctx), 
			CharGenAppearancePageComponent.PortType1 => GetPortsSelectorVM(ctx, 0), 
			CharGenAppearancePageComponent.PortType2 => GetPortsSelectorVM(ctx, 1), 
			CharGenAppearancePageComponent.NavigatorMutations => GetNavigatorMutationsSelectorVM(ctx), 
			CharGenAppearancePageComponent.VoiceType => GetVoiceSelectorVM(ctx), 
			CharGenAppearancePageComponent.ServoSkullType => GetServoSkullSelectorVM(ctx), 
			_ => null, 
		};
	}

	public static void UpdateComponent(CharGenAppearancePageComponent componentType, VirtualListElementVMBase component, CharGenContext ctx)
	{
		switch (componentType)
		{
		case CharGenAppearancePageComponent.Gender:
			UpdateGenderSelectorVM(ctx, component as TextureSequentialSelectorVM);
			break;
		case CharGenAppearancePageComponent.FaceType:
			UpdateFaceSelectorVM(ctx, component as SlideSequentialSelectorVM);
			break;
		case CharGenAppearancePageComponent.BodyType:
			UpdateBodySelectorVM(ctx, component as SlideSequentialSelectorVM);
			break;
		case CharGenAppearancePageComponent.SkinColour:
			UpdateBodyColorList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.HairType:
			UpdateHairList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.HairColour:
			UpdateHairColorList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.EyebrowType:
			UpdateEyebrowsList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.EyebrowColour:
			UpdateEyebrowColorList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.BeardType:
			UpdateBeardList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.BeardColour:
			UpdateBeardColorList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.FacePaint:
			UpdateFacePaintSelector(ctx, component as SlideTextureCombinedSelectorVM);
			break;
		case CharGenAppearancePageComponent.ScarsType:
			UpdateScarsSelectorVM(ctx, component as SlideSequentialSelectorVM);
			break;
		case CharGenAppearancePageComponent.Tattoo:
			UpdateTattooSelector(ctx, component as TextureSelectorTabsVM);
			break;
		case CharGenAppearancePageComponent.TattooColor:
			UpdateTattooColorList(ctx, ctx.CurrentTattooSet, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.PortType1:
			UpdatePortsList(ctx, component as TextureSelectorVM, 0);
			break;
		case CharGenAppearancePageComponent.PortType2:
			UpdatePortsList(ctx, component as TextureSelectorVM, 1);
			break;
		case CharGenAppearancePageComponent.NavigatorMutations:
			UpdateNavigatorMutationsList(ctx, component as TextureSelectorVM);
			break;
		case CharGenAppearancePageComponent.VoiceType:
		case CharGenAppearancePageComponent.ServoSkullType:
			break;
		}
	}

	private static CharGenPortraitsSelectorVM GetPortraitsSelectorVM(CharGenContext ctx)
	{
		return new CharGenPortraitsSelectorVM(ctx)
		{
			Type = CharGenAppearancePageComponent.Portraits
		};
	}

	private static TextureSequentialSelectorVM GetGenderSelectorVM(CharGenContext ctx)
	{
		TextureSequentialEntity current;
		TextureSequentialSelectorVM textureSequentialSelectorVM = new TextureSequentialSelectorVM(GetGenderList(ctx, out current), current);
		textureSequentialSelectorVM.Type = CharGenAppearancePageComponent.Gender;
		textureSequentialSelectorVM.SetTitle(UIStrings.Instance.CharGen.BodyType);
		return textureSequentialSelectorVM;
	}

	private static void UpdateGenderSelectorVM(CharGenContext ctx, TextureSequentialSelectorVM selectorVM)
	{
		TextureSequentialEntity current;
		List<TextureSequentialEntity> genderList = GetGenderList(ctx, out current);
		selectorVM.SetValues(genderList, current);
	}

	private static List<TextureSequentialEntity> GetGenderList(CharGenContext ctx, out TextureSequentialEntity current)
	{
		List<TextureSequentialEntity> list = new List<TextureSequentialEntity>();
		current = null;
		foreach (Gender gender in Enum.GetValues(typeof(Gender)))
		{
			Sprite genderIcon = UIConfig.Instance.UIIcons.GetGenderIcon(gender);
			TextureSequentialEntity textureSequentialEntity = new TextureSequentialEntity
			{
				Texture = genderIcon,
				Setter = delegate
				{
					ctx.RequestSetGender(gender, (int)gender);
				}
			};
			list.Add(textureSequentialEntity);
			if (ctx.Doll.Gender == gender)
			{
				current = textureSequentialEntity;
			}
		}
		return list;
	}

	private static SlideSequentialSelectorVM GetFaceSelectorVM(CharGenContext ctx)
	{
		StringSequentialEntity current;
		SlideSequentialSelectorVM slideSequentialSelectorVM = new SlideSequentialSelectorVM(GetFaceList(ctx, out current), current);
		slideSequentialSelectorVM.Type = CharGenAppearancePageComponent.FaceType;
		slideSequentialSelectorVM.SetTitle(UIStrings.Instance.CharGen.Face);
		return slideSequentialSelectorVM;
	}

	private static void UpdateFaceSelectorVM(CharGenContext ctx, SlideSequentialSelectorVM selectorVM)
	{
		StringSequentialEntity current;
		List<StringSequentialEntity> faceList = GetFaceList(ctx, out current);
		selectorVM.SetValues(faceList, current);
	}

	private static List<StringSequentialEntity> GetFaceList(CharGenContext ctx, out StringSequentialEntity current)
	{
		List<StringSequentialEntity> list = new List<StringSequentialEntity>();
		current = null;
		if (ctx.Doll.Race == null)
		{
			return list;
		}
		CustomizationOptions customizationOptions = ((ctx.Doll.Gender == Gender.Male) ? ctx.Doll.Race.MaleOptions : ctx.Doll.Race.FemaleOptions);
		for (int j = 0; j < customizationOptions.Heads.Length; j++)
		{
			EquipmentEntityLink head = customizationOptions.Heads[j];
			int i1 = j;
			StringSequentialEntity stringSequentialEntity = new StringSequentialEntity
			{
				Setter = delegate
				{
					ctx.RequestSetHead(head, i1);
				}
			};
			list.Add(stringSequentialEntity);
			if (head.Load() == ctx.Doll.Head.Load())
			{
				current = stringSequentialEntity;
			}
		}
		return list;
	}

	private static SlideSequentialSelectorVM GetBodySelectorVM(CharGenContext ctx)
	{
		StringSequentialEntity current;
		SlideSequentialSelectorVM slideSequentialSelectorVM = new SlideSequentialSelectorVM(GetBodyList(ctx, out current), current);
		slideSequentialSelectorVM.Type = CharGenAppearancePageComponent.BodyType;
		slideSequentialSelectorVM.SetTitle(UIStrings.Instance.CharGen.BodyConstitution);
		return slideSequentialSelectorVM;
	}

	private static void UpdateBodySelectorVM(CharGenContext ctx, SlideSequentialSelectorVM selectorVM)
	{
		StringSequentialEntity current;
		List<StringSequentialEntity> bodyList = GetBodyList(ctx, out current);
		selectorVM.SetValues(bodyList, current);
	}

	private static List<StringSequentialEntity> GetBodyList(CharGenContext ctx, out StringSequentialEntity current)
	{
		List<StringSequentialEntity> list = new List<StringSequentialEntity>();
		current = null;
		if (ctx.Doll.Race == null)
		{
			return list;
		}
		for (int j = 0; j < ctx.Doll.Race.Presets.Length; j++)
		{
			BlueprintRaceVisualPreset racePreset = ctx.Doll.Race.Presets[j];
			int i1 = j;
			StringSequentialEntity stringSequentialEntity = new StringSequentialEntity
			{
				Setter = delegate
				{
					ctx.RequestSetRace(racePreset, i1);
				}
			};
			list.Add(stringSequentialEntity);
			if (racePreset == ctx.Doll.RacePreset)
			{
				current = stringSequentialEntity;
			}
		}
		return list;
	}

	private static TextureSelectorVM GetBodyColorSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Paged);
		UpdateBodyColorList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.SkinColour;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.SkinTone);
		return textureSelectorVM;
	}

	private static void UpdateBodyColorList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> skinRamps = doll.GetSkinRamps();
		for (int j = 0; j < skinRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = skinRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetSkinColor(i1);
			});
			if (doll.SkinRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(skinRamps.Count);
	}

	private static TextureSelectorVM GetHairSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateHairList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.HairType;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.HairStyle);
		return textureSelectorVM;
	}

	private static void UpdateHairList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		if (doll.Race == null)
		{
			return;
		}
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		EquipmentEntityLink[] hair = ((doll.Gender == Gender.Male) ? doll.Race.MaleOptions : doll.Race.FemaleOptions).Hair;
		for (int j = 0; j < hair.Length; j++)
		{
			EquipmentEntityLink item = hair[j];
			EquipmentEntity equipmentEntity = item.Load();
			if (!(equipmentEntity == null))
			{
				int i1 = j;
				GetTextureSelectorItemVM(entitiesCollection, j, equipmentEntity.PreviewTexture, delegate
				{
					ctx.RequestSetHair(item, i1);
				});
				if (item.Load() == doll.Hair.Load())
				{
					selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
				}
			}
		}
		selector.SelectionGroup.ClearFromIndex(hair.Length);
	}

	private static TextureSelectorVM GetHairColorSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Paged);
		UpdateHairColorList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.HairColour;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.HairColor);
		return textureSelectorVM;
	}

	private static void UpdateHairColorList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> hairRamps = doll.GetHairRamps();
		for (int j = 0; j < hairRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = hairRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetHairColor(i1);
			});
			if (doll.HairRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(hairRamps.Count);
	}

	private static TextureSelectorVM GetEyebrowsSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateEyebrowsList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.EyebrowType;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.Eyebrows);
		return textureSelectorVM;
	}

	private static void UpdateEyebrowsList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		if (doll.Race == null)
		{
			return;
		}
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		EquipmentEntityLink[] eyebrows = ((doll.Gender == Gender.Male) ? doll.Race.MaleOptions : doll.Race.FemaleOptions).Eyebrows;
		for (int j = 0; j < eyebrows.Length; j++)
		{
			EquipmentEntityLink item = eyebrows[j];
			EquipmentEntity equipmentEntity = item.Load();
			if (!(equipmentEntity == null))
			{
				int i1 = j;
				GetTextureSelectorItemVM(entitiesCollection, j, equipmentEntity.PreviewTexture, delegate
				{
					ctx.RequestSetEyebrows(item, i1);
				});
				if (item.Load() == doll.Eyebrows.Load())
				{
					selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
				}
			}
		}
		selector.SelectionGroup.ClearFromIndex(eyebrows.Length);
	}

	private static TextureSelectorVM GetEyebrowsColorSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Paged);
		UpdateEyebrowColorList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.EyebrowColour;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.EyebrowsColor);
		return textureSelectorVM;
	}

	private static void UpdateEyebrowColorList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> eyebrowsRamps = doll.GetEyebrowsRamps();
		for (int j = 0; j < eyebrowsRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = eyebrowsRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetEyebrowsColor(i1);
			});
			if (doll.EyebrowsColorRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(eyebrowsRamps.Count);
	}

	private static TextureSelectorVM GetBeardSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateBeardList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.BeardType;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.Beard);
		return textureSelectorVM;
	}

	private static void UpdateBeardList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		if (doll.Race == null)
		{
			return;
		}
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		EquipmentEntityLink[] beards = ((doll.Gender == Gender.Male) ? doll.Race.MaleOptions : doll.Race.FemaleOptions).Beards;
		for (int j = 0; j < beards.Length; j++)
		{
			EquipmentEntityLink item = beards[j];
			EquipmentEntity equipmentEntity = item.Load();
			if (!(equipmentEntity == null))
			{
				int i1 = j;
				GetTextureSelectorItemVM(entitiesCollection, j, equipmentEntity.PreviewTexture, delegate
				{
					ctx.RequestSetBeard(item, i1);
				});
				if (item.Load() == doll.Beard.Load())
				{
					selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
				}
			}
		}
		selector.SelectionGroup.ClearFromIndex(beards.Length);
	}

	private static TextureSelectorVM GetBeardColorSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Paged);
		UpdateBeardColorList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.BeardColour;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.BeardColor);
		return textureSelectorVM;
	}

	private static void UpdateBeardColorList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> beardRamps = doll.GetBeardRamps();
		for (int j = 0; j < beardRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = beardRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetBeardColor(i1);
			});
			if (doll.BeardColorRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(beardRamps.Count);
	}

	private static SlideSequentialSelectorVM GetScarsSelectorVM(CharGenContext ctx)
	{
		StringSequentialEntity current;
		SlideSequentialSelectorVM slideSequentialSelectorVM = new SlideSequentialSelectorVM(GetScarsList(ctx, out current), current);
		slideSequentialSelectorVM.Type = CharGenAppearancePageComponent.ScarsType;
		slideSequentialSelectorVM.SetTitle(UIStrings.Instance.CharGen.Scars);
		return slideSequentialSelectorVM;
	}

	private static void UpdateScarsSelectorVM(CharGenContext ctx, SlideSequentialSelectorVM selectorVM)
	{
		StringSequentialEntity current;
		List<StringSequentialEntity> scarsList = GetScarsList(ctx, out current);
		selectorVM.SetValues(scarsList, current);
	}

	private static List<StringSequentialEntity> GetScarsList(CharGenContext ctx, out StringSequentialEntity current)
	{
		DollState doll = ctx.Doll;
		List<StringSequentialEntity> list = new List<StringSequentialEntity>();
		current = null;
		for (int j = 0; j < doll.Scars.Count; j++)
		{
			EquipmentEntityLink scar = doll.Scars[j];
			int i1 = j;
			StringSequentialEntity stringSequentialEntity = new StringSequentialEntity
			{
				Setter = delegate
				{
					ctx.RequestSetScar(scar, i1);
				}
			};
			list.Add(stringSequentialEntity);
			if (scar.Load() == doll.Scar.Load())
			{
				current = stringSequentialEntity;
			}
		}
		return list;
	}

	private static SlideTextureCombinedSelectorVM GetFacePaintSelectorVM(CharGenContext ctx)
	{
		SlideTextureCombinedSelectorVM slideTextureCombinedSelectorVM = new SlideTextureCombinedSelectorVM();
		slideTextureCombinedSelectorVM.Type = CharGenAppearancePageComponent.FacePaint;
		slideTextureCombinedSelectorVM.SetTitle(UIStrings.Instance.CharGen.FacePaint);
		UpdateFacePaintSelector(ctx, slideTextureCombinedSelectorVM);
		return slideTextureCombinedSelectorVM;
	}

	private static void UpdateFacePaintSelector(CharGenContext ctx, SlideTextureCombinedSelectorVM selector)
	{
		IEnumerable<SlideSequentialSelectorVM> facePaintSelectors = GetFacePaintSelectors(ctx);
		IEnumerable<TextureSelectorVM> facePaintColorSelectors = GetFacePaintColorSelectors(ctx);
		selector.SetValues(facePaintSelectors, facePaintColorSelectors);
	}

	private static IEnumerable<SlideSequentialSelectorVM> GetFacePaintSelectors(CharGenContext ctx)
	{
		List<SlideSequentialSelectorVM> list = new List<SlideSequentialSelectorVM>();
		for (int i = 0; i < 5; i++)
		{
			StringSequentialEntity current;
			List<StringSequentialEntity> facePaintList = GetFacePaintList(ctx, i, out current);
			list.Add(new SlideSequentialSelectorVM(facePaintList, current));
		}
		return list;
	}

	private static List<StringSequentialEntity> GetFacePaintList(CharGenContext ctx, int index, out StringSequentialEntity current)
	{
		DollState dollState = ctx.Doll;
		List<StringSequentialEntity> list = new List<StringSequentialEntity>();
		current = null;
		List<DollState.DollPrint> warpaints = dollState.Warpaints;
		if (warpaints.Count == 0 || index > warpaints.Count)
		{
			return list;
		}
		List<EquipmentEntityLink> paints = warpaints[index].Paints;
		for (int j = 0; j < paints.Count; j++)
		{
			EquipmentEntityLink option1 = paints[j];
			int i1 = j;
			StringSequentialEntity stringSequentialEntity = new StringSequentialEntity
			{
				Setter = delegate
				{
					dollState.SetWarpaint(option1, i1);
				}
			};
			list.Add(stringSequentialEntity);
			if (paints[j].Load() == warpaints[index].PaintEE.Load())
			{
				current = stringSequentialEntity;
			}
		}
		return list;
	}

	private static IEnumerable<TextureSelectorVM> GetFacePaintColorSelectors(CharGenContext ctx)
	{
		List<TextureSelectorVM> list = new List<TextureSelectorVM>();
		for (int i = 0; i < 5; i++)
		{
			TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Paged);
			UpdateFacePaintColorList(ctx, i, textureSelectorVM);
			list.Add(textureSelectorVM);
		}
		return list;
	}

	private static void UpdateFacePaintColorList(CharGenContext ctx, int index, TextureSelectorVM selector)
	{
		DollState dollState = ctx.Doll;
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> warpaintRamps = dollState.GetWarpaintRamps();
		for (int j = 0; j < warpaintRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = warpaintRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				dollState.SetWarpaintColor(i1, i1);
			});
			if (dollState.Warpaints[index].PaintRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(warpaintRamps.Count);
	}

	private static TextureSelectorTabsVM GetTattooSelectorVM(CharGenContext ctx)
	{
		TextureSelectorTabsVM textureSelectorTabsVM = new TextureSelectorTabsVM(ctx);
		textureSelectorTabsVM.Type = CharGenAppearancePageComponent.Tattoo;
		textureSelectorTabsVM.SetTitle(UIStrings.Instance.CharGen.Tattoo);
		UpdateTattooSelector(ctx, textureSelectorTabsVM);
		return textureSelectorTabsVM;
	}

	private static void UpdateTattooSelector(CharGenContext ctx, TextureSelectorTabsVM selector)
	{
		IEnumerable<TextureSelectorVM> tattooSelectors = GetTattooSelectors(ctx);
		selector.SetValues(tattooSelectors);
	}

	private static IEnumerable<TextureSelectorVM> GetTattooSelectors(CharGenContext ctx)
	{
		List<TextureSelectorVM> list = new List<TextureSelectorVM>();
		for (int i = 0; i < 5; i++)
		{
			TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Default);
			UpdateTattoosList(ctx, textureSelectorVM, i);
			list.Add(textureSelectorVM);
		}
		return list;
	}

	private static void UpdateTattoosList(CharGenContext ctx, TextureSelectorVM selector, int index)
	{
		DollState doll = ctx.Doll;
		if (doll.Race == null)
		{
			return;
		}
		List<DollState.DollPrint> tattoos = doll.Tattoos;
		if (tattoos.Count == 0 || index > tattoos.Count)
		{
			return;
		}
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		EquipmentEntityLink[] tattoos2 = ((doll.Gender == Gender.Male) ? doll.Race.MaleOptions : doll.Race.FemaleOptions).Tattoos;
		for (int j = 0; j < tattoos2.Length; j++)
		{
			EquipmentEntityLink item = tattoos2[j];
			EquipmentEntity equipmentEntity = item.Load();
			if (!(equipmentEntity == null))
			{
				int i1 = j;
				GetTextureSelectorItemVM(entitiesCollection, j, equipmentEntity.PreviewTexture, delegate
				{
					ctx.RequestSetTattoo(item, i1, index);
				});
				if (item.Load() == tattoos[index].PaintEE.Load())
				{
					selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
				}
			}
		}
		selector.SelectionGroup.ClearFromIndex(tattoos2.Length);
	}

	private static TextureSelectorVM GetTattooColorSelector(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Paged, hideIfNoElements: false);
		textureSelectorVM.Type = CharGenAppearancePageComponent.TattooColor;
		UpdateTattooColorList(ctx, ctx.CurrentTattooSet, textureSelectorVM);
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.TattooColor);
		textureSelectorVM.SetNoItemsDescription(UIStrings.Instance.CharGen.NothingToChoose);
		return textureSelectorVM;
	}

	private static void UpdateTattooColorList(CharGenContext ctx, int index, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> tattooRamps = doll.GetTattooRamps(index);
		for (int j = 0; j < tattooRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = tattooRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, j, item, delegate
			{
				ctx.RequestSetTattooColor(i1, index);
			});
			if (doll.Tattoos[index].PaintRampIndex == j)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(tattooRamps.Count);
	}

	private static TextureSelectorVM GetPortsSelectorVM(CharGenContext ctx, int index)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdatePortsList(ctx, textureSelectorVM, index);
		textureSelectorVM.Type = ((index == 0) ? CharGenAppearancePageComponent.PortType1 : CharGenAppearancePageComponent.PortType2);
		textureSelectorVM.SetTitle(string.Format(UIStrings.Instance.CharGen.Implant.Text, (index + 1).ToString()));
		return textureSelectorVM;
	}

	private static void UpdatePortsList(CharGenContext ctx, TextureSelectorVM selector, int index)
	{
		DollState doll = ctx.Doll;
		if (doll.Race == null)
		{
			return;
		}
		List<DollState.DollPrint> ports = doll.Ports;
		if (ports.Count == 0 || index > ports.Count)
		{
			return;
		}
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		EquipmentEntityLink[] ports2 = ((doll.Gender == Gender.Male) ? doll.Race.MaleOptions : doll.Race.FemaleOptions).Ports;
		for (int j = 0; j < ports2.Length; j++)
		{
			EquipmentEntityLink item = ports2[j];
			EquipmentEntity equipmentEntity = item.Load();
			if (!(equipmentEntity == null))
			{
				int i1 = j;
				GetTextureSelectorItemVM(entitiesCollection, j, equipmentEntity.PreviewTexture, delegate
				{
					ctx.RequestSetPort(item, i1, index);
				});
				if (item.Load() == ports[index].PaintEE.Load())
				{
					selector.SelectionGroup.TrySelectEntity(entitiesCollection[j]);
				}
			}
		}
		selector.SelectionGroup.ClearFromIndex(ports2.Length);
	}

	private static TextureSelectorVM GetNavigatorMutationsSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateNavigatorMutationsList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.NavigatorMutations;
		textureSelectorVM.SetTitle(UIStrings.Instance.CharGen.NavigatorMutations);
		return textureSelectorVM;
	}

	private static void UpdateNavigatorMutationsList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState dollState = ctx.Doll;
		if (dollState.Race == null)
		{
			return;
		}
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		EquipmentEntityLink[] navigatorMutations = ((dollState.Gender == Gender.Male) ? dollState.Race.MaleOptions : dollState.Race.FemaleOptions).NavigatorMutations;
		for (int i = 0; i < navigatorMutations.Length; i++)
		{
			EquipmentEntityLink item = navigatorMutations[i];
			EquipmentEntity equipmentEntity = item.Load();
			if (!(equipmentEntity == null))
			{
				GetTextureSelectorItemVM(entitiesCollection, i, equipmentEntity.PreviewTexture, delegate
				{
					dollState.SetNavigatorMutation(item);
				});
				if (item.Load() == dollState.NavigatorMutation.Load())
				{
					selector.SelectionGroup.TrySelectEntity(entitiesCollection[i]);
				}
			}
		}
		selector.SelectionGroup.ClearFromIndex(navigatorMutations.Length);
	}

	private static CharGenVoiceSelectorVM GetVoiceSelectorVM(CharGenContext ctx)
	{
		return new CharGenVoiceSelectorVM(ctx)
		{
			Type = CharGenAppearancePageComponent.VoiceType
		};
	}

	private static TextureSelectorVM GetServoSkullSelectorVM(CharGenContext ctx)
	{
		TextureSelectorVM textureSelectorVM = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ReactiveCollection<TextureSelectorItemVM>()), TextureSelectorType.Default);
		UpdateServoSkullList(ctx, textureSelectorVM);
		textureSelectorVM.Type = CharGenAppearancePageComponent.ServoSkullType;
		textureSelectorVM.SetTitle("ServoSkull");
		textureSelectorVM.SetDescription("ServoSkull description");
		return textureSelectorVM;
	}

	private static void UpdateServoSkullList(CharGenContext ctx, TextureSelectorVM selector)
	{
		DollState doll = ctx.Doll;
		ReactiveCollection<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		List<Texture2D> hairRamps = doll.GetHairRamps();
		int num = 0;
		for (int j = 0; j < hairRamps.Count; j++)
		{
			int i1 = j;
			Texture2D item = hairRamps[j];
			GetTextureSelectorItemVM(entitiesCollection, num, item, delegate
			{
				UILog.Log($"Set ServoSkull: {i1}");
			});
			entitiesCollection[num].IsSelected.Value = doll.HairRampIndex == j;
			num++;
		}
		selector.SelectionGroup.ClearFromIndex(hairRamps.Count);
	}

	public static void GetTextureSelectorItemVM(ReactiveCollection<TextureSelectorItemVM> valueList, int i, Texture2D item, Action setter)
	{
		if (valueList.Count > i)
		{
			valueList[i].UpdateTextureAndSetter(item, setter);
		}
		else
		{
			valueList.Add(new TextureSelectorItemVM(item, setter, i));
		}
	}
}
