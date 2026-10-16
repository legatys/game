using Game.Character;
using UnityEngine;

namespace Game.Interaction
{
    /// <summary>Патроны: добавляет магазины всему оружию игрока.</summary>
    public sealed class AmmoPickup : Pickup
    {
        /// <summary>Сколько магазинов добавить каждому оружию.</summary>
        [SerializeField] private int magazines = 2;

        /// <summary>Добавляет патроны; false — всё оружие уже заполнено.</summary>
        protected override bool TryApply(PlayerCharacter player) =>
            player.Inventory != null && player.Inventory.AddAmmoToAll(magazines) > 0;
    }
}
