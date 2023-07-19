using UnityEngine;

namespace FlatFury.GameModule.SOs.Coins
{
    /// <summary>
    /// The <see cref="CoinSO"/> class.
    /// This class represents the coin data.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Entities/Coin", fileName = "New Coin", order = 2)]
    internal sealed class CoinSO : ScriptableObject
    {
        [SerializeField] private int _value = 1;
        [SerializeField] private Color _color = Color.yellow;
        [SerializeField] private float _nonSpawnRadius = 0.1f;

        /// <summary> Gets the coin value. </summary>
        public int Value => _value;

        /// <summary> Gets the coin color. </summary>
        public Color Color => _color;

        /// <summary> Gets the radius of avoidance when the other objects are spawn nearby. </summary>
        public float NonSpawnRadius => _nonSpawnRadius;
    }
}
