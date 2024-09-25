using System;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Party.PC;

[Serializable]
public class DamageColorSet
{
	public Color NormalColor = Color.green;

	public Color DamageColor = Color.yellow;

	public Color NearDeathColor = Color.red;
}
