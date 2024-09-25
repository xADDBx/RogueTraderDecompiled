using System;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
public class ScriptableObjectLink<T, TLink> : WeakResourceLink<T> where T : ScriptableObject where TLink : ScriptableObjectLink<T, TLink>, new()
{
}
