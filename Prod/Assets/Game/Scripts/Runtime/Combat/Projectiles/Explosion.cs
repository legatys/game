using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>Урон по площади с затуханием к краю и толчком физических объектов.</summary>
    public static class Explosion
    {
        private static readonly Collider[] Buffer = new Collider[64];
        private static readonly HashSet<Transform> Damaged = new HashSet<Transform>();
        private static readonly HashSet<Rigidbody> Pushed = new HashSet<Rigidbody>();

        /// <summary>Наносит урон всем IDamageable в радиусе (с затуханием к краю), толкает физические тела и публикует ExplosionEvent.</summary>
        public static void Detonate(Vector3 center, float radius, float damage, float force, GameObject instigator)
        {
            if (radius <= 0f) return;

            var count = Physics.OverlapSphereNonAlloc(center, radius, Buffer, ~0, QueryTriggerInteraction.Ignore);
            Damaged.Clear();
            Pushed.Clear();

            for (var i = 0; i < count; i++)
            {
                var collider = Buffer[i];
                var closest = collider.ClosestPoint(center);
                var falloff = 1f - Mathf.Clamp01(Vector3.Distance(center, closest) / radius);

                var damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable != null && Damaged.Add(damageable.DamageRoot))
                {
                    var direction = (closest - center).normalized;
                    damageable.ApplyDamage(new DamageInfo(damage * falloff, closest, direction, instigator));
                }

                var body = collider.attachedRigidbody;
                if (body != null && !body.isKinematic && Pushed.Add(body))
                    body.AddExplosionForce(force, center, radius, 0.5f, ForceMode.Impulse);
            }

            EventBus.Publish(new ExplosionEvent(center, radius));
        }
    }
}
