using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Kingmaker.UI.Sound;
using Newtonsoft.Json;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Kingmaker.Code.UI.MVVM.VM.MainMenu;

public class MainMenuSideBarVM : VMBase, ISavesUpdatedHandler, ISubscriber, ILocalizationHandler
{
	public readonly ContextMenuEntityVM ContinueVm;

	public readonly ContextMenuEntityVM NewGameVm;

	public readonly ContextMenuEntityVM LoadVm;

	public readonly ContextMenuEntityVM NetVm;

	public readonly ContextMenuEntityVM LicenseVm;

	public readonly ContextMenuEntityVM CreditVm;

	public readonly ContextMenuEntityVM OptionsVm;

	public readonly ContextMenuEntityVM FeedbackVm;

	public readonly ContextMenuEntityVM ExitVm;

	private ReactiveProperty<SaveLoadVM> m_SaveLoadVM;

	private readonly List<ContextMenuEntityVM> m_Entities = new List<ContextMenuEntityVM>();

	private SaveStreamer m_SaveStreamer;

	private readonly IUIMainMenu m_MainMenu;

	public readonly ReactiveCommand LanguageChanged = new ReactiveCommand();

	public IReadOnlyReactiveProperty<SaveLoadVM> SaveLoadVM => m_SaveLoadVM;

	public bool ExitEnabled => true;

	public MainMenuSideBarVM(IUIMainMenu mainMenu)
	{
		AddDisposable(EventBus.Subscribe(this));
		UIStrings userInterfacesText = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText;
		m_MainMenu = mainMenu;
		m_Entities.Add(ContinueVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(userInterfacesText.MainMenu.Continue, mainMenu.LoadLastGame, () => Game.Instance.SaveManager.AreSavesUpToDate && Game.Instance.SaveManager.GetLatestSave() != null, UISounds.ButtonSoundsEnum.AnalogSound, UISounds.ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(LoadVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(userInterfacesText.MainMenu.LoadGame, delegate
		{
			EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
			{
				h.HandleOpenSaveLoad(SaveLoadMode.Load, singleMode: true);
			});
		}, () => Game.Instance.SaveManager.AreSavesUpToDate && Game.Instance.SaveManager.HasAnySaves(includingCorrupted: true), UISounds.ButtonSoundsEnum.AnalogSound, UISounds.ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(NewGameVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(userInterfacesText.MainMenu.NewGame, mainMenu.ShowNewGameSetup, null, UISounds.ButtonSoundsEnum.AnalogSound, UISounds.ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(NetVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(userInterfacesText.NetLobbyTexts.NetHeader, mainMenu.ShowNetLobby, null, UISounds.ButtonSoundsEnum.AnalogSound, UISounds.ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(OptionsVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(userInterfacesText.MainMenu.Settings, mainMenu.OpenSettings, null, UISounds.ButtonSoundsEnum.AnalogSound, UISounds.ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(CreditVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(userInterfacesText.MainMenu.Credits, mainMenu.ShowCredits, null, UISounds.ButtonSoundsEnum.AnalogSound, UISounds.ButtonSoundsEnum.AnalogSound)));
		m_Entities.Add(LicenseVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(userInterfacesText.MainMenu.License, mainMenu.ShowLicense)));
		m_Entities.Add(FeedbackVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(userInterfacesText.MainMenu.Feedback, mainMenu.ShowFeedback)));
		m_Entities.Add(ExitVm = new ContextMenuEntityVM(new ContextMenuCollectionEntity(userInterfacesText.MainMenu.Exit, mainMenu.Exit, () => ExitEnabled, UISounds.ButtonSoundsEnum.AnalogSound, UISounds.ButtonSoundsEnum.AnalogSound)));
		m_Entities.ForEach(base.AddDisposable);
		StoreManager.OnRefreshDLC += OnSaveListUpdated;
	}

	protected override void DisposeImplementation()
	{
		StoreManager.OnRefreshDLC -= OnSaveListUpdated;
		m_Entities.Clear();
	}

	public void ShowLicense()
	{
		m_MainMenu.ShowLicense();
	}

	public void ShowFeedback()
	{
		m_MainMenu.ShowFeedback();
	}

	public void OpenUrl(FeedbackPopupItemType itemType)
	{
		FeedbackPopupItem feedbackPopupItem = FeedbackPopupConfigLoader.Instance.Items.FirstOrDefault((FeedbackPopupItem i) => i.ItemType == itemType);
		Application.OpenURL((feedbackPopupItem != null) ? feedbackPopupItem.Url : "https://roguetrader.owlcat.games/");
	}

	public void OnSaveListUpdated()
	{
		foreach (ContextMenuEntityVM entity in m_Entities)
		{
			entity.RefreshEnabling();
		}
	}

	public async Task OnStreamSaves()
	{
		if (m_SaveStreamer == null)
		{
			m_SaveStreamer = new SaveStreamer();
		}
		await m_SaveStreamer.StreamSaves();
	}

	private static bool TryGetIntroductoryLocalFileText(string path, out string text)
	{
		text = string.Empty;
		if (!File.Exists(path))
		{
			return false;
		}
		try
		{
			LocalizedStringData localizedStringData = JsonConvert.DeserializeObject<LocalizedStringData>(File.ReadAllText(path));
			if (localizedStringData == null)
			{
				PFLog.UI.Error("Introductory Text File is bad on the path " + path);
				return false;
			}
			text = localizedStringData.GetText(LocalizationManager.Instance.CurrentLocale);
			return true;
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
			return false;
		}
	}

	public void GetIntroductoryText(Action<string> callback)
	{
		string localFileName = "IntroductoryText.json";
		MainThreadDispatcher.StartCoroutine(DownloadText("https://owlcat.games/raw/hmpEuY", callback, localFileName));
	}

	private static IEnumerator DownloadText(string url, Action<string> callback, string localFileName)
	{
		using UnityWebRequest www = UnityWebRequest.Get(url);
		string text = Path.Combine(Application.streamingAssetsPath, localFileName);
		string cachedFilePath = Path.Combine(Application.persistentDataPath, localFileName);
		if (!TryGetIntroductoryLocalFileText(cachedFilePath, out var text2) && !TryGetIntroductoryLocalFileText(text, out text2))
		{
			PFLog.UI.Error("Failed to load Introductory Text from default path: " + text);
		}
		callback?.Invoke(text2);
		PFLog.UI.Log("Downloading introductory text from URL: " + url);
		yield return www.SendWebRequest();
		if (www.result == UnityWebRequest.Result.Success)
		{
			string text3 = www.downloadHandler.text;
			try
			{
				File.WriteAllText(cachedFilePath, text3);
			}
			catch (Exception ex)
			{
				PFLog.UI.Error("JSON not saved in: " + cachedFilePath + ": " + ex.Message);
			}
			try
			{
				string obj = JsonConvert.DeserializeObject<LocalizedStringData>(text3)?.GetText(LocalizationManager.Instance.CurrentLocale);
				callback?.Invoke(obj);
				yield break;
			}
			catch (Exception ex2)
			{
				PFLog.UI.Error("Error deserializing introductory text: " + ex2.Message);
				callback?.Invoke(string.Empty);
				yield break;
			}
		}
		PFLog.UI.Error("Error downloading introductory text: " + www.error);
	}

	public void HandleLanguageChanged()
	{
		m_Entities.ForEach(delegate(ContextMenuEntityVM e)
		{
			e.UpdateTitle();
		});
		LanguageChanged.Execute();
	}
}
