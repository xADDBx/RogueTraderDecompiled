using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit;

[Serializable]
public struct UnitOvertipVisibilitySettings
{
	public UnitOvertipVisibility UnitOvertipVisibility;

	public float Alpha;

	public float Scale;

	public float YPosition;

	public Vector2 Size;

	public List<CanvasGroup> CanvasGroups;
}
