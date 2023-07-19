using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace FlatFury.MainModule.Scripts.Data
{
    /// <summary>
    /// The <see cref="PlayerData"/> class.
    /// This class represents the network-ready player data.
    /// </summary>
    internal struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
    {
        public ulong clientId;
        public int colorId;
        public FixedString64Bytes playerName;
        public long score;

        /// <summary> Gets the player name. </summary>
        public string PlayerName => playerName.ToString();

        /// <summary> Gets the color if set. </summary>
        public Color Color { get; set; }

        public bool Equals(PlayerData other) => 
            clientId == other.clientId && 
            colorId == other.colorId && 
            playerName == other.playerName &&
            score == other.score;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref colorId);
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref score);
        }

        public override string ToString() => $"#{clientId} : {playerName}; color={colorId}; score={score}";
    }
}
