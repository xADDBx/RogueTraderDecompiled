using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance;
using Kingmaker.UI.MVVM.VM.CharGen.Portrait;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Photon.Realtime;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ChangeAppearance;

public class ChangeAppearanceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICharGenCloseHandler, ISubscriber, INetLobbyPlayersHandler
{
	public readonly CharGenContext CharGenContext;

	public readonly ReactiveProperty<CharGenPortraitVM> PortraitVM = new ReactiveProperty<CharGenPortraitVM>();

	public readonly CharGenAppearanceComponentAppearancePhaseVM CharGenAppearancePhaseVM;

	public readonly BoolReactiveProperty IsMainCharacter = new BoolReactiveProperty();

	public readonly ReactiveProperty<CharacterVisualSettingsVM> VisualSettingsVM = new ReactiveProperty<CharacterVisualSettingsVM>();

	public readonly ReactiveCommand<bool> CheckCoopControls = new ReactiveCommand<bool>();

	public readonly ReactiveProperty<bool> ShouldShowVisualSettings = new ReactiveProperty<bool>();

	private readonly Action m_CloseAction;

	private readonly Action<BaseUnitEntity> m_CompleteAction;

	private bool m_PortraitSynchronizingInProgress;

	private bool m_IsCustomCompanionChargen;

	private IDisposable m_PortraitSubscription;

	public ChangeAppearanceVM(CharGenConfig config, Action closeAction, Action<BaseUnitEntity> completeAction)
	{
		m_CloseAction = closeAction;
		m_CompleteAction = completeAction;
		CharGenContext = new CharGenContext(config);
		CharGenContext.InitForChangeAppearance();
		CharGenAppearancePhaseVM = new CharGenAppearanceComponentAppearancePhaseVM(CharGenContext);
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		AddDisposable(CharGenAppearancePhaseVM.ShowVisualSettings.Subscribe(delegate(bool value)
		{
			ShouldShowVisualSettings.Value = value;
		}));
		AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		CharGenAppearancePhaseVM.Dispose();
		CharGenContext.Dispose();
		ClearPortrait();
		HideVisualSettings();
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager != null)
		{
			ClearPortrait();
			m_PortraitSubscription = CharGenContext.Doll.GetReactiveProperty((DollState dollState) => dollState.PortraitData).Subscribe(delegate(PortraitData value)
			{
				PortraitVM.Value?.Dispose();
				PortraitVM.Value = new CharGenPortraitVM(value);
			});
		}
	}

	private void ClearPortrait()
	{
		m_PortraitSubscription?.Dispose();
		m_PortraitSubscription = null;
		PortraitVM.Value?.Dispose();
		PortraitVM.Value = null;
	}

	public void Close()
	{
		Game.Instance.GameCommandQueue.CharGenClose(withComplete: false, syncPortrait: false);
	}

	public void Complete()
	{
		UISounds.Instance.Play(UISounds.Instance.Sounds.Buttons.FinishChargenButtonClick, isButton: false, playAnyway: true);
		bool syncPortrait = PhotonManager.Lobby.IsActive && PhotonManager.Initialized && PhotonManager.Instance.PortraitSyncer.IsNeedSyncPortrait();
		Game.Instance.GameCommandQueue.CharGenClose(withComplete: true, syncPortrait);
	}

	async void ICharGenCloseHandler.HandleClose(bool withComplete, bool syncPortrait)
	{
		if (syncPortrait)
		{
			if (!m_PortraitSynchronizingInProgress)
			{
				m_PortraitSynchronizingInProgress = true;
				try
				{
					await PhotonManager.Instance.PortraitSyncer.SyncPortraits(UINetUtility.IsControlMainCharacter());
				}
				finally
				{
					m_PortraitSynchronizingInProgress = false;
				}
				if (UINetUtility.IsControlMainCharacter())
				{
					Game.Instance.GameCommandQueue.CharGenClose(withComplete, syncPortrait: false);
				}
			}
		}
		else if (withComplete)
		{
			if (!m_IsCustomCompanionChargen)
			{
				SoundState.Instance.OnChargenChange(chargen: false);
			}
			else
			{
				SoundState.Instance.OnMusicStateChange(MusicStateHandler.MusicState.Setting);
			}
			if (CharGenContext.LevelUpManager.Value != null)
			{
				CharGenContext.LevelUpManager.Value.Commit();
				CharGenContext.LevelUpManager.Value = null;
			}
			m_CompleteAction?.Invoke(CharGenContext.CurrentUnit.Value);
		}
		else
		{
			m_CloseAction?.Invoke();
		}
	}

	public void SwitchVisualSettings()
	{
		if (VisualSettingsVM.Value == null)
		{
			ShowVisualSettings();
		}
		else
		{
			HideVisualSettings();
		}
	}

	public void ShowVisualSettings()
	{
		if (VisualSettingsVM.Value == null)
		{
			CharacterVisualSettingsVM disposable = (VisualSettingsVM.Value = new CharacterVisualSettingsVM(CharGenContext.Doll, HideVisualSettings));
			AddDisposable(disposable);
		}
	}

	public void HideVisualSettings()
	{
		DisposeAndRemove(VisualSettingsVM);
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		CheckCoopControls.Execute(UINetUtility.IsControlMainCharacter());
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}
}
