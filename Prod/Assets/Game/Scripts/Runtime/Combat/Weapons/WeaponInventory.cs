using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat.Weapons
{
    /// <summary>
    /// Слоты оружия персонажа: выдача стартового оружия, переключение, подбор, патроны.
    /// Само оружие тикает владелец (PlayerCombat), инвентарь только хранит и переключает.
    /// </summary>
    public sealed class WeaponInventory : MonoBehaviour
    {
        /// <summary>Оружие, выдаваемое при появлении.</summary>
        [SerializeField] private WeaponDefinition[] startingWeapons = Array.Empty<WeaponDefinition>();
        [Tooltip("Куда крепится модель оружия (обычно WeaponSocket на груди/руке)")]
        [SerializeField] private Transform weaponSocket;
        /// <summary>Максимум слотов.</summary>
        [SerializeField] private int maxWeapons = 4;

        private readonly List<Weapon> _weapons = new List<Weapon>();
        private int _index = -1;

        /// <summary>Всё оружие в слотах.</summary>
        public IReadOnlyList<Weapon> Weapons => _weapons;
        /// <summary>Индекс оружия в руках (-1 — нет).</summary>
        public int CurrentIndex => _index;
        /// <summary>Оружие в руках (или null).</summary>
        public Weapon Current => _index >= 0 && _index < _weapons.Count ? _weapons[_index] : null;

        /// <summary>Сменилось оружие в руках.</summary>
        public event Action<Weapon> WeaponEquipped;
        /// <summary>Добавлено новое оружие.</summary>
        public event Action<Weapon> WeaponAdded;

        private void Awake()
        {
            if (weaponSocket == null) weaponSocket = transform;
            foreach (var definition in startingWeapons)
                if (definition != null)
                    AddWeapon(definition);
        }

        /// <summary>Настройка из кода. Вызывать до активации объекта (до Awake).</summary>
        public void Configure(WeaponDefinition[] weapons, Transform socket)
        {
            startingWeapons = weapons ?? Array.Empty<WeaponDefinition>();
            weaponSocket = socket;
        }

        /// <summary>Добавить оружие. Если такое уже есть — добавляются патроны.</summary>
        public Weapon AddWeapon(WeaponDefinition definition)
        {
            var existing = _weapons.Find(w => w.Definition == definition);
            if (existing != null)
            {
                existing.AddReserveAmmo(definition.magazineSize);
                return existing;
            }

            if (_weapons.Count >= maxWeapons) return null;

            var weapon = WeaponFactory.Create(definition, weaponSocket);
            weapon.Holster();
            _weapons.Add(weapon);
            WeaponAdded?.Invoke(weapon);

            if (Current == null) Equip(_weapons.Count - 1);
            return weapon;
        }

        /// <summary>Взять оружие из слота <paramref name="index"/>. false — слот пуст или уже в руках.</summary>
        public bool Equip(int index)
        {
            if (index < 0 || index >= _weapons.Count || index == _index) return false;

            Current?.Holster();
            _index = index;
            Current.Draw();
            WeaponEquipped?.Invoke(Current);
            return true;
        }

        /// <summary>Следующее оружие по кругу.</summary>
        public void EquipNext() => Cycle(1);
        /// <summary>Предыдущее оружие по кругу.</summary>
        public void EquipPrevious() => Cycle(-1);

        /// <summary>Добавить всему оружию по magazines магазинов. Возвращает суммарно добавленное.</summary>
        public int AddAmmoToAll(int magazines)
        {
            var added = 0;
            foreach (var weapon in _weapons)
                added += weapon.AddReserveAmmo(weapon.Definition.magazineSize * magazines);
            return added;
        }

        private void Cycle(int step)
        {
            if (_weapons.Count < 2) return;
            Equip((_index + step + _weapons.Count) % _weapons.Count);
        }
    }
}
