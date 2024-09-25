using System;
using System.Collections.Generic;
using Code.GameCore.Blueprints;
using Code.GameCore.ElementsSystem.Actions;
using Code.GameCore.ElementsSystem.Debug;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.NodeLink;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Progression.Prerequisites;
using Kingmaker.Visual.Particles.Blueprints;
using RogueTrader.Editor.CameraRecorder;

public static class ClassesWithGuid
{
	public static readonly List<(Type Type, string Guid)> Classes = new List<(Type, string)>
	{
		(typeof(BlueprintEtudeConflictingGroup), "8e8785a8c11046ed9bce13c7c61e51ed"),
		(typeof(BlueprintAbilityAreaEffectGroup), "780ace165a134668b092b98a8a9d98c0"),
		(typeof(BlueprintAbilityGroup), "84a976c8e48e6274e8367073fad4a237"),
		(typeof(BlueprintBroken), "50180856b8ea4cf6965e53bb91472358"),
		(typeof(BlueprintComponent), "0956112276c2f8340b645b372472e09a"),
		(typeof(BlueprintComponentList), "ca9d3271950638d4e955338f6381dbe7"),
		(typeof(BlueprintFaction), "9c187edec85a6b845a4998e7cf445685"),
		(typeof(BlueprintItemsStash), "5e67da75e2474764a5fde3375352ac8f"),
		(typeof(BlueprintPet), "923b2d325a4900b49af56a5ac18ceceb"),
		(typeof(BlueprintProjectileTrajectory), "55a39726d0cce8a45877d1ffb0c3e5e6"),
		(typeof(BlueprintScriptableObject), "3ec4f91d40b87d34197f44f40a969d92"),
		(typeof(BlueprintSummonPool), "fc91c2d06c5f09a419eeca7a14709271"),
		(typeof(BlueprintTrapSettings), "de4e0b0e79fa417a9a142251950680f2"),
		(typeof(BlueprintUnlockableFlag), "06e9a18b1f15bcf41b3a0ce1a2a0dfdd"),
		(typeof(BlueprintVendorFaction), "535b405b948543eca5e301737aece91b"),
		(typeof(ComponentsList), "3519a736d2aa5cf46a07a5b5b8abf4a1"),
		(typeof(BlueprintNodeLinkRoot), "05f446aeb54b469c8d7f72df6734d364"),
		(typeof(PrototypeableObjectBase), "220d3836215f96e4596d705f9c303c7d"),
		(typeof(BlueprintActionCameraSettings), "303f32c8f2d7514428b142628c35df0f"),
		(typeof(BlueprintCameraFollowSettings), "fdd33cf2c5394802ae975acfd6de374d"),
		(typeof(BlueprintCameraSettings), "6c261c9ba23740c58dcd66c6353ac59d"),
		(typeof(CameraRoot), "c502cb732e5f4cd0ad2ce2c24b48c82c"),
		(typeof(SimpleBlueprint), "0b3cc43201601904bb7eb333c9b646ff"),
		(typeof(CameraRecorderData), "084826bccf334e3a892863750a393dc1"),
		(typeof(ActionsHolder), "63bb90208198450095c55997ebc6ae0a"),
		(typeof(BlueprintActionList), "a80970ca06938034d8c58815e415690e"),
		(typeof(ConditionsHolder), "b9ea3359b1204b798a61750d6cb4e723"),
		(typeof(ThrowExceptionAction), "f225557a3df442c0b7f3f8ca4d6fb3b6"),
		(typeof(ThrowExceptionCondition), "702fcb2beb1849bcb8b878f2dd02e476"),
		(typeof(Conditional), "52d8973f2e470e14c97b74209680491a"),
		(typeof(OrAndLogic), "1d392c8d9feed78408fbcb18f9468fb9"),
		(typeof(RandomAction), "fe04f935f78d4ba4c805faf9a4be38a3"),
		(typeof(PrerequisiteComposite), "3ebda9716ea24139a3a4d3f7c49dc90d"),
		(typeof(PrerequisiteStat), "3acf9db791c042a4b9588426022193a8"),
		(typeof(BlueprintFxLocatorGroup), "2c0480568ae94c9ba720eb332c778f3e")
	};
}
