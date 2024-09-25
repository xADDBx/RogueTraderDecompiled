using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Tutorial;

[TypeId("878c7bf2cac642b2b8f377507e1bddd5")]
public class BlueprintTutorialPage : BlueprintScriptableObject
{
	public TutorialMessageType Type;

	[CanBeNull]
	public Sprite Image;

	[NotNull]
	public LocalizedString Caption = new LocalizedString();

	[NotNull]
	public LocalizedString Message = new LocalizedString();

	[NotNull]
	public ConditionsChecker Conditions = new ConditionsChecker();

	[NotNull]
	public ActionList CloseActions = new ActionList();
}
