using System;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[Serializable]
public class SoundSettings
{
	[SerializeField]
	[AkEventReference]
	private string m_Event;

	[SerializeField]
	[Range(0f, 100f)]
	private float m_Gain = 100f;

	[SerializeField]
	[Range(0f, 100f)]
	private float m_Pitch = 50f;

	[SerializeField]
	private float m_Delay;

	[SerializeReference]
	public GameSyncSettings[] m_GameSyncs;

	public string Event => m_Event;

	public float Gain => m_Gain;

	public float Pitch => m_Pitch;

	public float Delay => m_Delay;

	public GameSyncSettings[] GameSyncs => m_GameSyncs;
}
