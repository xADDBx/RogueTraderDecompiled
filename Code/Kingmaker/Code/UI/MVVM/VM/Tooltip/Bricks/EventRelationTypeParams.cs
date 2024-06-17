using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

[Serializable]
public class EventRelationTypeParams
{
	public EventRelationType Type;

	public Sprite Icon;

	[Header("Colors")]
	public Color32 TypeColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
}
