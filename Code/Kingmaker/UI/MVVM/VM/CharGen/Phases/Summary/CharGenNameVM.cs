using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Summary;

public class CharGenNameVM : CharInfoComponentWithLevelUpVM
{
	public readonly ReactiveProperty<string> UnitName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<CharGenChangeNameMessageBoxVM> MessageBoxVM = new ReactiveProperty<CharGenChangeNameMessageBoxVM>();

	private readonly Func<string> m_GetRandomName;

	private readonly Action<string> m_OnSetName;

	public CharGenNameVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit, IReadOnlyReactiveProperty<LevelUpManager> levelUpManager, Func<string> getRandomName, Action<string> onSetName)
		: base(unit, levelUpManager)
	{
		UnitName.Value = PreviewUnit.Value.CharacterName;
		m_GetRandomName = getRandomName;
		m_OnSetName = onSetName;
	}

	public void ShowChangeNameMessageBox(Action onComplete = null)
	{
		DisposeMessageBox();
		MessageBoxVM.Value = new CharGenChangeNameMessageBoxVM(UIStrings.Instance.CharGen.ChooseName, UIStrings.Instance.SettingsUI.DialogApply, delegate(string text)
		{
			text = text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				SetNameAndNotify(text);
			}
			onComplete?.Invoke();
		}, delegate
		{
		}, UnitName.Value, GetRandomName, DisposeMessageBox);
	}

	private void DisposeMessageBox()
	{
		DisposeAndRemove(MessageBoxVM);
	}

	public string GetRandomName()
	{
		return m_GetRandomName?.Invoke();
	}

	public void SetName(string characterName)
	{
		UnitName.Value = characterName;
	}

	public void SetNameAndNotify(string characterName)
	{
		SetName(characterName);
		m_OnSetName?.Invoke(characterName);
	}
}
