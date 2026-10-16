using Game.Character;
using UnityEngine;

namespace Game.Interaction
{
    /// <summary>Патроны: добавляет магазины всему оружию игрока.</summary>
    public sealed class AmmoPickup : Pickup
    {
        [SerializeField] private int magazines = 2;

        protected override bool TryApply(PlayerCharacter player) =>
            player.Inventory != null && player.Inventory.AddAmmoToAll(magazines) > 0;
    }
}
