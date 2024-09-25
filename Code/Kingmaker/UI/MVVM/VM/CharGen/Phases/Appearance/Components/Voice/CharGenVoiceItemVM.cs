using Kingmaker.Blueprints;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Voice;

public class CharGenVoiceItemVM : SelectionGroupEntityVM
{
	public readonly BlueprintUnitAsksList Voice;

	public readonly string DisplayName;

	public readonly bool IsEmptyVoice;

	public readonly UnitAsksComponent Barks;

	public CharGenVoiceItemVM(BlueprintUnitAsksList voice)
		: base(allowSwitchOff: false)
	{
		Voice = voice;
		DisplayName = voice.DisplayName;
		Barks = voice.GetComponent<UnitAsksComponent>();
		IsEmptyVoice = string.IsNullOrEmpty(Barks?.PreviewSound);
	}

	protected override void DoSelectMe()
	{
	}
}
