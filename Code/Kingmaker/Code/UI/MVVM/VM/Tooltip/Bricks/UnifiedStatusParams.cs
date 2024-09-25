using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

[Serializable]
public class UnifiedStatusParams
{
	public UnifiedStatus Status;

	public Sprite Icon;

	public bool ShowFrame;

	[Header("Colors")]
	public Color32 IconColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color32 TextColor;

	public Color32 FrameColor;
}
