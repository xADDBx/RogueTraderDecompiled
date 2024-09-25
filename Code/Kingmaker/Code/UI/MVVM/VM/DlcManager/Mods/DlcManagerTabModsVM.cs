using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Modding;
using Code.Utility.ExtendedModInfo;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager.Mods;

public class DlcManagerTabModsVM : DlcManagerTabBaseVM, ISettingsDescriptionUIHandler, ISubscriber
{
	public readonly ReactiveProperty<DlcManagerModEntityVM> SelectedEntity = new ReactiveProperty<DlcManagerModEntityVM>();

	public SelectionGroupRadioVM<DlcManagerModEntityVM> SelectionGroup;

	public bool HaveMods;

	public readonly InfoSectionVM InfoVM;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly bool m_IsMainMenu;

	public readonly BoolReactiveProperty IsSteam = new BoolReactiveProperty();

	public readonly ReactiveCommand<bool> CheckModNeedToReloadCommand = new ReactiveCommand<bool>();

	public readonly BoolReactiveProperty NeedReload = new BoolReactiveProperty();

	public DlcManagerTabModsVM(bool isMainMenu)
	{
		m_IsMainMenu = isMainMenu;
		IsSteam.Value = false;
		AddMods();
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
		AddDisposable(CheckModNeedToReloadCommand.Subscribe(delegate
		{
			NeedReload.Value = CheckNeedToReloadGame();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	private void AddMods()
	{
		ModInitializer.CheckForModUpdates();
		ExtendedModInfo[] array = (from m in ModInitializer.GetAllModsInfo()
			orderby m.Id
			select m).ToArray();
		List<DlcManagerModEntityVM> list = new List<DlcManagerModEntityVM>();
		ExtendedModInfo[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			DlcManagerModEntityVM dlcManagerModEntityVM = new DlcManagerModEntityVM(array2[i], m_IsMainMenu, CheckModNeedToReloadCommand);
			list.Add(dlcManagerModEntityVM);
			AddDisposable(dlcManagerModEntityVM);
		}
		HaveMods = list.Any();
		SelectionGroup = new SelectionGroupRadioVM<DlcManagerModEntityVM>(list, SelectedEntity);
		AddDisposable(SelectionGroup);
		SelectedEntity.Value = list.FirstOrDefault();
	}

	public void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		SetupTooltipTemplate(ownTitle, ownDescription);
	}

	public void HandleHideSettingsDescription()
	{
	}

	private void SetupTooltipTemplate(string ownTitle = null, string ownDescription = null)
	{
		m_ReactiveTooltipTemplate.Value = ((ownTitle != null || ownDescription != null) ? TooltipTemplate(ownTitle, ownDescription) : null);
	}

	private TooltipBaseTemplate TooltipTemplate(string ownTitle = null, string ownDescription = null)
	{
		return new TooltipTemplateSettingsEntityDescription(null, ownTitle, ownDescription);
	}

	public bool CheckNeedToReloadGame()
	{
		return SelectionGroup.EntitiesCollection.Any((DlcManagerModEntityVM e) => e.WarningReloadGame.Value);
	}

	public void SetModsCurrentState()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(DlcManagerModEntityVM e)
		{
			e.SetActualModState();
		});
	}

	public void ResetModsCurrentState()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(DlcManagerModEntityVM e)
		{
			e.ResetTempModState();
		});
	}

	public void OpenNexusMods()
	{
		Application.OpenURL("https://www.nexusmods.com/warhammer40kroguetrader");
	}

	public void OpenSteamWorkshop()
	{
		Application.OpenURL("https://steamcommunity.com/app/2186680/workshop/");
	}
}
