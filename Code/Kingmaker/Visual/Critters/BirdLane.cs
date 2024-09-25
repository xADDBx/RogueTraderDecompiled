using System;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

public class BirdLane : MonoBehaviour
{
	public Transform[] Points;

	private void Start()
	{
		int num = Points.IndexOf(null);
		if (num != -1)
		{
			throw new NullReferenceException(string.Format("Points #{0} is null at '{1}'", num, base.transform.GetHierarchyPath("/")));
		}
	}
}
