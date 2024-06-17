using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.Controls.SelectableState;

[Serializable]
public class OwlcatMultiLayer
{
	public string LayerName;

	public List<OwlcatSelectableLayerPart> Parts;

	public void AddPart()
	{
		Parts.Add(new OwlcatSelectableLayerPart
		{
			Transition = OwlcatTransition.ColorTint,
			Colors = ColorBlock.defaultColorBlock
		});
	}
}
