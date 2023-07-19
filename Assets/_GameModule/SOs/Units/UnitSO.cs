using UnityEngine;

namespace FlatFury.GameModule.SOs.Units
{
    /// <summary>
    /// The <see cref="UnitSO"/> class.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Entities/Units", fileName = "New Unit", order = 0)]
    internal sealed class UnitSO : ScriptableObject
    {
        [SerializeField] private string _name = "Mk-I";
        [SerializeField] private float _moveSpeed = 3000f;
        [SerializeField] private float _rotateSpeed = 10f;
        [SerializeField] private float _health = 10f;
        [SerializeField] private float _nonSpawnRadius = 1f;

        /// <summary> Gets the name of the unit. </summary>
        public string Name => _name;

        /// <summary> Gets the move speed of the unit. </summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary> Gets the rotation speed of the unit. </summary>
        public float RotateSpeed => _rotateSpeed;

        /// <summary> Gets the (max) health of the unit. </summary>
        public float Health => _health;

        /// <summary> Gets the radius of avoidance when the other objects are spawn nearby. </summary>
        public float NonSpawnRadius => _nonSpawnRadius;
    }
}
