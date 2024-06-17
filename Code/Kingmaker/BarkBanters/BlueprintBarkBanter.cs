using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.BarkBanters;

[Serializable]
[TypeId("0aa1306159a0eb64388bdd80d67b4ac9")]
public class BlueprintBarkBanter : BlueprintScriptableObject, IWeighted
{
	[Serializable]
	public class BanterResponseEntry : IWeighted
	{
		[SerializeField]
		[FormerlySerializedAs("Unit")]
		private BlueprintUnitReference m_Unit;

		[SerializeField]
		private float m_Weight = 1f;

		[NotNull]
		public LocalizedString Response;

		[SerializeField]
		public ConditionsChecker ResponseCondition;

		public float Weight => m_Weight;

		public BlueprintUnit Unit
		{
			get
			{
				return m_Unit?.Get();
			}
			set
			{
				m_Unit = value.ToReference<BlueprintUnitReference>();
			}
		}

		public Entity Speaker => FindUnit(Unit);
	}

	[SerializeField]
	[FormerlySerializedAs("Unit")]
	private BlueprintUnitReference m_Unit;

	[NotNull]
	public BanterConditions Conditions = new BanterConditions();

	[SerializeField]
	private float m_Weight = 1f;

	[NotNull]
	public LocalizedString[] FirstPhrase = Array.Empty<LocalizedString>();

	public StrategySelectingResponse StrategySelectingResponse;

	[NotNull]
	public BanterResponseEntry[] Responses = Array.Empty<BanterResponseEntry>();

	public BlueprintUnit Unit
	{
		get
		{
			return m_Unit?.Get();
		}
		set
		{
			m_Unit = value.ToReference<BlueprintUnitReference>();
		}
	}

	public Entity Speaker => FindUnit(Unit);

	public float Weight => m_Weight;

	public bool CanBePlayed()
	{
		if (Unit == null)
		{
			return true;
		}
		if (Speaker == null)
		{
			return false;
		}
		if (Conditions.ResponseRequired && SelectResponse() == null)
		{
			return false;
		}
		Player player = Game.Instance.Player;
		if (Conditions.Unique && player.PlayedBanters.Contains(this))
		{
			return false;
		}
		int chapter = Game.Instance.Player.Chapter;
		if (Conditions.MinChapter > 0 && Conditions.MinChapter > chapter)
		{
			return false;
		}
		if (Conditions.MaxChapter > 0 && Conditions.MaxChapter < chapter)
		{
			return false;
		}
		if (!Conditions.ExtraConditions.Check())
		{
			return false;
		}
		return true;
	}

	[CanBeNull]
	public BarkBanterPlayer CreatePlayer()
	{
		if (Unit == null)
		{
			return new BarkBanterPlayer();
		}
		Entity speaker = Speaker;
		if (speaker == null)
		{
			return null;
		}
		LocalizedString localizedString = FirstPhrase.Random(PFStatefulRandom.Bark);
		if (localizedString == null)
		{
			return null;
		}
		BarkBanterPlayer barkBanterPlayer = new BarkBanterPlayer();
		barkBanterPlayer.AddEntry(speaker, localizedString);
		BanterResponseEntry banterResponseEntry = SelectResponse();
		Entity entity = banterResponseEntry?.Speaker;
		if (entity != null)
		{
			barkBanterPlayer.AddEntry(entity, banterResponseEntry.Response);
		}
		AstropathBriefComponent astropathBriefComponent = this.GetComponents<AstropathBriefComponent>().FirstOrDefault();
		if (astropathBriefComponent != null)
		{
			barkBanterPlayer.AddBrief(astropathBriefComponent.AstropathBrief);
		}
		return barkBanterPlayer;
	}

	[CanBeNull]
	private static BaseUnitEntity FindUnit(BlueprintUnit unitBp)
	{
		if (unitBp == null)
		{
			return null;
		}
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.Party.FirstOrDefault((BaseUnitEntity u) => u.Blueprint == unitBp);
		if (baseUnitEntity == null || !baseUnitEntity.LifeState.IsConscious)
		{
			return null;
		}
		return baseUnitEntity;
	}

	private static bool CheckAnswerConditions(ConditionsChecker condition)
	{
		return condition?.Check() ?? true;
	}

	[CanBeNull]
	public BanterResponseEntry SelectResponse()
	{
		IEnumerable<BanterResponseEntry> enumerable = Responses.Where((BanterResponseEntry r) => r.Speaker != null && CheckAnswerConditions(r.ResponseCondition));
		return StrategySelectingResponse switch
		{
			StrategySelectingResponse.Random => enumerable.Random(PFStatefulRandom.Bark), 
			StrategySelectingResponse.MostWeighted => enumerable.MostWeighted(), 
			_ => null, 
		};
	}

	[BlueprintButton(Name = "Debug Play")]
	public async void DebugPlay()
	{
		BarkBanterPlayer player = CreatePlayer();
		if (player != null)
		{
			while (!player.Finished)
			{
				player.Tick();
				await Task.Yield();
			}
		}
	}
}
