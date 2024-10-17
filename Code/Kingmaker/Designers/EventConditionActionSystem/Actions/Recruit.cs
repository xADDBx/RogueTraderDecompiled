using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UI;
using Kingmaker.UI.Selection;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Recruit")]
[AllowMultipleComponents]
[TypeId("b90eea06ce91f564e8793832eea02cef")]
public class Recruit : GameAction
{
	[Serializable]
	public class RecruitData
	{
		[ValidateNotNull]
		[SerializeField]
		[FormerlySerializedAs("CompanionBlueprint")]
		private BlueprintUnitReference m_CompanionBlueprint;

		[SerializeField]
		private BlueprintPortraitReference m_Portrait;

		[Tooltip("Optional. Which unit should be replaced.")]
		[SerializeReference]
		public AbstractUnitEvaluator NPCUnit;

		public bool MustBeInParty;

		[NonSerialized]
		public BaseUnitEntity RecruitedCompanion;

		public BlueprintUnit CompanionBlueprint => m_CompanionBlueprint?.Get();

		public BlueprintPortraitReference Portrait => m_Portrait;
	}

	public RecruitData[] Recruited;

	public bool AddToParty = true;

	public bool MatchPlayerXpExactly;

	public ActionList OnRecruit;

	public ActionList OnRecruitImmediate;

	[Tooltip("Will not open party selection if in party > 6 people. Use only if after that will be unrecruit/detach")]
	public bool DoNotOpenPartySelection;

	protected override void RunAction()
	{
		RecruitData[] recruited = Recruited;
		foreach (RecruitData data in recruited)
		{
			SwitchToCompanion(data);
		}
		Game.Instance.EntitySpawner.Tick();
		if (Game.Instance.Player.Party.Count > 6 && !DoNotOpenPartySelection)
		{
			recruited = Recruited;
			foreach (RecruitData recruitData2 in recruited)
			{
				Game.Instance.Player.RemoveCompanion(recruitData2.RecruitedCompanion, stayInGame: true);
			}
			ShowPartyInterface();
			return;
		}
		List<RecruitData> required = Recruited.Where((RecruitData recruitData) => recruitData.MustBeInParty).ToList();
		EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
		{
			h.HandleSetRequiredUnits(required.Select((RecruitData r) => r.RecruitedCompanion?.Blueprint ?? r.CompanionBlueprint).ToList());
		});
		recruited = Recruited;
		foreach (RecruitData recruitData3 in recruited)
		{
			using (ContextData<RecruitedUnitData>.Request().Setup(recruitData3.RecruitedCompanion))
			{
				OnRecruit.Run();
			}
		}
	}

	private void ShowPartyInterface()
	{
		List<RecruitData> required = Recruited.Where((RecruitData recruitData) => recruitData.MustBeInParty).ToList();
		if (required.Count > 5)
		{
			Element.LogError(this, "Cannot recruit {0} characters in {1}: too many!!", required.Count, this);
			return;
		}
		while (Game.Instance.Player.Party.Count > 6 - required.Count)
		{
			BaseUnitEntity value = Game.Instance.Player.Party.Last((BaseUnitEntity c) => c != GameHelper.GetPlayerCharacter());
			Game.Instance.Player.RemoveCompanion(value, stayInGame: true);
		}
		foreach (RecruitData item in required)
		{
			Game.Instance.Player.AddCompanion(item.RecruitedCompanion);
		}
		EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
		{
			h.HandleSetRequiredUnits(required.Select((RecruitData r) => r.RecruitedCompanion?.Blueprint ?? r.CompanionBlueprint).ToList());
		});
		EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
		{
			h.HandleCall(delegate
			{
				foreach (BaseUnitEntity item2 in Game.Instance.Player.RemoteCompanions.ToTempList())
				{
					item2.IsInGame = false;
					item2.View.SetVisible(visible: false);
				}
				Game.Instance.Player.FixPartyAfterChange();
				UIAccess.SelectionManager.UpdateSelectedUnits();
				RecruitData[] recruited2 = Recruited;
				foreach (RecruitData recruitData3 in recruited2)
				{
					using (ContextData<RecruitedUnitData>.Request().Setup(recruitData3.RecruitedCompanion))
					{
						OnRecruit.Run();
					}
				}
				List<UnitEntityView> views = Game.Instance.Player.Party.Select((BaseUnitEntity character) => character.View).ToTempList();
				(UIAccess.SelectionManager as SelectionManagerPC)?.MultiSelect(views);
			}, delegate
			{
				foreach (BaseUnitEntity item3 in Game.Instance.Player.RemoteCompanions.ToTempList())
				{
					item3.IsInGame = false;
				}
				RecruitData[] recruited = Recruited;
				foreach (RecruitData recruitData2 in recruited)
				{
					using (ContextData<RecruitedUnitData>.Request().Setup(recruitData2.RecruitedCompanion))
					{
						OnRecruit.Run();
					}
				}
			}, isCapital: false, sameFinishActions: false, canCancel: false);
		});
	}

	private void SwitchToCompanion(RecruitData data)
	{
		Player player = Game.Instance.Player;
		BaseUnitEntity baseUnitEntity = player.AllCharacters.FirstOrDefault((BaseUnitEntity u) => u.Blueprint == data.CompanionBlueprint);
		bool flag = baseUnitEntity != null && baseUnitEntity.GetOptional<UnitPartCompanion>()?.State == CompanionState.ExCompanion;
		bool flag2 = baseUnitEntity != null && baseUnitEntity.GetOptional<UnitPartCompanion>()?.State == CompanionState.Remote;
		BaseUnitEntity baseUnitEntity2 = null;
		if (data.NPCUnit != null)
		{
			if (!(data.NPCUnit.GetValue() is BaseUnitEntity baseUnitEntity3))
			{
				string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {data.NPCUnit} is not BaseUnitEntity";
				if (!QAModeExceptionReporter.MaybeShowError(message))
				{
					UberDebug.LogError(message);
				}
			}
			else
			{
				baseUnitEntity2 = baseUnitEntity3;
			}
		}
		if (baseUnitEntity == null || flag)
		{
			baseUnitEntity = GameHelper.RecruitNPC(baseUnitEntity2, data.CompanionBlueprint);
		}
		else
		{
			if (!flag2)
			{
				Element.LogError(this, "Attempted to double-recruit {0} in {1}", data.CompanionBlueprint, this);
				return;
			}
			if (baseUnitEntity2 != null && !baseUnitEntity2.Faction.IsPlayer)
			{
				Game.Instance.EntityDestroyer.Destroy(baseUnitEntity2);
				baseUnitEntity.Position = baseUnitEntity2.Position;
				baseUnitEntity.SetOrientation(baseUnitEntity2.Orientation);
			}
			baseUnitEntity.IsInGame = true;
			player.AddCompanion(baseUnitEntity);
		}
		baseUnitEntity.CombatGroup.Id = "<directly-controllable-unit>";
		if (data.Portrait?.Get() != null)
		{
			baseUnitEntity.UISettings.SetPortrait(data.Portrait);
		}
		if (MatchPlayerXpExactly)
		{
			int experience = Game.Instance.Player.MainCharacterEntity.ToBaseUnitEntity().Progression.Experience;
			baseUnitEntity.Progression.AdvanceExperienceTo(experience, log: false);
		}
		data.RecruitedCompanion = baseUnitEntity;
		using (ContextData<RecruitedUnitData>.Request().Setup(data.RecruitedCompanion))
		{
			OnRecruitImmediate.Run();
		}
		if (data.RecruitedCompanion.Master != null)
		{
			data.RecruitedCompanion.IsInGame = data.RecruitedCompanion.Master.IsInGame;
			using (ContextData<RecruitedUnitData>.Request().Setup(data.RecruitedCompanion))
			{
				OnRecruit.Run();
				return;
			}
		}
		if (!AddToParty)
		{
			player.RemoveCompanion(data.RecruitedCompanion);
		}
	}

	public override string GetCaption()
	{
		RecruitData[] recruited = Recruited;
		if (recruited != null && recruited.Length == 1)
		{
			return $"Recruit ({Recruited[0].CompanionBlueprint})";
		}
		return $"Recruit {Recruited?.Length} people";
	}
}
