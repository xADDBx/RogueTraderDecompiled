using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.Settings;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Decorative;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame.Difficulty;

public class NewGamePhaseDifficultyVM : NewGamePhaseBaseVm, ISettingsDescriptionUIHandler, ISubscriber, IOptionsWindowUIHandler
{
	private readonly ReactiveCollection<VirtualListElementVMBase> m_SettingEntities = new ReactiveCollection<VirtualListElementVMBase>();

	public readonly InfoSectionVM InfoVM;

	public readonly ReactiveProperty<TooltipBaseTemplate> ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	public readonly BoolReactiveProperty IsDefaultButtonInteractable = new BoolReactiveProperty();

	public IReadOnlyReactiveCollection<VirtualListElementVMBase> SettingEntities => m_SettingEntities;

	public NewGamePhaseDifficultyVM(Action backStep, Action nextStep)
		: base(backStep, nextStep)
	{
		SwitchSettingsScreen(UISettingsManager.SettingsScreen.Difficulty);
		m_SettingEntities.ForEach(base.AddDisposable);
		SettingsRoot.Difficulty.GameDifficulty.SetValueAndConfirm(GameDifficultyOption.Normal);
		SettingsRoot.Difficulty.OnlyOneSave.SetValueAndConfirm(value: false);
		IsDefaultButtonInteractable.Value = CheckDefaultButton();
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		HandleHideSettingsDescription();
	}

	private void SwitchSettingsScreen(UISettingsManager.SettingsScreen settingsScreen)
	{
		m_SettingEntities.Clear();
		foreach (UISettingsGroup settings in Game.Instance.UISettingsManager.GetSettingsList(settingsScreen))
		{
			if (!settings.IsVisible)
			{
				continue;
			}
			m_SettingEntities.Add(new SettingsEntityHeaderVM(settings.Title));
			foreach (UISettingsEntityBase visibleSettings in settings.VisibleSettingsList)
			{
				m_SettingEntities.Add(SettingsVM.GetVMForSettingsItem(visibleSettings, isNewGame: true));
			}
		}
		foreach (UISettingsGroup settings2 in Game.Instance.UISettingsManager.GetSettingsList(settingsScreen))
		{
			if (!settings2.IsVisible)
			{
				continue;
			}
			{
				foreach (UISettingsEntityBase visibleSettings2 in settings2.VisibleSettingsList)
				{
					if (!(visibleSettings2 is UISettingsEntityDisplayImages) && !(visibleSettings2 is UISettingsEntityAccessiabilityImage))
					{
						HandleShowSettingsDescription(visibleSettings2);
						break;
					}
				}
				break;
			}
		}
	}

	public void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		SetupTooltipTemplate(entity, ownTitle, ownDescription);
	}

	public void HandleHideSettingsDescription()
	{
	}

	private void SetupTooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		ReactiveTooltipTemplate.Value = ((entity != null || ownTitle != null || ownDescription != null) ? TooltipTemplate(entity, ownTitle, ownDescription) : null);
	}

	private TooltipBaseTemplate TooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		return new TooltipTemplateSettingsEntityDescription(entity, ownTitle, ownDescription);
	}

	public void OpenDefaultSettingsDialog()
	{
		string text = string.Format(Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SettingsUI.RestoreAllDefaultsMessage, UIStrings.Instance.NewGameWin.DifficultyMenuLabel.Text);
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(text, DialogMessageBoxBase.BoxType.Dialog, OnDefaultDialogAnswer);
		});
	}

	private void OnDefaultDialogAnswer(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			SettingsRoot.Difficulty.GameDifficulty.SetValueAndConfirm(GameDifficultyOption.Normal);
		}
	}

	public void HandleItemChanged(string key)
	{
		IsDefaultButtonInteractable.Value = CheckDefaultButton();
	}

	private bool CheckDefaultButton()
	{
		return (from @group in Game.Instance.UISettingsManager.GetSettingsList(UISettingsManager.SettingsScreen.Difficulty)
			where @group.IsVisible
			select @group).Any((UISettingsGroup group) => group.VisibleSettingsList.Where((UISettingsEntityBase setting) => !setting.NoDefaultReset).Any((UISettingsEntityBase setting) => setting is IUISettingsEntityWithValueBase iUISettingsEntityWithValueBase && iUISettingsEntityWithValueBase.SettingsEntity.CurrentValueIsNotDefault()));
	}
}
