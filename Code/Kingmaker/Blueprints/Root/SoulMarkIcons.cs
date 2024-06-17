using System;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class SoulMarkIcons
{
	public Sprite Corruption;

	public Sprite Faith;

	public Sprite Hope;

	public Sprite GetIconByDirection(SoulMarkDirection direction)
	{
		return direction switch
		{
			SoulMarkDirection.None => null, 
			SoulMarkDirection.Corruption => Corruption, 
			SoulMarkDirection.Faith => Faith, 
			SoulMarkDirection.Hope => Hope, 
			SoulMarkDirection.Reason => null, 
			_ => null, 
		};
	}
}
