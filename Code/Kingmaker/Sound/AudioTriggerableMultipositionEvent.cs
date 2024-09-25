using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Sound;

internal class AudioTriggerableMultipositionEvent : AudioTriggerableEvent
{
	[SerializeField]
	private List<AkAmbientLargeModePositioner> m_Positioners = new List<AkAmbientLargeModePositioner>();

	private bool m_PositionsUpdated;

	public override void OnTrigger()
	{
		if (!m_PositionsUpdated)
		{
			List<AkAmbientLargeModePositioner> list = m_Positioners.Valid().Distinct().ToList();
			AkPositionArray akPositionArray = new AkPositionArray((uint)list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				akPositionArray.Add(list[i].Position, list[i].Forward, list[i].Up);
			}
			AkSoundEngine.SetMultiplePositions(base.gameObject, akPositionArray, (ushort)akPositionArray.Count, AkMultiPositionType.MultiPositionType_MultiSources);
			m_PositionsUpdated = true;
		}
		base.OnTrigger();
	}
}
