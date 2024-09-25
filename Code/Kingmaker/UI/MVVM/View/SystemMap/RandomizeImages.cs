using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SystemMap;

[Serializable]
public class RandomizeImages
{
	public Image LeftTopImage;

	public Image LeftBottomImage;

	public Image RightBottomImage;

	public Sprite[] LeftTopSprites;

	public Sprite[] LeftBottomSprites;

	public Sprite[] RightBottomSprites;
}
