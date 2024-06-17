using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Sound;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("53900d6267fe4acbaaadb1bf10ee96f2")]
public class PostProcessingEffectsLibrary : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<PostProcessingEffectsLibrary>
	{
	}

	[Serializable]
	public class PostProcessingEffectWrapper
	{
		public VisualStateEffectType type;

		public VolumeProfile VolumeProfile;
	}

	[Serializable]
	public class WwiseStateReferenceEffectWrapper
	{
		public VisualStateEffectType type;

		public SoundEventReferences soundEventReferences;
	}

	[Serializable]
	public class SoundEventReferences
	{
		public AkStateReference State;

		[AkEventReference]
		public string StartEventName;

		[AkEventReference]
		public string StopEventName;
	}

	[SerializeField]
	private List<PostProcessingEffectWrapper> m_EffectWrappers;

	[SerializeField]
	private List<WwiseStateReferenceEffectWrapper> m_EffectWwiseEventWrappers;

	public Dictionary<VisualStateEffectType, VolumeProfile> GetEffectProfiles => m_EffectWrappers.ToDictionary((PostProcessingEffectWrapper x) => x.type, (PostProcessingEffectWrapper y) => y.VolumeProfile);

	public Dictionary<VisualStateEffectType, SoundEventReferences> GetEffectWwiseEvents => m_EffectWwiseEventWrappers.ToDictionary((WwiseStateReferenceEffectWrapper x) => x.type, (WwiseStateReferenceEffectWrapper y) => y.soundEventReferences);
}
