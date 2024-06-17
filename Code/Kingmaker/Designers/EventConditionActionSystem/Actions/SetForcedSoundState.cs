using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Sound;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SetForcedSoundState")]
[AllowMultipleComponents]
[TypeId("00ca6afae3a5475aad9aebf040370af8")]
public class SetForcedSoundState : GameAction
{
	[SerializeField]
	private AkStateReference m_State;

	[SerializeField]
	private bool m_ProlongTillNextCombat;

	public override void RunAction()
	{
		SoundState.Instance.MusicStateHandler.SetMusicStoryType(m_State, m_ProlongTillNextCombat);
	}

	public override string GetCaption()
	{
		if (m_State == null)
		{
			return "Sound Forced State (unknown)";
		}
		return $"Sound Forced State ({m_State.Group}\\{m_State.Value})";
	}
}
