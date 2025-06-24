using System;
using System.Collections.Generic;
using Kingmaker.AreaLogic.Cutscenes.Commands.Camera;
using Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;
using Kingmaker.AreaLogic.SceneControllables;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.EntitySystem;
using Kingmaker.UI.Selection.UnitMark;
using Kingmaker.View;
using Kingmaker.View.Equipment;
using Kingmaker.View.GlobalMap;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual;
using Kingmaker.Visual.CharactersRigidbody;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Critters;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.ForcedCulling;

public static class Updateables
{
	public static readonly List<Type> IUpdateables = new List<Type>
	{
		typeof(DirectorAdapter),
		typeof(ControllableAnimator),
		typeof(PlatformObjectEntity),
		typeof(SubtitleBarkHandle),
		typeof(BarkHandle),
		typeof(CutsceneArtController),
		typeof(EquipmentOffsets),
		typeof(FogOfWarRevealerSettings),
		typeof(StarSystemStarshipView),
		typeof(InteractionDoorPart),
		typeof(FogOfWarRevealerTrigger),
		typeof(Billboard),
		typeof(HumanoidRagdollManager),
		typeof(RigidbodyCreatureController),
		typeof(Character),
		typeof(StatueCharacterNew),
		typeof(Bird),
		typeof(Rabbit),
		typeof(StandardMaterialController),
		typeof(ForcedCullingService),
		typeof(FxFadeOut),
		typeof(ParticlesMaterialController),
		typeof(UnitFxVisibilityManager),
		typeof(UnitMultiHighlight)
	};

	public static readonly List<Type> ILateUpdateables = new List<Type>
	{
		typeof(BaseSurfaceUnitMark),
		typeof(BaseUnitMark),
		typeof(CharacterUnitMark),
		typeof(EnemyStarshipUnitMark),
		typeof(EnemyUnitMark),
		typeof(NpcUnitMark),
		typeof(StarshipUnitMark),
		typeof(EntityFader),
		typeof(AbstractUnitEntityView.LateUpdateDriver),
		typeof(SkeletonUpdateService)
	};
}
