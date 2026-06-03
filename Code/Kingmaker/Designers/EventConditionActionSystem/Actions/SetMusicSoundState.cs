using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Sound;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Group("Actions")]
[AllowMultipleComponents]
[TypeId("00ca6afae3a5475aad9aebf040370af8")]
public class SetMusicSoundState : GameAction
{
	[SerializeField]
	private AkStateReference m_State;

	[SerializeField]
	private bool m_ProlongTillNextCombat;

	[SerializeField]
	[Tooltip("Игнорировать стейт выхода из боя, если мы переключаемся на, например, катсцену")]
	[KDB("Эта галка позволяет обойти ситуации, когда при переходе между фазами боя мы хотим показать катсцену.Тогда игра считает, что Combat стейт сменился на Cutscene, и мы вышли из боя. Из-за этого может начать играть музыка эксплоринга, прежде чем включится музыка второй фазы. Юз кейс - Фазы боссфайта с Тразином")]
	private bool m_IgnoreCombatExit;

	protected override void RunAction()
	{
		SoundState.Instance.MusicStateHandler.SetMusicStoryType(m_State, m_ProlongTillNextCombat, m_IgnoreCombatExit);
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
