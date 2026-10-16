using Game.Character;
using UnityEngine;

namespace Game.Interaction
{
    /// <summary>Аптечка: лечит, если здоровье не полное.</summary>
    public sealed class HealthPickup : Pickup
    {
        [SerializeField] private float amount = 50f;

        protected override bool TryApply(PlayerCharacter player) => player.Health.Heal(amount) > 0f;
    }
}
