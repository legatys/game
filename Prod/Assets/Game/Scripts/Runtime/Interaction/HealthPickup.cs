using Game.Character;
using UnityEngine;

namespace Game.Interaction
{
    /// <summary>Аптечка: лечит, если здоровье не полное.</summary>
    public sealed class HealthPickup : Pickup
    {
        /// <summary>Сколько здоровья восстанавливает.</summary>
        [SerializeField] private float amount = 50f;

        /// <summary>Лечит; false — здоровье уже полное.</summary>
        protected override bool TryApply(PlayerCharacter player) => player.Health.Heal(amount) > 0f;
    }
}
