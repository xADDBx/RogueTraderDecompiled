using System.Collections.Generic;
using System.Linq;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager.SwitchOnDlcs;

public class DlcManagerTabSwitchOnDlcsVM : DlcManagerTabBaseVM, ISettingsDescriptionUIHandler, ISubscriber
{
	public readonly ReactiveProperty<DlcManagerSwitchOnDlcEntityVM> SelectedEntity = new ReactiveProperty<DlcManagerSwitchOnDlcEntityVM>();

	public SelectionGroupRadioVM<DlcManagerSwitchOnDlcEntityVM> SelectionGroup;

	public bool HaveDlcs;

	public readonly InfoSectionVM InfoVM;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly ReactiveCommand<bool> CheckModNeedToResaveCommand = new ReactiveCommand<bool>();

	public readonly BoolReactiveProperty NeedResave = new BoolReactiveProperty();

	public DlcManagerTabSwitchOnDlcsVM()
	{
		AddDlcs();
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
		AddDisposable(CheckModNeedToResaveCommand.Subscribe(delegate
		{
			NeedResave.Value = CheckNeedToResaveGame();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	private void AddDlcs()
	{
		IEnumerable<IBlueprintDlc> availableAdditionalContentDlcForCurrentCampaign = Game.Instance.Player.GetAvailableAdditionalContentDlcForCurrentCampaign();
		IEnumerable<IBlueprintDlc> second = from dlc in StoreManager.GetPurchasableDLCs()
			where dlc.DlcType == DlcTypeEnum.CosmeticDlc && dlc.IsAvailable
			select dlc;
		List<DlcManagerSwitchOnDlcEntityVM> list = (from dlc in availableAdditionalContentDlcForCurrentCampaign.Concat(second).OfType<BlueprintDlc>()
			where (!dlc.IsConsole() && !dlc.HideDlcForAll && (!dlc.HideWhoNotBuyDlc || dlc.IsPurchased)) || (dlc.IsConsole() && !dlc.HideDlcForAll && (!dlc.HideWhoNotBuyDlc || dlc.IsPurchased) && !dlc.HideInConsoleStores)
			orderby dlc.DlcType
			select dlc).ToList().Select(delegate(BlueprintDlc dlcEntity)
		{
			DlcManagerSwitchOnDlcEntityVM dlcManagerSwitchOnDlcEntityVM = new DlcManagerSwitchOnDlcEntityVM(dlcEntity, CheckModNeedToResaveCommand);
			AddDisposable(dlcManagerSwitchOnDlcEntityVM);
			return dlcManagerSwitchOnDlcEntityVM;
		}).ToList();
		HaveDlcs = list.Any();
		SelectionGroup = new SelectionGroupRadioVM<DlcManagerSwitchOnDlcEntityVM>(list, SelectedEntity);
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

	public bool CheckNeedToResaveGame()
	{
		return SelectionGroup.EntitiesCollection.Any((DlcManagerSwitchOnDlcEntityVM e) => e.WarningResaveGame.Value);
	}

	public void SetDlcsCurrentState()
	{
		List<BlueprintDlc> dlcList = (from dlc in SelectionGroup.EntitiesCollection
			where dlc.WarningResaveGame.Value
			select dlc.BlueprintDlc).ToList();
		Game.Instance.Player.ApplySwitchOnDlc(dlcList);
	}

	public void ResetDlcsCurrentState()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(DlcManagerSwitchOnDlcEntityVM e)
		{
			e.ResetTempDlcState();
		});
	}
}
