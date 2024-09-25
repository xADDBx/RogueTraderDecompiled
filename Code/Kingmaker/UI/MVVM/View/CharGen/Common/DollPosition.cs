using System;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common;

[Serializable]
internal struct DollPosition
{
	public CharacterDollPosition Position;

	public RectTransform Transform;
}
