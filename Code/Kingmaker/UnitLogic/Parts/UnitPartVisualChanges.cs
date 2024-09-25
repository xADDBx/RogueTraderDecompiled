using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartVisualChanges : BaseUnitPart, IHashable
{
	[JsonProperty]
	public List<string> SourceBone = new List<string>();

	[JsonProperty]
	public bool BoneReplaced;

	[JsonProperty]
	public bool BoneDefault;

	[JsonProperty]
	public CastSource CastSource;

	public void ReplaceCastSource(CastSource castSource)
	{
		CastSource = castSource;
	}

	public void AddReplacementBone(string newBone)
	{
		if (!BoneDefault)
		{
			BoneReplaced = true;
		}
		SourceBone.Add(newBone);
	}

	public void RemoveReplacementBone(string oldBone)
	{
		SourceBone.Remove(oldBone);
		if (SourceBone.Count <= 0)
		{
			BoneReplaced = false;
		}
	}

	public string GetReplacementBone()
	{
		if (SourceBone.Count < 0)
		{
			return "Locator_HeadCenterFX_00";
		}
		return SourceBone[SourceBone.Count - 1];
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<string> sourceBone = SourceBone;
		if (sourceBone != null)
		{
			for (int i = 0; i < sourceBone.Count; i++)
			{
				Hash128 val2 = StringHasher.GetHash128(sourceBone[i]);
				result.Append(ref val2);
			}
		}
		result.Append(ref BoneReplaced);
		result.Append(ref BoneDefault);
		result.Append(ref CastSource);
		return result;
	}
}
