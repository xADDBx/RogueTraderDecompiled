using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Scenes;

internal class FakeLoadingScreen : MonoBehaviour
{
	public Texture Texture;

	private void OnGUI()
	{
		GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture);
	}
}
