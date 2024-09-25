using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartMirrorImage : BaseUnitPart, IHashable
{
	[JsonProperty]
	public List<int> VisualImages = new List<int>();

	[JsonProperty]
	public List<int> MechanicsImages = new List<int>();

	[CanBeNull]
	public MirrorImageFX Fx;

	[JsonProperty]
	public Buff Source { get; private set; }

	public void Init(int imagesCount, Buff source)
	{
		if (Source == null)
		{
			VisualImages.Clear();
			MechanicsImages.Clear();
			for (int i = 1; i <= imagesCount; i++)
			{
				VisualImages.Add(i);
				MechanicsImages.Add(i);
			}
			Source = source;
		}
	}

	public int TryAbsorbHit(bool force = false)
	{
		if (MechanicsImages.Count <= 0)
		{
			return 0;
		}
		int num = (force ? MechanicsImages.Count : base.Owner.Random.Range(0, MechanicsImages.Count + 1));
		if (num <= 0)
		{
			return 0;
		}
		int result = MechanicsImages[num - 1];
		MechanicsImages.RemoveAt(num - 1);
		return result;
	}

	public void SpendReservedImage(int imageIndex)
	{
		if (Fx != null)
		{
			Fx.DestroyImage(imageIndex);
		}
		VisualImages.Remove(imageIndex);
		if (VisualImages.Count <= 0)
		{
			Source?.Remove();
		}
	}

	protected override void OnDetach()
	{
		Source = null;
		if (Fx != null)
		{
			Utils.EditorSafeDestroy(Fx.gameObject);
		}
		Fx = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<int> visualImages = VisualImages;
		if (visualImages != null)
		{
			for (int i = 0; i < visualImages.Count; i++)
			{
				int obj = visualImages[i];
				Hash128 val2 = UnmanagedHasher<int>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		List<int> mechanicsImages = MechanicsImages;
		if (mechanicsImages != null)
		{
			for (int j = 0; j < mechanicsImages.Count; j++)
			{
				int obj2 = mechanicsImages[j];
				Hash128 val3 = UnmanagedHasher<int>.GetHash128(ref obj2);
				result.Append(ref val3);
			}
		}
		Hash128 val4 = ClassHasher<Buff>.GetHash128(Source);
		result.Append(ref val4);
		return result;
	}
}
