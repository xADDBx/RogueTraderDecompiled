using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;

namespace Kingmaker.Utility.Random;

public static class PFStatefulRandom
{
	public static class Controllers
	{
		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Inclemency = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Inclemency");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom PsychicPhenomena = new Kingmaker.Utility.StatefulRandom.StatefulRandom("PsychicPhenomena");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Projectiles = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Projectiles");
	}

	public static class UnitLogic
	{
		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Abilities = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Abilities");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Customization = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Customization");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Parts = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Parts");
	}

	public static class Visuals
	{
		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Animation = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Animation");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Animation1 = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Animation1");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Animation2 = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Animation2");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Animation3 = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Animation3");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Animation4 = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Animation4");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom AnimationIdle = new Kingmaker.Utility.StatefulRandom.StatefulRandom("AnimationIdle");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom DollRoom = new Kingmaker.Utility.StatefulRandom.StatefulRandom("DollRoom");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Critters = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Critters");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Particles = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Particles");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Sounds = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Sounds");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom SolarSystem = new Kingmaker.Utility.StatefulRandom.StatefulRandom("SolarSystem");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Rigidbody = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Rigidbody");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom CharacterStudio = new Kingmaker.Utility.StatefulRandom.StatefulRandom("CharacterStudio");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom UnitAsks = new Kingmaker.Utility.StatefulRandom.StatefulRandom("UnitAsks");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Starship = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Starship");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom Fx = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Fx");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom HitSystem = new Kingmaker.Utility.StatefulRandom.StatefulRandom("HitSystem");

		public static Kingmaker.Utility.StatefulRandom.StatefulRandom AttachedDroppedLoot = new Kingmaker.Utility.StatefulRandom.StatefulRandom("AttachedDroppedLoot");
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct RandomContextHandler : IDisposable
	{
		void IDisposable.Dispose()
		{
			s_UiContext--;
		}
	}

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Action;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Blueprints;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Cutscene;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom ExtraCutscene;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom CutsceneAttack;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Timer;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom RuleSystem;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom DialogSystem;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Dungeon;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom GlobalMap;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Mechanics;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom SpaceCombat;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom UI;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Utility;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom View;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Visual;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Camera;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Swarm;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Weather;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Trails;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom LoadingScreen;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom DebugDialog;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Pathfinding;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom FpsFreezer;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom BaseGetter;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Bark;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Cargo;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Designers;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom Qa;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom UnitRandom;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom PreviewUnitRandom;

	public static Kingmaker.Utility.StatefulRandom.StatefulRandom NonDeterministic;

	public static readonly Kingmaker.Utility.StatefulRandom.StatefulRandom[] All;

	public static readonly Kingmaker.Utility.StatefulRandom.StatefulRandom[] Serializable;

	public static readonly Kingmaker.Utility.StatefulRandom.StatefulRandom[] NonSerializable;

	private static uint? s_OverridenRandomNoise;

	private static int s_UiContext;

	public static bool IsUiContext => 0 < s_UiContext;

	static PFStatefulRandom()
	{
		Action = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Action");
		Blueprints = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Blueprints");
		Cutscene = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Cutscene");
		ExtraCutscene = new Kingmaker.Utility.StatefulRandom.StatefulRandom("ExtraCutscene");
		CutsceneAttack = new Kingmaker.Utility.StatefulRandom.StatefulRandom("CutsceneAttack");
		Timer = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Timer");
		RuleSystem = new Kingmaker.Utility.StatefulRandom.StatefulRandom("RuleSystem");
		DialogSystem = new Kingmaker.Utility.StatefulRandom.StatefulRandom("DialogSystem");
		Dungeon = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Dungeon");
		GlobalMap = new Kingmaker.Utility.StatefulRandom.StatefulRandom("GlobalMap");
		Mechanics = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Mechanics");
		SpaceCombat = new Kingmaker.Utility.StatefulRandom.StatefulRandom("SpaceCombat");
		UI = new Kingmaker.Utility.StatefulRandom.StatefulRandom("UI");
		Utility = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Utility");
		View = new Kingmaker.Utility.StatefulRandom.StatefulRandom("View");
		Visual = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Visual");
		Camera = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Camera");
		Swarm = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Swarm");
		Weather = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Weather");
		Trails = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Trails");
		LoadingScreen = new Kingmaker.Utility.StatefulRandom.StatefulRandom("LoadingScreen");
		DebugDialog = new Kingmaker.Utility.StatefulRandom.StatefulRandom("DebugDialog");
		Pathfinding = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Pathfinding");
		FpsFreezer = new Kingmaker.Utility.StatefulRandom.StatefulRandom("FpsFreezer");
		BaseGetter = new Kingmaker.Utility.StatefulRandom.StatefulRandom("BaseGetter");
		Bark = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Bark");
		Cargo = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Cargo");
		Designers = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Designers");
		Qa = new Kingmaker.Utility.StatefulRandom.StatefulRandom("Qa");
		UnitRandom = new Kingmaker.Utility.StatefulRandom.StatefulRandom("UnitRandom");
		PreviewUnitRandom = new Kingmaker.Utility.StatefulRandom.StatefulRandom("PreviewUnitRandom");
		NonDeterministic = new Kingmaker.Utility.StatefulRandom.StatefulRandom("NonDeterministic");
		All = CollectAllStatefulRandom();
		Serializable = CollectSerializableStatefulRandom();
		NonSerializable = All.Except(Serializable).ToArray();
	}

	private static Kingmaker.Utility.StatefulRandom.StatefulRandom[] CollectAllStatefulRandom()
	{
		List<Kingmaker.Utility.StatefulRandom.StatefulRandom> list = new List<Kingmaker.Utility.StatefulRandom.StatefulRandom>
		{
			Action,
			Blueprints,
			Cutscene,
			ExtraCutscene,
			CutsceneAttack,
			Timer,
			RuleSystem,
			DialogSystem,
			Dungeon,
			GlobalMap,
			Mechanics,
			SpaceCombat,
			UI,
			Utility,
			View,
			Visual,
			Camera,
			Swarm,
			Weather,
			Trails,
			LoadingScreen,
			DebugDialog,
			Pathfinding,
			FpsFreezer,
			BaseGetter,
			Bark,
			Cargo,
			Designers,
			Qa,
			UnitRandom,
			PreviewUnitRandom,
			NonDeterministic,
			Controllers.Inclemency,
			Controllers.PsychicPhenomena,
			Controllers.Projectiles,
			UnitLogic.Abilities,
			UnitLogic.Customization,
			UnitLogic.Parts,
			Visuals.Animation,
			Visuals.AnimationIdle,
			Visuals.DollRoom,
			Visuals.Critters,
			Visuals.Particles,
			Visuals.Sounds,
			Visuals.SolarSystem,
			Visuals.Rigidbody,
			Visuals.CharacterStudio,
			Visuals.UnitAsks,
			Visuals.Starship,
			Visuals.Fx,
			Visuals.HitSystem,
			Visuals.AttachedDroppedLoot
		};
		list.IncreaseCapacity(list.Count + PFUuid.All.Length);
		Uuid[] all = PFUuid.All;
		foreach (Uuid uuid in all)
		{
			list.Add(uuid.Random);
		}
		list.AddRange(new Kingmaker.Utility.StatefulRandom.StatefulRandom[4]
		{
			Visuals.Animation1,
			Visuals.Animation2,
			Visuals.Animation3,
			Visuals.Animation4
		});
		return list.ToArray();
	}

	private static Kingmaker.Utility.StatefulRandom.StatefulRandom[] CollectSerializableStatefulRandom()
	{
		List<Kingmaker.Utility.StatefulRandom.StatefulRandom> list = new List<Kingmaker.Utility.StatefulRandom.StatefulRandom>(All);
		list.Remove(ExtraCutscene);
		list.Remove(UI);
		list.Remove(Visual);
		list.Remove(Trails);
		list.Remove(PreviewUnitRandom);
		list.Remove(Visuals.AnimationIdle);
		list.Remove(Visuals.DollRoom);
		list.Remove(Visuals.Particles);
		list.Remove(Visuals.Sounds);
		list.Remove(Visuals.UnitAsks);
		list.Remove(Visuals.Fx);
		list.Remove(Visuals.AttachedDroppedLoot);
		list.Remove(Camera);
		list.Remove(NonDeterministic);
		list.RemoveAll((Kingmaker.Utility.StatefulRandom.StatefulRandom r) => PFUuid.NonSerializable.Contains((Uuid uuid) => uuid.Random == r));
		return list.ToArray();
	}

	public static void GetStates(ref List<SerializableRandState> endStates)
	{
		Kingmaker.Utility.StatefulRandom.StatefulRandom[] all = All;
		if (endStates == null)
		{
			endStates = new List<SerializableRandState>(all.Length);
		}
		else
		{
			endStates.Clear();
		}
		int i = 0;
		for (int num = all.Length; i < num; i++)
		{
			Kingmaker.Utility.StatefulRandom.StatefulRandom statefulRandom = all[i];
			endStates.Add(new SerializableRandState(statefulRandom.Name, statefulRandom.State));
		}
	}

	public static void OverrideRandomNoise(uint randomNoise)
	{
		s_OverridenRandomNoise = randomNoise;
	}

	public static void SetStates(List<SerializableRandState> endStates)
	{
		uint num = (uint)(((int?)s_OverridenRandomNoise) ?? UnityEngine.Random.Range(int.MinValue, int.MaxValue));
		s_OverridenRandomNoise = null;
		UnityEngine.Random.InitState(-1);
		Kingmaker.Utility.StatefulRandom.StatefulRandom[] all = All;
		for (uint num2 = 0u; num2 < all.Length; num2++)
		{
			all[num2].Seed(num2 + num);
		}
		int i = 0;
		for (int num3 = endStates.TryCount(); i < num3; i++)
		{
			SerializableRandState serializableRandState = endStates[i];
			if (TryFindRnd(serializableRandState.Name, out var rnd2))
			{
				rnd2.State = serializableRandState.Value;
				rnd2.Seed(rnd2.Rand.Seed + num);
			}
		}
		static bool TryFindRnd(string name, out Kingmaker.Utility.StatefulRandom.StatefulRandom rnd)
		{
			Kingmaker.Utility.StatefulRandom.StatefulRandom[] all2 = All;
			int j = 0;
			for (int num4 = all2.Length; j < num4; j++)
			{
				Kingmaker.Utility.StatefulRandom.StatefulRandom statefulRandom = all2[j];
				if (statefulRandom.Name == name)
				{
					rnd = statefulRandom;
					return true;
				}
			}
			rnd = null;
			return false;
		}
	}

	public static RandomContextHandler StartUiContext()
	{
		s_UiContext++;
		return default(RandomContextHandler);
	}

	[RuntimeInitializeOnLoadMethod]
	private static void InitializeOnLoad()
	{
		SetStates(new List<SerializableRandState>());
	}
}
