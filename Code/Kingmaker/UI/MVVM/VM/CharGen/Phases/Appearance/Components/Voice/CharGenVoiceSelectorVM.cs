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
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Voice;

public class CharGenVoiceSelectorVM : BaseCharGenAppearancePageComponentVM, ICharGenAppearancePhaseVoiceHandler, ISubscriber
{
	public SelectionGroupRadioVM<CharGenVoiceItemVM> VoiceSelector;

	public readonly ReactiveProperty<CharGenVoiceItemVM> SelectedVoiceVM = new ReactiveProperty<CharGenVoiceItemVM>();

	public readonly ReactiveProperty<UnitAsksComponent> Barks = new ReactiveProperty<UnitAsksComponent>();

	private readonly ReactiveCollection<CharGenVoiceItemVM> m_VoicesList = new ReactiveCollection<CharGenVoiceItemVM>();

	private readonly CharGenContext m_CharGenContext;

	private SelectionStateVoice m_SelectionStateVoice;

	private Gender DollGender => m_CharGenContext.Doll.Gender;

	public CharGenVoiceSelectorVM(CharGenContext ctx)
	{
		m_CharGenContext = ctx;
		AddDisposable(EventBus.Subscribe(this));
		CreateVoices();
		AddDisposable(m_CharGenContext.CurrentUnit.Subscribe(delegate
		{
			UpdateFromMechanic();
		}));
		AddDisposable(m_CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
		AddDisposable(SelectedVoiceVM.Subscribe(OnChooseVoice));
		SoundBanksManager.LoadBank(UIConsts.PCDemoVoicesENGBank);
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
		VoiceSelector = AddDisposableAndReturn(new SelectionGroupRadioVM<CharGenVoiceItemVM>(m_VoicesList, SelectedVoiceVM));
	}

	private void UpdateSelector()
	{
		m_IsAvailable.Value = m_VoicesList.Any();
	}

	public override void OnBeginView()
	{
		UpdateSelector();
		if (SelectedVoiceVM.Value != null)
		{
			OnChooseVoice(SelectedVoiceVM.Value);
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
			SelectedVoiceVM.Value = VoiceSelector.EntitiesCollection.FirstOrDefault((CharGenVoiceItemVM item) => item.Voice == pregenVoice);
			return;
		}
		BlueprintCharGenRoot charGenRoot = BlueprintRoot.Instance.CharGenRoot;
		int value2 = ((DollGender == Gender.Male) ? charGenRoot.MaleVoiceDefaultId : charGenRoot.FemaleVoiceDefaultId);
		value2 = Math.Clamp(value2, 0, VoiceSelector.EntitiesCollection.Count - 1);
		SelectedVoiceVM.Value = VoiceSelector.EntitiesCollection.ElementAt(value2);
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
		SelectedVoiceVM.Value = charGenVoiceItemVM;
		m_SelectionStateVoice?.SelectVoice(charGenVoiceItemVM.Voice);
		Barks.Value = charGenVoiceItemVM.Barks;
		Barks.Value.PlayPreview();
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
}
