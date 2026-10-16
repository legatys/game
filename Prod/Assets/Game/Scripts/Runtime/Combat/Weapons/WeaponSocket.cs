using UnityEngine;

namespace Game.Combat.Weapons
{
    /// <summary>Источник точки прицеливания для визуала (сокет оружия, IK головы).</summary>
    public interface IAimProvider
    {
        /// <summary>Персонаж целится или стреляет — оружие и голова смотрят в точку прицела.</summary>
        bool IsCombatReady { get; }
        Vector3 AimPoint { get; }
    }

    /// <summary>
    /// Точка крепления оружия. В бою поворачивает ствол в точку прицела, вне боя — держит
    /// оружие в "походном" положении. Работает после анимации (LateUpdate), поэтому
    /// можно вешать сокет на кость скелета (грудь/правая рука).
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class WeaponSocket : MonoBehaviour
    {
        [SerializeField] private Vector3 restEulerAngles = new Vector3(35f, -20f, 0f);
        [SerializeField] private float rotationSharpness = 25f;

        private IAimProvider _aim;
        private Transform _owner;

        private void Awake()
        {
            _aim = GetComponentInParent<IAimProvider>();
            _owner = _aim is Component component ? component.transform : transform.root;
        }

        private void LateUpdate()
        {
            Quaternion target;
            if (_aim != null && _aim.IsCombatReady)
            {
                var direction = _aim.AimPoint - transform.position;
                target = direction.sqrMagnitude > 0.01f ? Quaternion.LookRotation(direction) : _owner.rotation;
            }
            else
            {
                target = _owner.rotation * Quaternion.Euler(restEulerAngles);
            }

            var t = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, t);
        }
    }
}
