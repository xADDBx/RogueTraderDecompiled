using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Highlighting;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class HighlightAnimation : MonoBehaviour
{
	public MultiHighlighter.HighlightSource Animation;

	private MultiHighlighter m_Player;

	public void Play(MultiHighlighter player)
	{
		if ((bool)m_Player)
		{
			m_Player.Stop(Animation);
		}
		player.Play(Animation);
		m_Player = player;
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		if ((bool)m_Player)
		{
			m_Player.Stop(Animation);
		}
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		if ((bool)m_Player)
		{
			m_Player.Play(Animation);
		}
	}
}
