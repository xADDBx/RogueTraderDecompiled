using System;
using Kingmaker.Code.UI.MVVM.View.Other;
using Owlcat.Runtime.UI.Controls.Button;

namespace Kingmaker.Code.UI.MVVM.View.Transition.Common;

[Serializable]
public class PointOnMap
{
	public string Comment;

	public CanvasTransformSettings LightBeamPointSettings;

	public OwlcatMultiButton PointButton;
}
