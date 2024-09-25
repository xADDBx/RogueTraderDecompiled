using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("922dffbccac37b744b07eeca32a3ae84")]
public class BlueprintMythicInfo : BlueprintScriptableObject
{
	[SerializeField]
	private Mythic _mythic;

	[SerializeField]
	private BlueprintEtudeReference _etudeReference;

	[SerializeField]
	private LocalizedString _mythicName;

	public Mythic Mythic => _mythic;

	public string EtudeGuid => _etudeReference?.Guid;

	public bool IsSatisfied
	{
		get
		{
			if (_etudeReference == null)
			{
				return false;
			}
			BlueprintEtude blueprintEtude = _etudeReference.Get();
			if (blueprintEtude == null)
			{
				return false;
			}
			return Game.Instance.Player.EtudesSystem.Etudes.Get(blueprintEtude)?.IsPlaying ?? false;
		}
	}

	public bool HasLinkedEtude
	{
		get
		{
			if (_etudeReference != null)
			{
				return !_etudeReference.IsEmpty();
			}
			return false;
		}
	}

	public string ApplyTextDecorator(string text)
	{
		return $"{text} [{_mythicName}]";
	}
}
