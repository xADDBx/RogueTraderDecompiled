using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("8a82e6db29f72c04fb28dc6112f7cef9")]
public class FadeOutFxAction : GameAction
{
	[SerializeReference]
	public TransformEvaluator Target;

	[Tooltip("If false, the FX object will remain in the scene after fading out but with opacity set to 0")]
	public bool DestroyFxObject = true;

	[Tooltip("If true, the FX will fade in instead of fade out")]
	public bool FadeIn;

	public override string GetCaption()
	{
		string text = (FadeIn ? "Fade In FX" : "Fade Out FX");
		if (!FadeIn && !DestroyFxObject)
		{
			text += " (Keep Object)";
		}
		return text;
	}

	protected override void RunAction()
	{
		if (Target == null)
		{
			return;
		}
		FxFadeOut[] componentsInChildren = Target.GetValue().gameObject.GetComponentsInChildren<FxFadeOut>(includeInactive: true);
		foreach (FxFadeOut fxFadeOut in componentsInChildren)
		{
			if (FadeIn)
			{
				fxFadeOut.StartFadeInFunc();
				continue;
			}
			fxFadeOut.DestroyFxObject = DestroyFxObject;
			fxFadeOut.StartForceFadeOut = true;
		}
	}
}
