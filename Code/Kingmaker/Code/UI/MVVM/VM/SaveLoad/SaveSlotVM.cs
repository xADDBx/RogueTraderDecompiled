using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Localization;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class SaveSlotVM : SelectionGroupEntityVM
{
	public readonly IReadOnlyReactiveProperty<SaveLoadMode> Mode;

	public readonly BoolReactiveProperty IsEmpty = new BoolReactiveProperty();

	public readonly StringReactiveProperty CharacterName = new StringReactiveProperty();

	public readonly StringReactiveProperty GameName = new StringReactiveProperty();

	public readonly StringReactiveProperty GameId = new StringReactiveProperty();

	public readonly StringReactiveProperty SaveName = new StringReactiveProperty();

	public readonly StringReactiveProperty Description = new StringReactiveProperty();

	public readonly StringReactiveProperty LocationName = new StringReactiveProperty();

	public readonly StringReactiveProperty TimeInGame = new StringReactiveProperty();

	public readonly ReactiveProperty<DateTime> SystemSaveTime = new ReactiveProperty<DateTime>();

	public readonly BoolReactiveProperty ShowDlcRequiredLabel = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsCurrentIronManSave = new BoolReactiveProperty();

	public List<List<string>> DlcRequiredMap = new List<List<string>>();

	public readonly BoolReactiveProperty ShowAutoSaveMark = new BoolReactiveProperty();

	public readonly BoolReactiveProperty ShowQuickSaveMark = new BoolReactiveProperty();

	public readonly ReactiveProperty<List<SaveLoadPortraitVM>> PartyPortraits = new ReactiveProperty<List<SaveLoadPortraitVM>>();

	public readonly ReactiveProperty<Texture2D> ScreenShot = new ReactiveProperty<Texture2D>();

	public readonly ReactiveProperty<Texture2D> ScreenShotHighRes = new ReactiveProperty<Texture2D>();

	public readonly BoolReactiveProperty ShowArrowPartyHint = new BoolReactiveProperty();

	private readonly SaveLoadActions m_SaveLoadActions;

	public SaveInfo Reference { get; private set; }

	public bool IsActuallySaved { get; private set; }

	private static UISaveLoadTexts SaveLoadTexts => BlueprintRoot.Instance.LocalizedTexts.UserInterfacesText.SaveLoadTexts;

	public bool ShowSaveLoadButton
	{
		get
		{
			if (Mode.Value != SaveLoadMode.Load)
			{
				return Reference.Type == SaveInfo.SaveType.Manual;
			}
			return true;
		}
	}

	public SaveSlotVM(SaveInfo saveInfo, IReadOnlyReactiveProperty<SaveLoadMode> mode, SaveLoadActions actions = default(SaveLoadActions), bool allowSwitchOff = false)
		: base(allowSwitchOff)
	{
		Mode = mode;
		AddDisposable(mode.Subscribe(SetMode));
		IsEmpty.Value = false;
		m_SaveLoadActions = actions;
		SetSaveInfo(saveInfo);
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Clear();
	}

	public void SaveOrLoad()
	{
		if (IsCurrentIronManSave.Value)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.CannotLoadCurrentIronManSave, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else if (PhotonManager.Lobby.IsActive && !PhotonManager.DLC.IsDLCsInLobbyReady)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.DlcListIsNotLoading, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else if (ShowDlcRequiredLabel.Value)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.DlcRequired, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			m_SaveLoadActions.SaveOrLoad?.Invoke(Reference);
		}
	}

	public void Delete()
	{
		m_SaveLoadActions.Delete?.Invoke(Reference);
	}

	public void DeleteWithoutBox()
	{
		m_SaveLoadActions.DeleteWithoutBox?.Invoke(Reference);
	}

	public void ShowScreenshot()
	{
		m_SaveLoadActions.ShowScreenshot?.Invoke(this);
	}

	public void HideScreenshot()
	{
		m_SaveLoadActions.ShowScreenshot?.Invoke(null);
	}

	public void SetSaveInfo(SaveInfo saveInfo)
	{
		Reference = saveInfo;
		IsEmpty.Value = Reference == null;
		if (Reference == null)
		{
			Clear();
			return;
		}
		IsActuallySaved = saveInfo.IsActuallySaved;
		CharacterName.Value = Reference.PlayerCharacterName;
		GameName.Value = Reference.Name;
		GameId.Value = Reference.GameId;
		SaveName.Value = Reference.Name ?? string.Empty;
		Description.Value = Reference.Description;
		StringReactiveProperty locationName = LocationName;
		object obj = Reference.AreaNameOverride;
		if (obj == null)
		{
			LocalizedString localizedString = SimpleBlueprintExtendAsObject.Or(Reference.Area, null)?.AreaName;
			obj = ((localizedString != null) ? ((string)localizedString) : "");
		}
		locationName.Value = (string)obj;
		SystemSaveTime.Value = Reference.SystemSaveTime;
		TimeInGame.Value = UIUtility.TimeSpanToInGameTime(Reference.GameTotalTime);
		BoolReactiveProperty showAutoSaveMark = ShowAutoSaveMark;
		SaveInfo.SaveType type = Reference.Type;
		showAutoSaveMark.Value = type == SaveInfo.SaveType.Auto || type == SaveInfo.SaveType.IronMan;
		ShowQuickSaveMark.Value = Reference.Type == SaveInfo.SaveType.Quick;
		if (Reference.PartyPortraits != null)
		{
			PartyPortraits.Value = Reference.PartyPortraits.Where((PortraitForSave portrait) => portrait != null).Take(12).Select(delegate(PortraitForSave portrait)
			{
				string rank = (portrait.IsMainCharacter ? Reference.PlayerCharacterRank.ToString() : string.Empty);
				return new SaveLoadPortraitVM(portrait.Data.SmallPortrait, rank);
			})
				.ToList();
			ShowArrowPartyHint.Value = Reference.PartyPortraits.Count > 6;
		}
		IsCurrentIronManSave.Value = saveInfo.Type == SaveInfo.SaveType.IronMan && !RootUIContext.Instance.IsMainMenu && saveInfo.GameId == Game.Instance.Player.GameId;
		CheckDLC();
	}

	public void TrySetSaveName(string newName)
	{
		if (!IsActuallySaved && Reference != null && !string.IsNullOrEmpty(newName))
		{
			SaveName.Value = newName.Trim();
			Reference.Name = newName.Trim();
		}
	}

	private void SetScreenShot()
	{
		ScreenShot.Value = Reference.Screenshot;
	}

	private void SetScreenShotHighRes()
	{
		ScreenShotHighRes.Value = Reference.ScreenshotHighRes;
	}

	private void SetMode(SaveLoadMode mode)
	{
		CheckDLC();
	}

	public void CheckDLC()
	{
		ShowDlcRequiredLabel.Value = Reference != null && !Reference.CheckDlcAvailable();
		if (Reference == null || Reference.CheckDlcAvailable())
		{
			return;
		}
		DlcRequiredMap = new List<List<string>>();
		foreach (List<IBlueprintDlc> item in Reference.GetRequiredDLCMap())
		{
			DlcRequiredMap.Add((from t in item.OfType<BlueprintDlc>()
				select t.GetDlcName()).ToList());
		}
	}

	private void Clear()
	{
		CharacterName.Value = string.Empty;
		GameName.Value = string.Empty;
		GameId.Value = string.Empty;
		SaveName.Value = string.Empty;
		Description.Value = string.Empty;
		LocationName.Value = string.Empty;
		TimeInGame.Value = string.Empty;
		ShowDlcRequiredLabel.Value = false;
		IsCurrentIronManSave.Value = false;
		ShowAutoSaveMark.Value = false;
		ShowQuickSaveMark.Value = false;
		ReactiveProperty<List<SaveLoadPortraitVM>> partyPortraits = PartyPortraits;
		if (partyPortraits != null && partyPortraits.Value != null)
		{
			foreach (SaveLoadPortraitVM item in PartyPortraits.Value)
			{
				item.Dispose();
			}
			PartyPortraits.Value = null;
		}
		ScreenShot.Value = null;
		ScreenShotHighRes.Value = null;
	}

	public bool ReferenceSaveEquals(SaveInfo saveInfo)
	{
		if (saveInfo == null || Reference == null)
		{
			return false;
		}
		return Reference.FolderName == saveInfo.FolderName;
	}

	protected override void DoSelectMe()
	{
		m_SaveLoadActions.Select?.Invoke(Reference);
	}

	public void UpdateScreenshot()
	{
		Game.Instance.SaveManager.LoadScreenshot(Reference, highRes: false, SetScreenShot);
	}

	public void UpdateHighResScreenshot()
	{
		Game.Instance.SaveManager.LoadScreenshot(Reference, highRes: true, SetScreenShotHighRes);
	}

	public void SetAvailable(bool state)
	{
		SetAvailableState(state);
	}

	public void SetAvailableAndActive(bool state)
	{
		SetAvailableState(state);
		Active.Value = state;
	}

	public void DisposeHighResScreenshot()
	{
		SaveScreenshotManager.Instance.DisposeScreenshotTexture(Reference.ScreenshotHighRes);
		Reference.ScreenshotHighRes = null;
		SetScreenShotHighRes();
	}
}
