using Kingmaker.Blueprints.Root;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

[ExecuteInEditMode]
public class DialogColorsConfig : MonoBehaviour
{
	[SerializeField]
	private DialogColors m_DialogColors;

	private readonly Color32 m_DefaultColor = new Color32(0, 0, 0, 0);

	public DialogColors DialogColors => m_DialogColors;

	private void Awake()
	{
	}
}
