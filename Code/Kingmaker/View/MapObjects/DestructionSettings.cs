using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Visual.Effects.VAT;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class DestructionSettings
{
	public int DC;

	public int ApproachRadius = 4;

	[HideInInspector]
	public VATPlayer DestructionPlayer;

	[HideInInspector]
	public List<GameObject> NormalViews;

	[HideInInspector]
	public List<GameObject> DestroyedViews;

	[AkEventReference]
	public string DestructionStartSound;

	[AkEventReference]
	public string DestructionStopSound;

	[AkEventReference]
	public string SuccessSound;

	[AkEventReference]
	public string FailedSound;

	public ActionsReference OnDestructionActions;
}
