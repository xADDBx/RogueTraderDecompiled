using System;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SystemMap;

[Serializable]
public class CircleArtRotation
{
	public RectTransform CircleArt;

	public float CircleArtDefaultScale = 1f;

	public float CircleArtMoveScale = 0.97f;

	public float CircleArtMinRotation = 5f;

	public float CircleArtMaxRotation = 10f;

	public float CircleArtAnimationDuration = 2f;

	public float CircleArtCloseAnimationDuration = 1f;
}
