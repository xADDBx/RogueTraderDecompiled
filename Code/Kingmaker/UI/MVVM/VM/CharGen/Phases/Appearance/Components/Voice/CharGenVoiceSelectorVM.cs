using System;
using System.Linq;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Voice;
using Kingmaker.Utility.GameConst;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Voice;

public class CharGenVoiceSelectorVM : BaseCharGenAppearancePageComponentVM, ICharGenAppearancePhaseVoiceHandler, ISubscriber, ICharGenChangePhaseHandler
{
	public SelectionGroupRadioVM<CharGenVoiceItemVM> VoiceSelector;

	private readonly ReactiveProperty<CharGenVoiceItemVM> m_SelectedVoiceVM = new ReactiveProperty<CharGenVoiceItemVM>();

	private readonly ReactiveProperty<UnitAsksComponent> m_Barks = new ReactiveProperty<UnitAsksComponent>();

	private readonly ReactiveCollection<CharGenVoiceItemVM> m_VoicesList = new ReactiveCollection<CharGenVoiceItemVM>();

	private readonly CharGenContext m_CharGenContext;

	private SelectionStateVoice m_SelectionStateVoice;

	private bool m_IsSelectedManually;

	private CharGenPhaseType m_CurrentPhase;

	private Gender DollGender => m_CharGenContext.Doll.Gender;

	public CharGenVoiceSelectorVM(CharGenContext ctx)
	{
		m_CharGenContext = ctx;
		AddDisposable(EventBus.Subscribe(this));
		CreateVoices();
		AddDisposable(m_CharGenContext.CurrentUnit.Subscribe(delegate
		{
			DelayedInvoker.InvokeInFrames(UpdateFromMechanic, 1);
		}));
		AddDisposable(m_CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
		AddDisposable(m_SelectedVoiceVM.Subscribe(delegate(CharGenVoiceItemVM value)
		{
			OnChooseVoice(value);
			m_IsSelectedManually = true;
		}));
		SoundBanksManager.LoadBank(UIConsts.PCDemoVoicesENGBank);
		m_IsSelectedManually = false;
	}

	protected override void DisposeImplementation()
	{
		m_VoicesList.Clear();
		SoundBanksManager.UnloadBank(UIConsts.PCDemoVoicesENGBank);
	}

	private void CreateVoices()
	{
		m_VoicesList.Clear();
		foreach (BlueprintUnitAsksList voice in BlueprintRoot.Instance.CharGenRoot.Voices)
		{
			CharGenVoiceItemVM charGenVoiceItemVM = new CharGenVoiceItemVM(voice);
			AddDisposable(charGenVoiceItemVM);
			if (!charGenVoiceItemVM.IsEmptyVoice)
			{
				m_VoicesList.Add(charGenVoiceItemVM);
			}
		}
		VoiceSelector = AddDisposableAndReturn(new SelectionGroupRadioVM<CharGenVoiceItemVM>(m_VoicesList, m_SelectedVoiceVM));
	}

	private void UpdateSelector()
	{
		m_IsAvailable.Value = m_VoicesList.Any();
	}

	public override void OnBeginView()
	{
		UpdateSelector();
		if (m_CharGenContext.Doll.TrackPortrait && !m_IsSelectedManually)
		{
			UpdateFromMechanic();
		}
		if (m_SelectedVoiceVM.Value != null)
		{
			OnChooseVoice(m_SelectedVoiceVM.Value);
		}
		else
		{
			UpdateFromMechanic();
		}
	}

	private void UpdateFromMechanic()
	{
		LevelUpManager value = m_CharGenContext.LevelUpManager.Value;
		BlueprintUnitAsksList pregenVoice = value.PreviewUnit.Asks.List;
		if (!m_CharGenContext.IsCustomCharacter.Value && pregenVoice != null)
		{
			m_SelectedVoiceVM.Value = VoiceSelector.EntitiesCollection.FirstOrDefault((CharGenVoiceItemVM item) => item.Voice == pregenVoice);
			return;
		}
		BlueprintCharGenRoot charGenRoot = BlueprintRoot.Instance.CharGenRoot;
		int value2 = ((DollGender == Gender.Male) ? charGenRoot.MaleVoiceDefaultId : charGenRoot.FemaleVoiceDefaultId);
		value2 = Math.Clamp(value2, 0, VoiceSelector.EntitiesCollection.Count - 1);
		m_SelectedVoiceVM.Value = VoiceSelector.EntitiesCollection.ElementAt(value2);
		m_IsSelectedManually = false;
	}

	private void OnChooseVoice(CharGenVoiceItemVM voice)
	{
		if (voice?.Voice != null)
		{
			Game.Instance.GameCommandQueue.CharGenChangeVoice(voice.Voice);
		}
	}

	void ICharGenAppearancePhaseVoiceHandler.HandleChangeVoice(BlueprintUnitAsksList blueprint)
	{
		CharGenVoiceItemVM charGenVoiceItemVM = m_VoicesList.FirstOrDefault((CharGenVoiceItemVM elem) => blueprint == elem?.Voice);
		if (charGenVoiceItemVM == null)
		{
			PFLog.UI.Error("BlueprintUnitAsksList not found! ID=" + blueprint.AssetGuid);
			return;
		}
		m_SelectedVoiceVM.Value = charGenVoiceItemVM;
		m_SelectionStateVoice?.SelectVoice(charGenVoiceItemVM.Voice);
		m_Barks.Value = charGenVoiceItemVM.Barks;
		if (!m_CharGenContext.IsCustomCharacter.Value || m_CurrentPhase != 0)
		{
			m_Barks.Value.PlayPreview();
		}
		Changed();
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager != null)
		{
			BlueprintVoiceSelection selectionByType = CharGenUtility.GetSelectionByType<BlueprintVoiceSelection>(manager.Path);
			if (selectionByType != null)
			{
				m_SelectionStateVoice = manager.GetSelectionState(manager.Path, selectionByType, 0) as SelectionStateVoice;
			}
		}
	}

	public void HandlePhaseChange(CharGenPhaseType phaseType)
	{
		m_CurrentPhase = phaseType;
	}
}
