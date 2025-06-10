using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Career;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.DarkestHour;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Homeworld;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Homeworld.ChildPhases;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.MomentOfTriumph;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Navigator;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Occupation;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Occupation.ChildPhases;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Ship;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.SoulMark;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Stats;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Summary;
using Kingmaker.UI.MVVM.VM.CharGen.Portrait;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UniRx;
using Photon.Realtime;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public class CharGenVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ILevelUpManagerUIHandler, ISubscriber, ICharGenCloseHandler, INetLobbyPlayersHandler
{
	public readonly CharGenContext CharGenContext;

	private readonly List<CharGenPhaseBaseVM> m_PhasesList = new List<CharGenPhaseBaseVM>();

	public readonly SelectionGroupRadioVM<CharGenPhaseBaseVM> PhasesSelectionGroupRadioVM;

	public readonly ReactiveProperty<CharGenPhaseBaseVM> CurrentPhaseVM = new ReactiveProperty<CharGenPhaseBaseVM>();

	public readonly ReactiveProperty<CharGenPortraitVM> PortraitVM = new ReactiveProperty<CharGenPortraitVM>();

	public readonly ReactiveProperty<CharacterVisualSettingsVM> VisualSettingsVM = new ReactiveProperty<CharacterVisualSettingsVM>();

	public readonly ReactiveProperty<bool> CurrentPhaseIsCompleted = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> ShouldShowVisualSettings = new ReactiveProperty<bool>();

	private readonly ReactiveCollection<CharGenPhaseBaseVM> m_PhasesCollection = new ReactiveCollection<CharGenPhaseBaseVM>();

	private CharGenPhaseBaseVM m_LastPhase;

	private readonly Action m_CloseAction;

	private readonly Action<BaseUnitEntity> m_CompleteAction;

	private IDisposable m_PortraitSubscription;

	private IDisposable m_PhaseIsCompletedSubscription;

	private CompositeDisposable m_PhaseSubscriptions = new CompositeDisposable();

	private bool m_IsPhasesDirty;

	private readonly bool m_IsCustomCompanionChargen;

	public readonly ReactiveCommand<bool> CheckCoopControls = new ReactiveCommand<bool>();

	public readonly BoolReactiveProperty IsMainCharacter = new BoolReactiveProperty();

	private bool m_PortraitSynchronizingInProgress;

	public IReadOnlyReactiveCollection<CharGenPhaseBaseVM> PhasesCollection => m_PhasesCollection;

	public bool CurrentPhaseCanInterrupt => CurrentPhaseVM.Value?.CanInterruptChargen ?? false;

	public CharGenPhaseBaseVM LastPhase => m_LastPhase;

	public CharGenVM(CharGenConfig config, Action closeAction, Action<BaseUnitEntity> completeAction, bool isCustomCompanionChargen)
	{
		m_CloseAction = closeAction;
		m_CompleteAction = completeAction;
		m_IsCustomCompanionChargen = isCustomCompanionChargen;
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		AddDisposable(CharGenContext = new CharGenContext(config));
		AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
		AddDisposable(CharGenContext.IsCustomCharacter.Subscribe(delegate
		{
			m_IsPhasesDirty = true;
			HideVisualSettings();
		}));
		UpdatePhases();
		PhasesSelectionGroupRadioVM = AddDisposableAndReturn(new SelectionGroupRadioVM<CharGenPhaseBaseVM>(m_PhasesCollection, CurrentPhaseVM));
		PhasesSelectionGroupRadioVM.TrySelectFirstValidEntity();
		AddDisposable(CurrentPhaseVM.ObserveLastValueOnLateUpdate().Subscribe(delegate(CharGenPhaseBaseVM phase)
		{
			ClearPhaseSubscription();
			HideVisualSettings();
			if (phase != null)
			{
				m_PhaseSubscriptions.Add(phase.IsCompletedAndAvailable.Subscribe(delegate(bool value)
				{
					CurrentPhaseIsCompleted.Value = value;
				}));
				m_PhaseSubscriptions.Add(phase.ShowVisualSettings.Subscribe(delegate(bool value)
				{
					ShouldShowVisualSettings.Value = value;
				}));
			}
		}));
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.LateUpdateAsObservable(), delegate
		{
			UpdateState();
		}));
	}

	protected override void DisposeImplementation()
	{
		ClearPhaseSubscription();
		ClearPortrait();
		HideVisualSettings();
		foreach (CharGenPhaseBaseVM phases in m_PhasesList)
		{
			phases.Dispose();
		}
		m_PhasesList.Clear();
	}

	private void UpdateState()
	{
		if (m_IsPhasesDirty)
		{
			UpdatePhases();
		}
	}

	public void Complete()
	{
		UISounds.Instance.Play(UISounds.Instance.Sounds.Buttons.FinishChargenButtonClick, isButton: false, playAnyway: true);
		bool syncPortrait = PhotonManager.Lobby.IsActive && PhotonManager.Initialized && PhotonManager.Instance.PortraitSyncer.IsNeedSyncPortrait();
		Game.Instance.GameCommandQueue.CharGenClose(withComplete: true, syncPortrait);
	}

	public void Close()
	{
		Game.Instance.GameCommandQueue.CharGenClose(withComplete: false, syncPortrait: false);
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
			CloseWithComplete();
		}
		else
		{
			CloseWithoutComplete();
		}
	}

	private void CloseWithComplete()
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
		CharGenContext.CurrentUnit.Value.Body.Initialize();
		CharGenContext.CurrentUnit.Value.Body.InitializeWeapons(CharGenContext.CurrentUnit.Value.Body.Owner.OriginalBlueprint.Body);
		m_CompleteAction?.Invoke(CharGenContext.CurrentUnit.Value);
	}

	private void CloseWithoutComplete()
	{
		m_CloseAction?.Invoke();
	}

	public void SetLastPhase(CharGenPhaseBaseVM viewModel)
	{
		m_LastPhase = viewModel;
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

	private void HandleUpdatePhases()
	{
		m_IsPhasesDirty = true;
		HideVisualSettings();
	}

	private void UpdatePhases()
	{
		List<CharGenPhaseBaseVM> phasesList = m_PhasesList;
		bool value = CharGenContext.IsCustomCharacter.Value;
		bool flag = CharGenContext.CharGenConfig.Mode == CharGenConfig.CharGenMode.NewGame;
		if (TryClearPhaseFromList<CharGenPregenPhaseVM>(check: true, phasesList))
		{
			AddPhase(phasesList, new CharGenPregenPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenAppearanceComponentAppearancePhaseVM>(check: true, phasesList))
		{
			AddPhase(phasesList, new CharGenAppearanceComponentAppearancePhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenSoulMarkPhaseVM>(value && !flag, phasesList))
		{
			AddPhase(phasesList, new CharGenSoulMarkPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenHomeworldPhaseVM>(value, phasesList))
		{
			AddPhase(phasesList, new CharGenHomeworldPhaseVM(CharGenContext));
		}
		bool check = HasFeatureGroupInSelections(FeatureGroup.ChargenImperialWorld);
		if (TryClearPhaseFromList<CharGenImperialHomeworldChildPhaseVM>(check, phasesList))
		{
			AddPhase(phasesList, new CharGenImperialHomeworldChildPhaseVM(CharGenContext));
		}
		bool check2 = HasFeatureGroupInSelections(FeatureGroup.ChargenForgeWorld);
		if (TryClearPhaseFromList<CharGenForgeHomeworldChildPhaseVM>(check2, phasesList))
		{
			AddPhase(phasesList, new CharGenForgeHomeworldChildPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenOccupationPhaseVM>(value, phasesList))
		{
			AddPhase(phasesList, new CharGenOccupationPhaseVM(CharGenContext));
		}
		bool check3 = HasFeatureGroupInSelections(FeatureGroup.ChargenNavigator);
		if (TryClearPhaseFromList<CharGenNavigatorPhaseVM>(check3, phasesList))
		{
			AddPhase(phasesList, new CharGenNavigatorPhaseVM(CharGenContext));
		}
		bool check4 = HasFeatureGroupInSelections(FeatureGroup.ChargenPsyker);
		if (TryClearPhaseFromList<CharGenSanctionedPsykerChildPhaseVM>(check4, phasesList))
		{
			AddPhase(phasesList, new CharGenSanctionedPsykerChildPhaseVM(CharGenContext));
		}
		bool check5 = HasFeatureGroupInSelections(FeatureGroup.ChargenArbitrator);
		if (TryClearPhaseFromList<CharGenArbitratorChildPhaseVM>(check5, phasesList))
		{
			AddPhase(phasesList, new CharGenArbitratorChildPhaseVM(CharGenContext));
		}
		bool check6 = HasFeatureGroupInSelections(FeatureGroup.ChargenMomentOfTriumph);
		if (TryClearPhaseFromList<CharGenMomentOfTriumphPhaseVM>(check6, phasesList))
		{
			AddPhase(phasesList, new CharGenMomentOfTriumphPhaseVM(CharGenContext));
		}
		bool check7 = HasFeatureGroupInSelections(FeatureGroup.ChargenDarkestHour);
		if (TryClearPhaseFromList<CharGenDarkestHourPhaseVM>(check7, phasesList))
		{
			AddPhase(phasesList, new CharGenDarkestHourPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenCareerPhaseVM>(value, phasesList))
		{
			AddPhase(phasesList, new CharGenCareerPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenAttributesPhaseVM>(value, phasesList))
		{
			AddPhase(phasesList, new CharGenAttributesPhaseVM(CharGenContext, CurrentPhaseVM));
		}
		if (TryClearPhaseFromList<CharGenShipPhaseVM>(flag, phasesList))
		{
			AddPhase(phasesList, new CharGenShipPhaseVM(CharGenContext));
		}
		if (TryClearPhaseFromList<CharGenSummaryPhaseVM>(check: true, phasesList))
		{
			AddPhase(phasesList, new CharGenSummaryPhaseVM(CharGenContext));
		}
		UpdatePhasesLinks();
		m_IsPhasesDirty = false;
	}

	private bool TryClearPhaseFromList<TPhase>(bool check, List<CharGenPhaseBaseVM> list) where TPhase : CharGenPhaseBaseVM
	{
		if (check)
		{
			if (!list.Any((CharGenPhaseBaseVM ph) => ph is TPhase))
			{
				return true;
			}
		}
		else
		{
			TPhase val = null;
			foreach (CharGenPhaseBaseVM item in list)
			{
				if (item is TPhase val2)
				{
					val = val2;
				}
			}
			if (val != null && !(CurrentPhaseVM.Value is TPhase))
			{
				RemoveDisposable(val);
				val.Dispose();
				list.Remove(val);
			}
		}
		return false;
	}

	private void AddPhase(List<CharGenPhaseBaseVM> list, CharGenPhaseBaseVM phase)
	{
		AddDisposable(phase);
		list.Add(phase);
	}

	private void UpdatePhasesLinks()
	{
		if (!m_PhasesList.Any())
		{
			return;
		}
		m_PhasesList.Sort((CharGenPhaseBaseVM a, CharGenPhaseBaseVM b) => a.OrderPriority.CompareTo(b.OrderPriority));
		List<CharGenPhaseBaseVM> list = m_PhasesCollection.ToList();
		for (int i = 0; i < m_PhasesList.Count; i++)
		{
			CharGenPhaseBaseVM item = m_PhasesList[i];
			if (m_PhasesCollection.Contains(item))
			{
				m_PhasesCollection.Move(m_PhasesCollection.IndexOf(item), i);
				list.Remove(item);
			}
			else
			{
				m_PhasesCollection.Insert(i, item);
			}
		}
		foreach (CharGenPhaseBaseVM item2 in list)
		{
			item2.Dispose();
			RemoveDisposable(item2);
			m_PhasesCollection.Remove(item2);
		}
		m_PhasesCollection[0].UpdateAvailableState(previousIsCompleted: true);
		for (int j = 0; j < m_PhasesCollection.Count - 1; j++)
		{
			m_PhasesCollection[j].SetNextPhase(m_PhasesCollection[j + 1]);
		}
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

	private void ClearPhaseSubscription()
	{
		m_PhaseSubscriptions.Clear();
		CurrentPhaseIsCompleted.Value = false;
	}

	private bool HasFeatureGroupInSelections(FeatureGroup featureGroup)
	{
		if (!CharGenContext.IsCustomCharacter.Value)
		{
			return false;
		}
		LevelUpManager value = CharGenContext.LevelUpManager.Value;
		if (value == null)
		{
			return false;
		}
		return CharGenUtility.GetFeatureSelectionsByGroup(value.Path, featureGroup, value.PreviewUnit).Any();
	}

	public void HandleCreateLevelUpManager(LevelUpManager manager)
	{
	}

	public void HandleDestroyLevelUpManager()
	{
	}

	public void HandleUISelectCareerPath()
	{
	}

	public void HandleUICommitChanges()
	{
	}

	public void HandleUISelectionChanged()
	{
		m_IsPhasesDirty = true;
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
