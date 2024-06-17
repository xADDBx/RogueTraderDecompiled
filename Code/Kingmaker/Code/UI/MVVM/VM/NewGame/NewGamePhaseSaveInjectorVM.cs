using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.UI.Models.LevelUp;
using Kingmaker.UI.MVVM.VM.MainMenu;
using Kingmaker.UnitLogic.Levelup.CharGen;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame;

public class NewGamePhaseSaveInjectorVM : NewGamePhaseBaseVm
{
	private readonly ReactiveProperty<string> m_CharacterName = new ReactiveProperty<string>();

	private ReactiveProperty<Sprite> m_Portrait = new ReactiveProperty<Sprite>();

	public readonly SaveSlotCollectionVM SaveSlotCollectionVm;

	private readonly List<SaveSlotVM> m_SaveSlotVMs = new List<SaveSlotVM>();

	public readonly ReactiveCommand CollectionWasUpdated = new ReactiveCommand();

	private readonly ReactiveProperty<SaveLoadMode> m_Mode = new ReactiveProperty<SaveLoadMode>(SaveLoadMode.Load);

	public IReadOnlyReactiveProperty<string> CharacterName => m_CharacterName;

	public IReadOnlyReactiveProperty<Sprite> Portrait => m_Portrait;

	private bool NeedToShow
	{
		get
		{
			if (Game.NewGamePreset == BlueprintRoot.Instance.NewGamePreset)
			{
				return false;
			}
			bool flag = false;
			foreach (NewGameRoot.StoryEntity story in BlueprintRoot.Instance.NewGameSettings.StoryList)
			{
				if (!story.ComingSoon && story.DlcReward != null && Game.NewGamePreset == story.DlcReward.Campaign.StartGamePreset)
				{
					flag = story.DlcReward.Campaign.ImportFromMainCampaign.CanImport;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
			return CheckSaves();
		}
	}

	private bool CheckSaves()
	{
		Game.Instance.SaveManager.UpdateSaveListIfNeeded();
		List<SaveInfo> list = Game.Instance.SaveManager.Where((SaveInfo si) => si.DLCCampaign == null).ToList();
		if (list.Any())
		{
			UpdateSavesCollection(list);
			return true;
		}
		return false;
	}

	public NewGamePhaseSaveInjectorVM(Action backStep, Action nextStep)
		: base(backStep, nextStep)
	{
		AddDisposable(MainMenuChargenUnits.Instance.CurrentPregenUnit.Subscribe(SetUnitProperties));
		AddDisposable(SaveSlotCollectionVm = new SaveSlotCollectionVM(new ReactiveProperty<SaveLoadMode>(SaveLoadMode.Load)));
	}

	private void SetUnitProperties([CanBeNull] ChargenUnit chargenUnit)
	{
		if (chargenUnit == null)
		{
			m_CharacterName.Value = BlueprintRoot.Instance.LocalizedTexts.UserInterfacesText.NewGameWin.CreateNewCharacter;
			m_Portrait.Value = MainMenuChargenUnits.Instance.CustomCharacterPortrait.FullLengthPortrait;
		}
		else
		{
			m_CharacterName.Value = chargenUnit.Blueprint.GetComponent<PregenUnitComponent>().PregenName;
			m_Portrait.Value = chargenUnit.Blueprint.PortraitSafe.FullLengthPortrait;
		}
	}

	private void UpdateSavesCollection(List<SaveInfo> collection)
	{
		collection.Sort((SaveInfo s1, SaveInfo s2) => -s1.SystemSaveTime.CompareTo(s2.SystemSaveTime));
		foreach (SaveInfo saveInfo in collection)
		{
			if (!m_SaveSlotVMs.Any((SaveSlotVM vm) => vm.ReferenceSaveEquals(saveInfo)))
			{
				SaveSlotVM saveSlotVM = new SaveSlotVM(saveInfo, m_Mode);
				AddDisposable(saveSlotVM);
				SaveSlotCollectionVm.HandleNewSave(saveSlotVM);
				m_SaveSlotVMs.Add(saveSlotVM);
			}
		}
		List<SaveSlotVM> list = new List<SaveSlotVM>();
		foreach (SaveSlotVM item in m_SaveSlotVMs.Where((SaveSlotVM saveSlotVm) => !collection.Any(saveSlotVm.ReferenceSaveEquals)))
		{
			item.Dispose();
			list.Add(item);
		}
		foreach (SaveSlotVM item2 in list)
		{
			SaveSlotCollectionVm.HandleDeleteSave(item2);
			m_SaveSlotVMs.Remove(item2);
		}
		CollectionWasUpdated.Execute();
	}

	public void AcceptSave()
	{
		Game.ImportSave = m_SaveSlotVMs.FirstOrDefault((SaveSlotVM slot) => slot.IsSelected.Value)?.Reference;
	}

	public void DeclineSave()
	{
		Game.ImportSave = null;
	}

	public override bool SetEnabled(bool value, bool? direction = null)
	{
		if (value && !NeedToShow && direction.HasValue)
		{
			if (direction.Value)
			{
				OnNext();
			}
			else
			{
				OnBack();
			}
			return false;
		}
		return base.SetEnabled(value, direction);
	}

	protected override void DisposeImplementation()
	{
	}
}
