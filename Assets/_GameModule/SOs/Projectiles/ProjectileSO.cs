using UnityEngine;

namespace FlatFury.GameModule.SOs.Projectiles
{
    /// <summary>
    /// The <see cref="ProjectileSO"/> class.
    /// This class represents the projectile data.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Entities/Projectile", fileName = "New Projectile", order = 1)]
    internal sealed class ProjectileSO : ScriptableObject
    {
        [SerializeField] private float _damage = 1f;
        [SerializeField] private float _fireRate = 10f;
        [SerializeField] private float _speed = 10f;

        /// <summary> Gets the projectile damage. </summary>
        public float Damage => _damage;

        /// <summary> Gets the projectile fire rate. </summary>
        public float FireRate => _fireRate;

        /// <summary> Gets the projectile speed. </summary>
        public float Speed => _speed;
    }
}
