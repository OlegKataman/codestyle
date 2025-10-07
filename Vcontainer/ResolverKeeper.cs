using UnityEngine;
using VContainer;

namespace BlackHole.Common
{
    public sealed class ResolverKeeper : MonoBehaviour
    {
        [Inject]
        public IObjectResolver ObjectResolver { get; private set; }
    }
}
