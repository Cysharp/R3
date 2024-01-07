using System.Threading;
using UnityEngine;

namespace R3
{
    internal static class MonoBehaviourExtensions
    {
        public static CancellationToken GetDestroyCancellationToken(this MonoBehaviour value)
        {
            // UNITY_2022_2_OR_NEWER has MonoBehavior.destroyCancellationToken
#if UNITY_2022_2_OR_NEWER
            return value.destroyCancellationToken;
#else
            return CancellationToken.None;;
#endif
        }
    }
}
