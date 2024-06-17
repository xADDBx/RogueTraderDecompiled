using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutscenePlayerDataScope : ContextData<CutscenePlayerDataScope>
{
	private ICutscenePlayerData m_Player;

	[CanBeNull]
	public new static ICutscenePlayerData Current => ContextData<CutscenePlayerDataScope>.Current?.m_Player;

	public CutscenePlayerDataScope Setup(ICutscenePlayerData player)
	{
		m_Player = player;
		return this;
	}

	protected override void Reset()
	{
		m_Player = null;
	}
}
