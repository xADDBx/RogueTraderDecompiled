using System;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class MapObjectFxSettings
{
	[ValidateNotNull]
	public GameObject FxPrefab;

	public bool StartActive = true;

	public Transform FxRoot;
}
