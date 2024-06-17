using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.TextureSelector;

public class TextureSelectorColorItemView : TextureSelectorItemView
{
	protected override Sprite GetSprite(Texture2D texture)
	{
		Rect rect = new Rect(texture.width / 2, 0f, 1f, texture.height);
		return Sprite.Create(texture, rect, Vector2.zero);
	}
}
