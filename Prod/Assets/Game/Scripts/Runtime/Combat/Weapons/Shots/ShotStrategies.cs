using Game.Core;
using UnityEngine;

namespace Game.Combat.Weapons
{
    /// <summary>
    /// Паттерн Strategy: ЧТО происходит при выстреле (луч, дробь, снаряд).
    /// Новый тип выстрела (лазер, огнемёт) = новый класс + строка в WeaponFactory.
    /// </summary>
    public interface IShotStrategy
    {
        void Fire(in ShotContext context);
    }

    /// <summary>Мгновенный луч (пистолет, винтовка).</summary>
    public sealed class HitscanShot : IShotStrategy
    {
        public void Fire(in ShotContext context)
        {
            var direction = PhysicsQuery.RandomInCone(context.BaseDirection, context.SpreadDegrees);
            ShotUtility.FireRay(context, direction, context.Definition.damage);
        }
    }

    /// <summary>Несколько лучей-дробинок за выстрел (дробовик).</summary>
    public sealed class SpreadShot : IShotStrategy
    {
        public void Fire(in ShotContext context)
        {
            var definition = context.Definition;
            var baseDirection = PhysicsQuery.RandomInCone(context.BaseDirection, context.SpreadDegrees);

            for (var i = 0; i < Mathf.Max(1, definition.pelletCount); i++)
            {
                var direction = PhysicsQuery.RandomInCone(baseDirection, definition.pelletSpread);
                ShotUtility.FireRay(context, direction, definition.damage);
            }
        }
    }

    /// <summary>Физический снаряд из пула (гранатомёт, ракета).</summary>
    public sealed class ProjectileShot : IShotStrategy
    {
        public void Fire(in ShotContext context)
        {
            var definition = context.Definition;
            if (definition.projectilePrefab == null)
            {
                Debug.LogWarning($"[ProjectileShot] У {definition.name} не задан projectilePrefab.");
                return;
            }

            var direction = PhysicsQuery.RandomInCone(context.BaseDirection, context.SpreadDegrees);
            var instance = CoreServices.Spawn(definition.projectilePrefab.gameObject, context.Origin, Quaternion.LookRotation(direction));
            instance.GetComponent<Projectile>().Launch(direction * definition.projectileSpeed, definition, context.Owner);
        }
    }

    /// <summary>Общий код лучевых выстрелов: рейкаст, урон, сила удара, эффекты.</summary>
    public static class ShotUtility
    {
        public static void FireRay(in ShotContext context, Vector3 direction, float damage)
        {
            var definition = context.Definition;
            var ray = new Ray(context.Origin, direction);
            var end = ray.GetPoint(definition.range);

            if (PhysicsQuery.Raycast(ray, definition.range, definition.hitMask, context.OwnerRoot, out var hit))
            {
                end = hit.point;

                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.ApplyDamage(new DamageInfo(damage, hit.point, direction, context.Owner));

                if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
                    hit.rigidbody.AddForceAtPosition(direction * definition.impactForce, hit.point, ForceMode.Impulse);

                CoreServices.Spawn(definition.impactPrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
            }

            SpawnTracer(definition.tracerPrefab, context.Origin, end);
        }

        private static void SpawnTracer(GameObject prefab, Vector3 from, Vector3 to)
        {
            var instance = CoreServices.Spawn(prefab, from, Quaternion.identity);
            if (instance != null && instance.TryGetComponent(out Tracer tracer)) tracer.Show(from, to);
        }
    }
}
