using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound;
using Owlcat.Runtime.Core.Registry;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("168ca2fb1b284714caf2ac02c537a76c")]
public class EtudeBracketAudioObjects : EtudeBracketTrigger, IAudioSceneHandler, ISubscriber, IHashable
{
	private class SelectObjectAttribute : PropertyAttribute
	{
	}

	[SelectObject]
	public string ConnectedObjectName;

	public override bool RequireLinkedArea => true;

	protected override void OnExit()
	{
		Toggle(on: false);
	}

	protected override void OnEnter()
	{
		Toggle(on: true);
	}

	private void Toggle(bool on)
	{
		EtudeConnectedAudio etudeConnectedAudio = ObjectRegistry<EtudeConnectedAudio>.Instance.FirstOrDefault((EtudeConnectedAudio o) => o.name == ConnectedObjectName);
		if (etudeConnectedAudio != null)
		{
			etudeConnectedAudio.Toggle(on);
		}
	}

	protected override void OnResume()
	{
		OnEnter();
	}

	public void OnAudioReloaded()
	{
		OnEnter();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
