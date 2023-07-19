using Unity.Netcode.Components;
using UnityEngine;

namespace FlatFury.MainModule.Scripts.Network
{
    /// <summary>
    /// The <see cref="ClientNetworkTransform"/> class.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
