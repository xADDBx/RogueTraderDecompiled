using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Levelup.Selections.Doll;

public class DollState : ICanConvertPropertiesToReactive
{
	public struct EEAdapter
	{
		private EquipmentEntityLink m_Link;

		public string AssetId => m_Link.AssetId;

		public EEAdapter(EquipmentEntityLink link)
		{
			m_Link = link;
		}

		public EquipmentEntity Load()
		{
			return m_Link?.Load(ignorePreloadWarning: true);
		}

		public EquipmentEntityLink GetLink()
		{
			return m_Link;
		}
	}

	public class DollPrint
	{
		public EEAdapter PaintEE;

		public List<EquipmentEntityLink> Paints;

		public int PaintRampIndex = -1;

		public void SetPrimaryRampIndex(Character character)
		{
			if (PaintRampIndex >= 0 && PaintEE.Load() != null)
			{
				character.SetPrimaryRampIndex(PaintEE.Load(), PaintRampIndex);
			}
		}
	}

	private delegate int GetRampIndexDelegate(EEAdapter eeAdapter, bool secondary = false);

	[NotNull]
	private static readonly List<Texture2D> s_EmptyRamps = new List<Texture2D>();

	public List<DollPrint> Warpaints = new List<DollPrint>(5);

	public List<DollPrint> Tattoos = new List<DollPrint>(5);

	public List<DollPrint> Ports = new List<DollPrint>(2);

	public const int WarpaintsNumber = 5;

	public const int TattoosNumber = 5;

	public const int PortsNumber = 2;

	public int HairRampIndex = -1;

	public int SkinRampIndex = -1;

	public int EyesColorRampIndex = -1;

	public int EyebrowsColorRampIndex = -1;

	public int BeardColorRampIndex = -1;

	public int HornsRampIndex = -1;

	public int EquipmentRampIndex = -1;

	public int EquipmentRampIndexSecondary = -1;

	private PregenDollSettings.Entry m_DefaultSettings;

	private bool m_TrackPortrait = true;

	[NotNull]
	private List<EquipmentEntityLink> m_MechanicsEntities = new List<EquipmentEntityLink>();

	private List<KingmakerEquipmentEntity> m_EquipmentEntities = new List<KingmakerEquipmentEntity>();

	private ReactiveCommand m_UpdateCommand;

	private bool m_ShowClothTemp = true;

	private bool m_ShowCloth = true;

	private bool m_ShowHelmTemp = true;

	private bool m_ShowHelm = true;

	private bool m_ShowHelmAboveAll;

	private bool m_ShowBackpack = true;

	[NotNull]
	private static BlueprintCharGenRoot Root => BlueprintRoot.Instance.CharGenRoot;

	public Gender Gender { get; private set; }

	[CanBeNull]
	public BlueprintRace Race { get; private set; }

	[CanBeNull]
	public BlueprintRaceVisualPreset RacePreset { get; private set; }

	public EEAdapter Head { get; private set; }

	public EEAdapter Scar { get; private set; }

	public EEAdapter Eyebrows { get; private set; }

	public EEAdapter Hair { get; private set; }

	public EEAdapter Beard { get; private set; }

	public EEAdapter Horn { get; private set; }

	public EEAdapter NavigatorMutation { get; private set; }

	public BlueprintPortrait Portrait { get; private set; }

	public PortraitData PortraitData => Portrait?.Data;

	public List<EquipmentEntityLink> Clothes { get; private set; } = new List<EquipmentEntityLink>();


	public List<EquipmentEntityLink> Scars { get; private set; } = new List<EquipmentEntityLink>();


	public bool LeftHanded { get; private set; }

	public bool TrackPortrait => m_TrackPortrait;

	public ReactiveCommand UpdateCommand
	{
		get
		{
			if (m_UpdateCommand == null)
			{
				m_UpdateCommand = new ReactiveCommand();
				ObservableExtensions.Subscribe(UpdateCommandOnLateUpdate.ObserveLastValueOnLateUpdate(), delegate
				{
					m_UpdateCommand.Execute();
				});
			}
			return m_UpdateCommand;
		}
	}

	private ReactiveCommand UpdateCommandOnLateUpdate { get; } = new ReactiveCommand();


	public bool ShowClothTemp
	{
		set
		{
			if (m_ShowClothTemp != value)
			{
				m_ShowClothTemp = value;
				Updated();
			}
		}
	}

	public bool ShowCloth
	{
		get
		{
			if (m_ShowCloth)
			{
				return m_ShowClothTemp;
			}
			return false;
		}
		set
		{
			if (m_ShowCloth != value)
			{
				m_ShowCloth = value;
				Updated();
			}
		}
	}

	public bool ShowHelmTemp
	{
		set
		{
			if (m_ShowHelmTemp != value)
			{
				m_ShowHelmTemp = value;
				Updated();
			}
		}
	}

	public bool ShowHelm
	{
		get
		{
			if (m_ShowHelm)
			{
				return m_ShowHelmTemp;
			}
			return false;
		}
		set
		{
			if (m_ShowHelm != value)
			{
				m_ShowHelm = value;
				Updated();
			}
		}
	}

	public bool ShowHelmAboveAll
	{
		get
		{
			return m_ShowHelmAboveAll;
		}
		set
		{
			if (m_ShowHelmAboveAll != value)
			{
				m_ShowHelmAboveAll = value;
				Updated();
			}
		}
	}

	public bool ShowBackpack
	{
		get
		{
			return m_ShowBackpack;
		}
		set
		{
			if (m_ShowBackpack != value)
			{
				m_ShowBackpack = value;
				Updated();
			}
		}
	}

	public void Refresh()
	{
		Updated();
	}

	public List<EquipmentEntityLink> CollectEntities()
	{
		List<EquipmentEntityLink> list = new List<EquipmentEntityLink>();
		if (RacePreset != null)
		{
			list.AddRange(RacePreset.Skin.GetLinks(Gender, Race?.RaceId ?? Kingmaker.Blueprints.Race.Human));
		}
		if (Head.Load() != null)
		{
			list.Add(Head.GetLink());
		}
		if (Eyebrows.Load() != null)
		{
			list.Add(Eyebrows.GetLink());
		}
		if (Hair.Load() != null)
		{
			list.Add(Hair.GetLink());
		}
		if (Beard.Load() != null)
		{
			list.Add(Beard.GetLink());
		}
		if (Horn.Load() != null)
		{
			list.Add(Horn.GetLink());
		}
		if (NavigatorMutation.Load() != null)
		{
			list.Add(NavigatorMutation.GetLink());
		}
		if (Scar.Load() != null)
		{
			list.Add(Scar.GetLink());
		}
		foreach (EquipmentEntityLink item in Clothes.EmptyIfNull())
		{
			list.Add(item);
		}
		CollectDollPrints(list, Warpaints);
		CollectDollPrints(list, Tattoos);
		CollectDollPrints(list, Ports);
		list.AddRange(m_MechanicsEntities);
		list.RemoveAll((EquipmentEntityLink ee) => ee == null);
		return list;
	}

	private void CollectDollPrints(List<EquipmentEntityLink> result, IEnumerable<DollPrint> dollPrints)
	{
		foreach (DollPrint item in dollPrints.Where((DollPrint w) => w.PaintEE.Load() != null))
		{
			result.Add(item.PaintEE.GetLink());
		}
	}

	public void ApplyRamps(Character character)
	{
		ApplyRamp(character, HairRampIndex, Hair);
		ApplyRamp(character, BeardColorRampIndex, Beard);
		ApplyRamp(character, EyebrowsColorRampIndex, Eyebrows);
		ApplyRamp(character, EyesColorRampIndex, GetHeadEntities(), secondary: true);
		if (SkinRampIndex >= 0)
		{
			ApplyRamp(character, SkinRampIndex, GetSkinEntities());
			ApplyRamp(character, SkinRampIndex, Horn, secondary: true);
			ApplyRamp(character, SkinRampIndex, NavigatorMutation, secondary: true);
			ApplyRamp(character, SkinRampIndex, Scar);
		}
		ApplyRamp(character, HornsRampIndex, Horn);
		foreach (DollPrint warpaint in Warpaints)
		{
			warpaint.SetPrimaryRampIndex(character);
		}
		foreach (DollPrint tattoo in Tattoos)
		{
			tattoo.SetPrimaryRampIndex(character);
		}
		if (EquipmentRampIndex < 0 && EquipmentRampIndexSecondary < 0)
		{
			return;
		}
		foreach (EquipmentEntityLink clothe in Clothes)
		{
			character.SetPrimaryRampIndex(clothe.Load(), EquipmentRampIndex);
			character.SetSecondaryRampIndex(clothe.Load(), EquipmentRampIndexSecondary);
		}
	}

	private void ApplyRamp(Character character, int rampIndex, IEnumerable<EEAdapter> ees, bool secondary = false)
	{
		foreach (EEAdapter ee in ees)
		{
			ApplyRamp(character, rampIndex, ee, secondary);
		}
	}

	private void ApplyRamp(Character character, int rampIndex, EEAdapter ee, bool secondary = false)
	{
		if (rampIndex >= 0 && !(ee.Load() == null))
		{
			if (!secondary)
			{
				character.SetPrimaryRampIndex(ee.Load(), rampIndex);
			}
			else
			{
				character.SetSecondaryRampIndex(ee.Load(), rampIndex);
			}
		}
	}

	[CanBeNull]
	public Skeleton GetSkeleton()
	{
		if (RacePreset == null)
		{
			return null;
		}
		if (Gender != 0)
		{
			return RacePreset.FemaleSkeleton;
		}
		return RacePreset.MaleSkeleton;
	}

	public void Setup(BaseUnitEntity unit, PregenDollSettings settings)
	{
		Gender = unit.Gender;
		Race = unit.Progression.Race ?? ProgressionRoot.Instance.CharacterRaces.FirstOrDefault();
		Portrait = unit.Blueprint.PortraitSafe;
		Validate();
		PopulatePregenDollSettings(settings);
		SetTrackPortrait(state: false);
		UpdateMechanicsEntities(unit);
	}

	public void SetupFromUnit(BaseUnitEntity unit)
	{
		BlueprintPortrait portrait = unit.UISettings.PortraitBlueprint ?? unit.Blueprint.PortraitSafe;
		SetTrackPortrait(state: true);
		SetPortrait(portrait);
		SetTrackPortrait(state: false);
		Gender = unit.Gender;
		if (Race == null)
		{
			BlueprintRace obj = unit.Progression.Race ?? ProgressionRoot.Instance.CharacterRaces.FirstOrDefault();
			BlueprintRace blueprintRace = obj;
			Race = obj;
		}
		Validate();
		DollData doll = unit.ViewSettings.Doll;
		if (doll != null)
		{
			PopulateFromDollData(doll);
		}
		else
		{
			PregenDollSettings component = unit.Blueprint.GetComponent<PregenDollSettings>();
			if (component != null)
			{
				PopulatePregenDollSettings(component);
			}
			else
			{
				Character characterAvatar = unit.View.CharacterAvatar;
				if (characterAvatar != null)
				{
					PopulateFromCharacterView(characterAvatar);
				}
				else
				{
					PFLog.Default.Error($"Cannot populate DollData for character {unit}: it has neither DollData, PregenDollSettings, Character component!");
				}
			}
		}
		UpdateMechanicsEntities(unit);
	}

	private void PopulatePregenDollSettings(PregenDollSettings settings)
	{
		m_DefaultSettings = settings.Default;
		RacePreset = m_DefaultSettings.RacePreset;
		Head = new EEAdapter(m_DefaultSettings.Head);
		CreateWarpaints(m_DefaultSettings);
		CreateTattoos(m_DefaultSettings);
		CreatePorts(m_DefaultSettings);
		Scar = new EEAdapter(m_DefaultSettings.Scar);
		Hair = new EEAdapter(m_DefaultSettings.Hair);
		Eyebrows = new EEAdapter(m_DefaultSettings.Eyebrows);
		Beard = new EEAdapter(m_DefaultSettings.Beard);
		Horn = new EEAdapter(m_DefaultSettings.Horn);
		NavigatorMutation = new EEAdapter(m_DefaultSettings.NavigatorMutation);
		HairRampIndex = m_DefaultSettings.HairRampIndex;
		SkinRampIndex = m_DefaultSettings.SkinRampIndex;
		EyesColorRampIndex = m_DefaultSettings.EyesColorRampIndex;
		HornsRampIndex = m_DefaultSettings.HornsRampIndex;
		EyebrowsColorRampIndex = m_DefaultSettings.EyebrowsColorRampIndex;
		BeardColorRampIndex = m_DefaultSettings.BeardColorRampIndex;
		EquipmentRampIndex = m_DefaultSettings.EquipmentRampIndex;
		EquipmentRampIndexSecondary = m_DefaultSettings.EquipmentRampIndexSecondary;
		Scars = GetScarsList(Race, Gender);
		Updated();
	}

	private void PopulateFromDollData(DollData dollData)
	{
		RacePreset = dollData.RacePreset;
		PopulateFromEEIdList(dollData.EquipmentEntityIds, GetRampIndex, dollData.ClothesPrimaryIndex, dollData.ClothesSecondaryIndex);
		int GetRampIndex(EEAdapter ee, bool secondary = false)
		{
			EquipmentEntityLink link = ee.GetLink();
			if (link == null)
			{
				return -1;
			}
			return (secondary ? dollData.EntitySecondaryRampIdices : dollData.EntityRampIdices).GetValueOrDefault(link.AssetId, -1);
		}
	}

	private void PopulateFromCharacterView(Character character)
	{
		PopulateFromEEIdList(character.SavedEquipmentEntities.Select((EquipmentEntityLink ee) => ee.AssetId), GetRampIndex);
		int GetRampIndex(EEAdapter ee, bool secondary = false)
		{
			EquipmentEntityLink link = ee.GetLink();
			if (link == null)
			{
				return -1;
			}
			Character.SavedSelectedRampIndices savedSelectedRampIndices = character.m_SavedRampIndices.Find((Character.SavedSelectedRampIndices ri) => ri.EquipmentEntityLink.AssetId == link.AssetId);
			if (savedSelectedRampIndices == null)
			{
				return -1;
			}
			if (!secondary)
			{
				return savedSelectedRampIndices.PrimaryIndex;
			}
			return savedSelectedRampIndices.SecondaryIndex;
		}
	}

	private void PopulateFromEEIdList(IEnumerable<string> eeIds, GetRampIndexDelegate getRampIndex, int clothesIndexPrimary = -1, int clotherIndexSecondary = -1)
	{
		if (Race == null)
		{
			return;
		}
		CustomizationOptions customizationOptions = ((Gender == Gender.Male) ? Race.MaleOptions : Race.FemaleOptions);
		HashSet<string> eeIdSet;
		using (CollectionPool<HashSet<string>, string>.Get(out eeIdSet))
		{
			eeIdSet.AddRange(eeIds);
			Head = GetEE(customizationOptions.Heads);
			CreateWarpaints(null);
			CreateTattoos(null);
			CreatePorts(null);
			LoadDollPrintList(Warpaints);
			LoadDollPrintList(Tattoos);
			LoadDollPrintList(Ports);
			Scar = GetEE(customizationOptions.Scars);
			Hair = GetEE(customizationOptions.Hair);
			Eyebrows = GetEE(customizationOptions.Eyebrows);
			Beard = GetEE(customizationOptions.Beards);
			Horn = GetEE(customizationOptions.Horns);
			NavigatorMutation = GetEE(customizationOptions.NavigatorMutations);
			HairRampIndex = getRampIndex(Hair);
			SkinRampIndex = getRampIndex(Head);
			EyesColorRampIndex = getRampIndex(Head, secondary: true);
			HornsRampIndex = getRampIndex(Horn);
			EyebrowsColorRampIndex = getRampIndex(Eyebrows);
			BeardColorRampIndex = getRampIndex(Beard);
			EquipmentRampIndex = clothesIndexPrimary;
			EquipmentRampIndexSecondary = clotherIndexSecondary;
			Scars = GetScarsList(Race, Gender);
			Updated();
		}
		EEAdapter GetEE(IList<EquipmentEntityLink> availableEELinks)
		{
			return new EEAdapter(availableEELinks.FirstOrDefault((EquipmentEntityLink ee) => eeIdSet.Contains(ee.AssetId)));
		}
		void LoadDollPrintList(List<DollPrint> list)
		{
			LoadDollPrintListInternal(list, eeIdSet, getRampIndex);
		}
	}

	private static void LoadDollPrintListInternal(List<DollPrint> list, HashSet<string> eeIdSet, GetRampIndexDelegate rampIndexGetter)
	{
		List<EquipmentEntityLink> value;
		using (CollectionPool<List<EquipmentEntityLink>, EquipmentEntityLink>.Get(out value))
		{
			value.Clear();
			value.AddRange(list[0].Paints.Where((EquipmentEntityLink ee) => eeIdSet.Contains(ee.AssetId)));
			for (int i = 0; i < value.Count && i < list.Count; i++)
			{
				DollPrint dollPrint = list[i];
				dollPrint.PaintEE = new EEAdapter(value[i]);
				dollPrint.PaintRampIndex = rampIndexGetter(dollPrint.PaintEE);
			}
		}
	}

	private static List<EquipmentEntityLink> GetScarsList(BlueprintRace race, Gender gender)
	{
		if (race == null)
		{
			return null;
		}
		return ((gender == Gender.Male) ? race.MaleOptions : race.FemaleOptions).Scars.ToList();
	}

	public void SetTrackPortrait(bool state)
	{
		m_TrackPortrait = state;
	}

	public void SetPortrait([NotNull] BlueprintPortrait portrait)
	{
		Portrait = portrait;
		if (!m_TrackPortrait)
		{
			UpdateCommandOnLateUpdate.Execute();
			return;
		}
		PortraitDollSettings component = portrait.GetComponent<PortraitDollSettings>();
		if (component == null)
		{
			UpdateCommandOnLateUpdate.Execute();
			return;
		}
		if (Race != component.Race || Gender != component.Gender)
		{
			ClearRampIndexes();
		}
		Gender = component.Gender;
		Race = component.Race;
		PregenDollSettings component2 = portrait.GetComponent<PregenDollSettings>();
		if (component2 != null)
		{
			PopulatePregenDollSettings(component2);
			return;
		}
		CreateWarpaints(m_DefaultSettings);
		CreateTattoos(m_DefaultSettings);
		CreatePorts(m_DefaultSettings);
		Updated();
	}

	public void SetGender(Gender gender)
	{
		if (Gender != gender)
		{
			Head = default(EEAdapter);
			Hair = default(EEAdapter);
			Beard = default(EEAdapter);
			Eyebrows = default(EEAdapter);
			Horn = default(EEAdapter);
			NavigatorMutation = default(EEAdapter);
			if (m_TrackPortrait && Gender != gender)
			{
				SetTrackPortrait(state: false);
			}
			Gender = gender;
			ClearRampIndexes();
			Updated();
		}
	}

	public void SetRace([NotNull] BlueprintRace race)
	{
		if (Race != race)
		{
			Head = default(EEAdapter);
			Hair = default(EEAdapter);
			Beard = default(EEAdapter);
			Eyebrows = default(EEAdapter);
			if (m_TrackPortrait && Race != race)
			{
				SetTrackPortrait(state: false);
			}
			Race = race;
			CreateWarpaints(m_DefaultSettings);
			CreateTattoos(m_DefaultSettings);
			CreatePorts(m_DefaultSettings);
			ClearRampIndexes();
			Updated();
		}
	}

	public void SetRacePreset([NotNull] BlueprintRaceVisualPreset racePreset)
	{
		if (RacePreset != racePreset)
		{
			SetTrackPortrait(state: false);
			RacePreset = racePreset;
			Updated();
		}
	}

	public void SetHead([NotNull] EquipmentEntityLink head)
	{
		if (!(Head.GetLink() == head))
		{
			SetTrackPortrait(state: false);
			Head = new EEAdapter(head);
			Updated();
		}
	}

	public void SetHair([NotNull] EquipmentEntityLink hair)
	{
		if (!(Hair.GetLink() == hair))
		{
			SetTrackPortrait(state: false);
			Hair = new EEAdapter(hair);
			Updated();
		}
	}

	public void SetBeard([NotNull] EquipmentEntityLink beard)
	{
		if (!(Beard.GetLink() == beard))
		{
			SetTrackPortrait(state: false);
			Beard = new EEAdapter(beard);
			Updated();
		}
	}

	public void SetEyebrows([NotNull] EquipmentEntityLink eyebrows)
	{
		if (!(Eyebrows.GetLink() == eyebrows))
		{
			SetTrackPortrait(state: false);
			Eyebrows = new EEAdapter(eyebrows);
			Updated();
		}
	}

	public void SetScar([NotNull] EquipmentEntityLink scar)
	{
		if (!(Scar.GetLink() == scar))
		{
			SetTrackPortrait(state: false);
			Scar = new EEAdapter(scar);
			Updated();
		}
	}

	public void SetWarpaint(EquipmentEntityLink warpaint, int index)
	{
		if (index < Warpaints.Count && !(Warpaints[index].PaintEE.GetLink() == warpaint))
		{
			SetTrackPortrait(state: false);
			Warpaints[index].PaintEE = new EEAdapter(warpaint);
			Updated();
		}
	}

	public void SetTattoo(EquipmentEntityLink tattoo, int index)
	{
		if (index < Tattoos.Count && !(Tattoos[index].PaintEE.GetLink() == tattoo))
		{
			SetTrackPortrait(state: false);
			Tattoos[index].PaintEE = new EEAdapter(tattoo);
			Updated();
		}
	}

	public void SetPort([NotNull] EquipmentEntityLink port, int index)
	{
		if (index < Ports.Count && !(Ports[index].PaintEE.GetLink() == port))
		{
			SetTrackPortrait(state: false);
			Ports[index].PaintEE = new EEAdapter(port);
			Updated();
		}
	}

	public void SetHorn([NotNull] EquipmentEntityLink horn)
	{
		if (!(Horn.GetLink() == horn))
		{
			SetTrackPortrait(state: false);
			Horn = new EEAdapter(horn);
			Updated();
		}
	}

	public void SetNavigatorMutation([NotNull] EquipmentEntityLink mutation)
	{
		if (!(NavigatorMutation.GetLink() == mutation))
		{
			SetTrackPortrait(state: false);
			NavigatorMutation = new EEAdapter(mutation);
			Updated();
		}
	}

	public List<Texture2D> GetHairRamps()
	{
		List<Texture2D> ramps = GetRamps(Hair);
		if (ramps.Count <= 0)
		{
			return s_EmptyRamps;
		}
		return ramps;
	}

	public List<Texture2D> GetBeardRamps()
	{
		List<Texture2D> ramps = GetRamps(Beard);
		if (ramps.Count <= 0)
		{
			return s_EmptyRamps;
		}
		return ramps;
	}

	public List<Texture2D> GetEyebrowsRamps()
	{
		List<Texture2D> ramps = GetRamps(Eyebrows);
		if (ramps.Count <= 0)
		{
			return s_EmptyRamps;
		}
		return ramps;
	}

	public List<Texture2D> GetHornsRamps()
	{
		List<Texture2D> ramps = GetRamps(Horn);
		if (ramps.Count <= 0)
		{
			return s_EmptyRamps;
		}
		return ramps;
	}

	public List<Texture2D> GetSkinRamps()
	{
		return GetRamps(Head);
	}

	public List<Texture2D> GetWarpaintRamps()
	{
		if (Warpaints.Count == 0)
		{
			return s_EmptyRamps;
		}
		List<Texture2D> ramps = GetRamps(Warpaints.First().PaintEE);
		if (ramps.Count <= 0)
		{
			return s_EmptyRamps;
		}
		return ramps;
	}

	public List<Texture2D> GetTattooRamps(int index)
	{
		if (Tattoos.Count == 0 || index >= Tattoos.Count)
		{
			return s_EmptyRamps;
		}
		List<Texture2D> ramps = GetRamps(Tattoos[index].PaintEE);
		if (ramps.Count <= 0)
		{
			return s_EmptyRamps;
		}
		return ramps;
	}

	public List<Texture2D> GetOutfitRampsPrimary()
	{
		return CharGenUtility.GetClothesColorsProfile(Clothes)?.Ramps ?? s_EmptyRamps;
	}

	public List<Texture2D> GetOutfitRampsSecondary()
	{
		return CharGenUtility.GetClothesColorsProfile(Clothes, secondary: true)?.Ramps ?? s_EmptyRamps;
	}

	private static List<Texture2D> GetRamps(EEAdapter ee, bool primary = true)
	{
		if (ee.Load() == null)
		{
			return s_EmptyRamps;
		}
		if (!primary)
		{
			return ee.Load().SecondaryRamps;
		}
		return ee.Load().PrimaryRamps;
	}

	public void SetHairColor(int rampIndex)
	{
		if (HairRampIndex != rampIndex)
		{
			SetTrackPortrait(state: false);
			HairRampIndex = rampIndex;
			Updated();
		}
	}

	public void SetEyebrowsColor(int rampIndex)
	{
		if (EyebrowsColorRampIndex != rampIndex)
		{
			SetTrackPortrait(state: false);
			EyebrowsColorRampIndex = rampIndex;
			Updated();
		}
	}

	public void SetBeardColor(int rampIndex)
	{
		if (BeardColorRampIndex != rampIndex)
		{
			SetTrackPortrait(state: false);
			BeardColorRampIndex = rampIndex;
			Updated();
		}
	}

	public void SetSkinColor(int rampIndex)
	{
		if (SkinRampIndex != rampIndex)
		{
			SetTrackPortrait(state: false);
			SkinRampIndex = rampIndex;
			Updated();
		}
	}

	public void SetHornsColor(int rampIndex)
	{
		if (HornsRampIndex != rampIndex)
		{
			SetTrackPortrait(state: false);
			HornsRampIndex = rampIndex;
			Updated();
		}
	}

	public void SetWarpaintColor(int rampIndex, int index)
	{
		if (index < Warpaints.Count && Warpaints[index].PaintRampIndex != rampIndex)
		{
			SetTrackPortrait(state: false);
			Warpaints[index].PaintRampIndex = rampIndex;
			Updated();
		}
	}

	public void SetTattooColor(int rampIndex, int index)
	{
		if (index < Tattoos.Count && Tattoos[index].PaintRampIndex != rampIndex)
		{
			SetTrackPortrait(state: false);
			Tattoos[index].PaintRampIndex = rampIndex;
			Updated();
		}
	}

	public void SetEquipColors(int primaryIndex, int secondaryIndex)
	{
		bool num = EquipmentRampIndex != primaryIndex || EquipmentRampIndexSecondary != secondaryIndex;
		EquipmentRampIndex = primaryIndex;
		EquipmentRampIndexSecondary = secondaryIndex;
		if (num)
		{
			Updated();
		}
	}

	public void SetLeftHanded(bool leftHanded)
	{
		LeftHanded = leftHanded;
		Updated();
	}

	public void Validate()
	{
		Clothes = GetClothes().ToList();
		if (Race == null)
		{
			RacePreset = null;
			Head = default(EEAdapter);
			Hair = default(EEAdapter);
			Eyebrows = default(EEAdapter);
			Beard = default(EEAdapter);
			Ports = new List<DollPrint>(2);
			Scars = new List<EquipmentEntityLink>();
			Tattoos = new List<DollPrint>(5);
			ClearRampIndexes();
		}
		else
		{
			if (!Race.Presets.Contains(RacePreset))
			{
				RacePreset = Race.Presets.FirstOrDefault();
			}
			CustomizationOptions customizationOptions = ((Gender == Gender.Male) ? Race.MaleOptions : Race.FemaleOptions);
			if (!LinksHasAdapter(customizationOptions.Heads, Head))
			{
				Head = new EEAdapter(customizationOptions.Heads.FirstOrDefault());
			}
			if (!LinksHasAdapter(customizationOptions.Eyebrows, Eyebrows))
			{
				Eyebrows = new EEAdapter(customizationOptions.Eyebrows.FirstOrDefault());
			}
			if (!LinksHasAdapter(customizationOptions.Hair, Hair))
			{
				Hair = new EEAdapter(customizationOptions.Hair.FirstOrDefault());
			}
			if (!LinksHasAdapter(customizationOptions.Beards, Beard))
			{
				Beard = new EEAdapter(customizationOptions.Beards.FirstOrDefault());
			}
			if (!LinksHasAdapter(customizationOptions.Horns, Horn))
			{
				Horn = new EEAdapter(customizationOptions.Horns.FirstOrDefault());
			}
			if (!LinksHasAdapter(customizationOptions.NavigatorMutations, NavigatorMutation))
			{
				NavigatorMutation = new EEAdapter(customizationOptions.NavigatorMutations.FirstOrDefault());
			}
			foreach (DollPrint port in Ports)
			{
				if (!LinksHasAdapter(customizationOptions.Ports, port.PaintEE))
				{
					port.PaintEE = new EEAdapter(customizationOptions.Ports.FirstOrDefault());
				}
			}
			foreach (DollPrint tattoo in Tattoos)
			{
				if (!LinksHasAdapter(customizationOptions.Tattoos, tattoo.PaintEE))
				{
					tattoo.PaintEE = new EEAdapter(tattoo.Paints.FirstOrDefault());
				}
			}
			if (ShowCloth && (GetOutfitRampsPrimary()?.Count ?? 0) <= EquipmentRampIndex)
			{
				EquipmentRampIndex = -1;
			}
			if (ShowCloth && (GetOutfitRampsSecondary()?.Count ?? 0) <= EquipmentRampIndexSecondary)
			{
				EquipmentRampIndexSecondary = -1;
			}
			Scars = GetScarsList(Race, Gender);
		}
		if (!LinksHasAdapter(Scars.ToArray(), Scar))
		{
			Scar = new EEAdapter(Scars.FirstOrDefault());
		}
		EquipmentEntityLink[] warpaintsForCustomization = BlueprintRoot.Instance.CharGenRoot.WarpaintsForCustomization;
		foreach (DollPrint warpaint in Warpaints)
		{
			warpaint.Paints = warpaintsForCustomization.ToList();
			if (!LinksHasAdapter(warpaintsForCustomization, warpaint.PaintEE))
			{
				warpaint.PaintEE = new EEAdapter(warpaint.Paints.FirstOrDefault());
			}
		}
	}

	private bool LinksHasAdapter(EquipmentEntityLink[] links, EEAdapter adapter)
	{
		if (adapter.GetLink() != null)
		{
			return links.FindIndex((EquipmentEntityLink l) => l.AssetId == adapter.AssetId) >= 0;
		}
		return false;
	}

	private void Updated()
	{
		Validate();
		UpdateCommandOnLateUpdate.Execute();
		EventBus.RaiseEvent(delegate(ILevelUpDollHandler h)
		{
			h.HandleDollStateUpdated(this);
		});
	}

	private IEnumerable<EquipmentEntityLink> GetClothes()
	{
		Race race = Race?.RaceId ?? Kingmaker.Blueprints.Race.Human;
		if (ShowCloth && m_EquipmentEntities.Any((KingmakerEquipmentEntity e) => e != null))
		{
			return CharGenUtility.GetClothes(m_EquipmentEntities, Gender, race);
		}
		if (Gender != 0)
		{
			return Root.FemaleClothes;
		}
		return Root.MaleClothes;
	}

	private List<EEAdapter> GetSkinEntities()
	{
		List<EEAdapter> list = new List<EEAdapter>();
		EquipmentEntityLink[] array = RacePreset?.Skin?.GetLinks(Gender, (RacePreset?.RaceId).Value);
		if (array != null)
		{
			list.AddRange(array.Select((EquipmentEntityLink ee) => new EEAdapter(ee)));
		}
		if (Head.Load() != null)
		{
			list.Add(Head);
		}
		return list;
	}

	private List<EEAdapter> GetHeadEntities()
	{
		List<EEAdapter> list = new List<EEAdapter>();
		if (Head.Load() != null)
		{
			list.Add(Head);
		}
		return list;
	}

	private void CreateTattoos(PregenDollSettings.Entry s)
	{
		Tattoos.Clear();
		if (Race != null)
		{
			CustomizationOptions customizationOptions = ((Gender == Gender.Male) ? Race.MaleOptions : Race.FemaleOptions);
			Tattoos = new List<DollPrint>
			{
				new DollPrint
				{
					PaintEE = new EEAdapter(s?.Tattoo),
					PaintRampIndex = (s?.TattooRampIndex ?? (-1)),
					Paints = customizationOptions.Tattoos.ToList()
				},
				new DollPrint
				{
					PaintEE = new EEAdapter(s?.Tattoo2),
					PaintRampIndex = (s?.TattooRampIndex ?? (-1)),
					Paints = customizationOptions.Tattoos.ToList()
				},
				new DollPrint
				{
					PaintEE = new EEAdapter(s?.Tattoo3),
					PaintRampIndex = (s?.TattooRampIndex ?? (-1)),
					Paints = customizationOptions.Tattoos.ToList()
				},
				new DollPrint
				{
					PaintEE = new EEAdapter(s?.Tattoo4),
					PaintRampIndex = (s?.TattooRampIndex ?? (-1)),
					Paints = customizationOptions.Tattoos.ToList()
				},
				new DollPrint
				{
					PaintEE = new EEAdapter(s?.Tattoo5),
					PaintRampIndex = (s?.TattooRampIndex ?? (-1)),
					Paints = customizationOptions.Tattoos.ToList()
				}
			};
		}
	}

	private void CreateWarpaints(PregenDollSettings.Entry s)
	{
		Warpaints.Clear();
		for (int i = 0; i < 5; i++)
		{
			Warpaints.Add(new DollPrint
			{
				PaintEE = new EEAdapter(s?.Warpaint),
				PaintRampIndex = (s?.WarpaintRampIndex ?? (-1)),
				Paints = BlueprintRoot.Instance.CharGenRoot.WarpaintsForCustomization.ToList()
			});
		}
	}

	private void CreatePorts(PregenDollSettings.Entry s)
	{
		Ports.Clear();
		if (Race != null)
		{
			CustomizationOptions customizationOptions = ((Gender == Gender.Male) ? Race.MaleOptions : Race.FemaleOptions);
			Ports = new List<DollPrint>
			{
				new DollPrint
				{
					PaintEE = new EEAdapter(s?.Port),
					PaintRampIndex = -1,
					Paints = customizationOptions.Ports.ToList()
				},
				new DollPrint
				{
					PaintEE = new EEAdapter(s?.Port2),
					PaintRampIndex = -1,
					Paints = customizationOptions.Ports.ToList()
				}
			};
		}
	}

	public void UpdateMechanicsEntities(BaseUnitEntity unit)
	{
		bool flag = false;
		IEnumerable<KingmakerEquipmentEntity> enumerable = from i in unit.Progression.Features.Enumerable.Select((Kingmaker.UnitLogic.Feature f) => f.Blueprint.GetComponent<AddKingmakerEquipmentEntity>()).NotNull()
			select i.EquipmentEntity;
		if (!m_EquipmentEntities.SequenceEqual(enumerable))
		{
			m_EquipmentEntities = enumerable.ToList();
			flag = true;
		}
		IEnumerable<EquipmentEntityLink> enumerable2 = CollectMechanicEntities(unit);
		if (!m_MechanicsEntities.SequenceEqual(enumerable2))
		{
			m_MechanicsEntities = enumerable2.ToList();
			flag = true;
		}
		if (flag)
		{
			Updated();
		}
	}

	private IEnumerable<EquipmentEntityLink> CollectMechanicEntities(BaseUnitEntity unit)
	{
		foreach (Kingmaker.UnitLogic.Feature feature in unit.Progression.Features)
		{
			IEnumerable<EquipmentEntityLink> source = from c in feature.Blueprint.GetComponents<AddEquipmentEntity>()
				select c.EquipmentEntity;
			foreach (EquipmentEntityLink item in source.Where((EquipmentEntityLink eeLink) => eeLink.Load() != null))
			{
				yield return item;
			}
		}
	}

	[NotNull]
	public DollData CreateData()
	{
		DollData data = new DollData
		{
			Gender = Gender,
			RacePreset = RacePreset
		};
		if (Head.Load() != null)
		{
			data.EquipmentEntityIds.Add(Head.AssetId);
		}
		if (Eyebrows.Load() != null)
		{
			data.EquipmentEntityIds.Add(Eyebrows.AssetId);
		}
		if (Hair.Load() != null)
		{
			data.EquipmentEntityIds.Add(Hair.AssetId);
		}
		if (Beard.Load() != null)
		{
			data.EquipmentEntityIds.Add(Beard.AssetId);
		}
		if (Horn.Load() != null)
		{
			data.EquipmentEntityIds.Add(Horn.AssetId);
		}
		if (NavigatorMutation.Load() != null)
		{
			data.EquipmentEntityIds.Add(NavigatorMutation.AssetId);
		}
		foreach (DollPrint item in Warpaints.Where((DollPrint warpaint) => warpaint.PaintEE.Load() != null))
		{
			data.EquipmentEntityIds.Add(item.PaintEE.AssetId);
			if (item.PaintRampIndex >= 0)
			{
				data.EntityRampIdices[item.PaintEE.AssetId] = item.PaintRampIndex;
			}
		}
		foreach (DollPrint item2 in Tattoos.Where((DollPrint tattoo) => tattoo.PaintEE.Load() != null))
		{
			data.EquipmentEntityIds.Add(item2.PaintEE.AssetId);
			if (item2.PaintRampIndex >= 0)
			{
				data.EntityRampIdices[item2.PaintEE.AssetId] = item2.PaintRampIndex;
			}
		}
		foreach (DollPrint item3 in Ports.Where((DollPrint port) => port.PaintEE.Load() != null))
		{
			data.EquipmentEntityIds.Add(item3.PaintEE.AssetId);
		}
		if (Scar.Load() != null)
		{
			data.EquipmentEntityIds.Add(Scar.AssetId);
		}
		if (Race != null)
		{
			EquipmentEntityLink tail = Race.GetTail(Gender, SkinRampIndex);
			if (tail != null)
			{
				data.EquipmentEntityIds.Add(tail.AssetId);
			}
		}
		SetRampIdices(SkinRampIndex, GetSkinEntities(), ref data);
		SetRampIndex(HairRampIndex, Hair, ref data);
		SetRampIdices(EyesColorRampIndex, GetHeadEntities(), ref data, secondary: true);
		SetRampIndex(HornsRampIndex, Horn, ref data);
		SetRampIndex(BeardColorRampIndex, Beard, ref data);
		SetRampIndex(EyebrowsColorRampIndex, Eyebrows, ref data);
		data.ClothesPrimaryIndex = EquipmentRampIndex;
		data.ClothesSecondaryIndex = EquipmentRampIndexSecondary;
		return data;
	}

	private void SetRampIdices(int rampIndex, List<EEAdapter> ees, ref DollData data, bool secondary = false)
	{
		foreach (EEAdapter ee in ees)
		{
			SetRampIndex(rampIndex, ee, ref data, secondary);
		}
	}

	private static void SetRampIndex(int rampIndex, EEAdapter ee, ref DollData data, bool secondary = false)
	{
		if (rampIndex >= 0 && !(ee.Load() == null))
		{
			if (!secondary)
			{
				data.EntityRampIdices[ee.AssetId] = rampIndex;
			}
			else
			{
				data.EntitySecondaryRampIdices[ee.AssetId] = rampIndex;
			}
		}
	}

	private void ClearRampIndexes()
	{
		HairRampIndex = -1;
		SkinRampIndex = -1;
		EyesColorRampIndex = -1;
		EyebrowsColorRampIndex = -1;
		BeardColorRampIndex = -1;
		HornsRampIndex = -1;
		EquipmentRampIndex = -1;
		EquipmentRampIndexSecondary = -1;
	}

	public DollState Copy()
	{
		return new DollState
		{
			Gender = Gender,
			Race = Race,
			RacePreset = RacePreset,
			Head = Head,
			Scar = Scar,
			Eyebrows = Eyebrows,
			Hair = Hair,
			Beard = Beard,
			Horn = Horn,
			NavigatorMutation = NavigatorMutation,
			Portrait = Portrait,
			Clothes = Clothes.ToList(),
			Scars = Scars.ToList(),
			Tattoos = Tattoos,
			Warpaints = Warpaints,
			Ports = Ports.ToList(),
			HairRampIndex = HairRampIndex,
			SkinRampIndex = SkinRampIndex,
			EyesColorRampIndex = EyesColorRampIndex,
			EyebrowsColorRampIndex = EyebrowsColorRampIndex,
			BeardColorRampIndex = BeardColorRampIndex,
			HornsRampIndex = HornsRampIndex,
			EquipmentRampIndex = EquipmentRampIndex,
			EquipmentRampIndexSecondary = EquipmentRampIndexSecondary,
			LeftHanded = LeftHanded,
			m_TrackPortrait = m_TrackPortrait,
			m_MechanicsEntities = m_MechanicsEntities.ToList()
		};
	}
}
