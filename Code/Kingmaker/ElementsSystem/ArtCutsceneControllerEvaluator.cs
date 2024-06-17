using Kingmaker.AreaLogic.Cutscenes.Commands.Camera;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.View;
using Owlcat.QA.Validation;

namespace Kingmaker.ElementsSystem;

[TypeId("e81fafe622455b54d96caa4efeff0898")]
public class ArtCutsceneControllerEvaluator : GenericEvaluator<CutsceneArtController>
{
	[AllowedEntityType(typeof(CutsceneArtController))]
	[ValidateNotNull]
	public EntityReference RefOnSceneObject;

	public override string GetCaption()
	{
		return $"CutsceneController from {RefOnSceneObject}";
	}

	protected override CutsceneArtController GetValueInternal()
	{
		EntityViewBase entityViewBase = (EntityViewBase)RefOnSceneObject.FindView();
		if (!(entityViewBase != null))
		{
			return null;
		}
		return entityViewBase.GetComponent<CutsceneArtController>();
	}
}
