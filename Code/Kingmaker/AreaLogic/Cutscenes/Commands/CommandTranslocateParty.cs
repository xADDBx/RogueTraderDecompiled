using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Unity.Collections;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("d1b13a2a9ce49e645a228792a62998f5")]
public class CommandTranslocateParty : CommandBase
{
	[Serializable]
	public class Target
	{
		[AllowedEntityType(typeof(LocatorView))]
		[ValidateNotEmpty]
		public EntityReference Entity;

		[SerializeReference]
		public AbstractUnitEvaluator Unit;
	}

	public delegate void DoDistributeDelegate(BaseUnitEntity character, EntityReference targetRef, int characterIndex);

	private static readonly LogChannel Logger = PFLog.Cutscene;

	[SerializeField]
	private Player.CharactersList m_UnitsList;

	[SerializeField]
	protected bool FollowLocatorOrientation;

	[SerializeReference]
	public AbstractUnitEvaluator[] ExceptThese;

	[HideIf("True")]
	public EntityReference[] Targets;

	[ValidateNotEmpty]
	[ValidateNoNullEntries]
	public Target[] TargetsV2;

	private bool True => true;

	public override void OnEnable()
	{
		base.OnEnable();
		if (TargetsV2.Empty() && !Targets.Empty())
		{
			TargetsV2 = Targets?.Select((EntityReference x) => new Target
			{
				Entity = x
			}).ToArray();
			Targets = null;
		}
	}

	public static void Distribute(IEnumerable<BaseUnitEntity> srcCharacters, Target[] srcTargets, DoDistributeDelegate doDistributeDelegate)
	{
		List<BaseUnitEntity> characters = srcCharacters.ToList();
		var list = srcTargets.Select(delegate(Target x)
		{
			EntityReference entity = x.Entity;
			AbstractUnitEvaluator unit = x.Unit;
			AbstractUnitEntity value;
			return new
			{
				Entity = entity,
				Unit = ((unit != null && unit.TryGetValue(out value)) ? value : null)
			};
		}).ToList();
		int ci;
		for (ci = 0; ci < characters.Count; ci++)
		{
			int num = list.FindIndex(x => characters[ci] == x.Unit);
			if (num != -1)
			{
				doDistributeDelegate(characters[ci], list[num].Entity, ci);
				characters[ci] = null;
				list.RemoveAtSwapBack(num);
			}
		}
		if (list.Empty())
		{
			list = srcTargets.Select((Target x) => new
			{
				Entity = x.Entity,
				Unit = (AbstractUnitEntity)null
			}).ToList();
		}
		int i = 0;
		int num2 = 0;
		for (; i < characters.Count; i++)
		{
			if (characters[i] != null)
			{
				doDistributeDelegate(characters[i], list[num2].Entity, i);
				num2 = (num2 + 1) % list.Count;
			}
		}
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		BlueprintAreaPart part = null;
		Distribute(from x in Game.Instance.Player.GetCharactersList(m_UnitsList)
			where !ElementExtendAsObject.Valid(ExceptThese).Any((AbstractUnitEvaluator y) => y != null && y.TryGetValue(out var value) && x == value)
			select x, TargetsV2, Translocate);
		if (part != null && part != Game.Instance.CurrentlyLoadedAreaPart)
		{
			Logger.Warning($"{this}: translocating party from {Game.Instance.CurrentlyLoadedAreaPart} to {part}. Starting fix process.");
			LoadingProcess.Instance.StartLoadingProcess(Game.Instance.SceneLoader.SwitchToAreaPartCoroutine(part));
		}
		void Translocate(BaseUnitEntity character, EntityReference targetRef, int characterIndex)
		{
			IEntityViewBase entityViewBase = targetRef.FindView();
			if (entityViewBase == null)
			{
				Logger.Error($"{this}: Can't find locator {targetRef} for teleport unit");
			}
			else
			{
				if (characterIndex < TargetsV2.Length && character.IsCustomCompanion() && !character.IsViewActive)
				{
					character.IsInGame = true;
					character.Parts.GetOptional<UnitPartCompanion>()?.SetState(CompanionState.InParty);
				}
				character.Position = entityViewBase.ViewTransform.position;
				if (FollowLocatorOrientation)
				{
					character.MovementAgent.Stop();
					character.SetOrientation(entityViewBase.ViewTransform.rotation.eulerAngles.y);
				}
				part = AreaService.FindMechanicBoundsContainsPoint(character.Position);
			}
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	protected override void OnStop(CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return $"Teleport party ({m_UnitsList})";
	}
}
