using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Fade;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("c70d3959f8ce9d145a883fab41d59e62")]
public class CommandFadeout : CommandFadeoutBase
{
	[SerializeField]
	[ConditionalHide("m_Continuous")]
	private float m_Lifetime = 1f;

	protected override float Lifetime => m_Lifetime;

	protected override void Fadeout(bool fade)
	{
		FadeCanvas.Fadeout(fade);
	}

	public override string GetCaption()
	{
		return "Fade screen";
	}
}
