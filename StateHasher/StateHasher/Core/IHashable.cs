using UnityEngine;

namespace StateHasher.Core;

public interface IHashable
{
	Hash128 GetHash128();
}
