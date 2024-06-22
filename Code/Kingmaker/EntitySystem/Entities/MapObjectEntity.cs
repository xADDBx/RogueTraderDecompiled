using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[JsonObject(IsReference = true, MemberSerialization = MemberSerialization.OptIn)]
public class MapObjectEntity : MechanicEntity<BlueprintMechanicEntityFact>, IMapObjectEntity, IMechanicEntity, IEntity, IDisposable, IHashable
{
	[JsonProperty]
	public Dictionary<UnitReference, int> LastAwarenessRollRank = new Dictionary<UnitReference, int>();

	[JsonProperty]
	public bool IsAwarenessCheckPassed { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int? AwarenessCheckDC { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public SkillCheckDifficulty? AwarenessCheckDifficulty { get; set; }

	[JsonProperty]
	public bool WasHighlightedOnReveal { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	public MapObjectViewSettings ViewSettings { get; set; }

	public IEnumerable<InteractionPart> Interactions => Parts.GetAll<InteractionPart>();

	public override bool IsSuppressible => true;

	public override bool IsAffectedByFogOfWar => true;

	public override bool AddToGrid => true;

	public new MapObjectView View => (MapObjectView)base.View;

	public MapObjectEntity(string uniqueId, bool isInGame, BlueprintMechanicEntityFact blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public MapObjectEntity(MapObjectView view)
		: this(view.UniqueId, view.IsInGameBySettings, BlueprintWarhammerRoot.Instance.DefaultMapObjectBlueprint)
	{
	}

	protected MapObjectEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return new MechanicEntityFact(blueprint);
	}

	protected override IEntityViewBase CreateViewForData()
	{
		if (ViewSettings?.Blueprint != null)
		{
			GameObject gameObject = ViewSettings.Blueprint.Prefab.Load();
			if ((bool)gameObject && !gameObject.GetComponent<MapObjectView>())
			{
				PFLog.Default.Error($"Resource with id '{ViewSettings.Blueprint}' is not a MapObjectView");
				return null;
			}
			MapObjectView mapObjectView = UnityEngine.Object.Instantiate(gameObject, ViewSettings.Position, ViewSettings.Rotation).Or(null)?.GetComponent<MapObjectView>();
			if ((bool)mapObjectView)
			{
				mapObjectView.UniqueId = base.UniqueId;
				ViewSettings.Blueprint.InitializeObjectView(mapObjectView);
			}
			return mapObjectView;
		}
		return null;
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (View.AwarenessCheckComponent == null)
		{
			IsAwarenessCheckPassed = true;
		}
		if (!AwarenessCheckDC.HasValue || !AwarenessCheckDifficulty.HasValue)
		{
			AwarenessCheckDC = View.AwarenessCheckComponent.Or(null)?.GetCustomDC();
			AwarenessCheckDifficulty = View.AwarenessCheckComponent.Or(null)?.Difficulty;
		}
		else if (View.AwarenessCheckComponent != null)
		{
			View.AwarenessCheckComponent.SetCustomDC(AwarenessCheckDC.Value);
			View.AwarenessCheckComponent.Difficulty = AwarenessCheckDifficulty.Value;
		}
		WasHighlightedOnReveal = View.InteractionComponent == null;
		FactHolder.Fact fact = Facts.GetAll<FactHolder.Fact>().FirstItem();
		BlueprintLogicConnector blueprintLogicConnector = (View.FactHolder ? View.FactHolder.Blueprint : null);
		if (fact?.Blueprint != blueprintLogicConnector)
		{
			if (fact != null)
			{
				Facts.Remove(fact);
			}
			if ((bool)blueprintLogicConnector)
			{
				Facts.Add(new FactHolder.Fact(blueprintLogicConnector));
			}
		}
	}

	protected override void OnSpawn()
	{
		base.OnSpawn();
		EventBus.RaiseEvent((IMapObjectEntity)this, (Action<IMapObjectHandler>)delegate(IMapObjectHandler h)
		{
			h.HandleMapObjectSpawned();
		}, isCheckRuntime: true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		EventBus.RaiseEvent((IMapObjectEntity)this, (Action<IMapObjectHandler>)delegate(IMapObjectHandler h)
		{
			h.HandleMapObjectDestroyed();
		}, isCheckRuntime: true);
	}

	public virtual bool IsAwarenessRollAllowed([NotNull] BaseUnitEntity unit)
	{
		if (LastAwarenessRollRank.TryGetValue(unit.FromBaseUnitEntity(), out var value))
		{
			return value < unit.Skills.SkillCoercion.BaseValue;
		}
		return true;
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<EntityBoundsPart>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsAwarenessCheckPassed;
		result.Append(ref val2);
		if (AwarenessCheckDC.HasValue)
		{
			int val3 = AwarenessCheckDC.Value;
			result.Append(ref val3);
		}
		if (AwarenessCheckDifficulty.HasValue)
		{
			SkillCheckDifficulty val4 = AwarenessCheckDifficulty.Value;
			result.Append(ref val4);
		}
		Dictionary<UnitReference, int> lastAwarenessRollRank = LastAwarenessRollRank;
		if (lastAwarenessRollRank != null)
		{
			int val5 = 0;
			foreach (KeyValuePair<UnitReference, int> item in lastAwarenessRollRank)
			{
				Hash128 hash = default(Hash128);
				UnitReference obj = item.Key;
				Hash128 val6 = UnitReferenceHasher.GetHash128(ref obj);
				hash.Append(ref val6);
				int obj2 = item.Value;
				Hash128 val7 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash.Append(ref val7);
				val5 ^= hash.GetHashCode();
			}
			result.Append(ref val5);
		}
		bool val8 = WasHighlightedOnReveal;
		result.Append(ref val8);
		Hash128 val9 = ClassHasher<MapObjectViewSettings>.GetHash128(ViewSettings);
		result.Append(ref val9);
		return result;
	}
}
public abstract class MapObjectEntity<TBlueprint> : MapObjectEntity, IHashable where TBlueprint : BlueprintMechanicEntityFact
{
	public new TBlueprint OriginalBlueprint => (TBlueprint)base.OriginalBlueprint;

	public new TBlueprint Blueprint => (TBlueprint)base.Blueprint;

	public override Type RequiredBlueprintType => typeof(TBlueprint);

	protected MapObjectEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected MapObjectEntity(string uniqueId, bool isInGame, TBlueprint blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
