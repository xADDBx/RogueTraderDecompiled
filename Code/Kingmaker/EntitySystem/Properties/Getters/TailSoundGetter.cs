using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Sound;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("3a972b2e74412bf46afa82ed77b52284")]
public class TailSoundGetter : PropertyGetter
{
	protected override string GetInnerCaption()
	{
		return "Current zone tail sound";
	}

	protected override int GetBaseValue()
	{
		foreach (AudioZoneTriggerableSwitch item in AudioZoneTriggerableSwitch.AudioZone)
		{
			Bounds bounds = new Bounds(item.Bounds.center + item.transform.position, item.Bounds.size);
			if (bounds.Contains(base.CurrentEntity.Position))
			{
				return (int)item.Switch.ValueHash;
			}
		}
		return 0;
	}
}
