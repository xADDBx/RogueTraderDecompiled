using System;
using Kingmaker.UI.Common.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SystemMap;

[Serializable]
public class NegativeEffects
{
	public FadeAnimator RedBackground;

	public Image ResourceIcon;

	public Image ResourceSeparator;

	public TextMeshProUGUI ResourceCount;

	public TextMeshProUGUI ResourceCountAdditional;

	public Color RedIconColor;

	public Color RedSeparatorColor;

	public Color RedCountColor;

	public Color RedCountAdditionalColor;

	public Color NormalIconColor;

	public Color NormalSeparatorColor;

	public Color NormalCountColor;

	public Color NormalCountAdditionalColor;
}
