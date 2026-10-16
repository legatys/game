using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Физические запросы, которые игнорируют коллайдеры "владельца" (самого стрелка).
    /// Благодаря этому не нужно настраивать отдельные слои для игрока: луч камеры и пули
    /// не попадают в собственную капсулу персонажа.
    /// </summary>
    public static class PhysicsQuery
    {
        private static readonly RaycastHit[] Buffer = new RaycastHit[32];

        public static bool Raycast(Ray ray, float maxDistance, LayerMask mask, Transform ignoreRoot, out RaycastHit hit)
        {
            var count = Physics.RaycastNonAlloc(ray, Buffer, maxDistance, mask, QueryTriggerInteraction.Ignore);
            return Closest(count, ignoreRoot, out hit);
        }

        public static bool SphereCast(Ray ray, float radius, float maxDistance, LayerMask mask, Transform ignoreRoot, out RaycastHit hit)
        {
            var count = Physics.SphereCastNonAlloc(ray, radius, Buffer, maxDistance, mask, QueryTriggerInteraction.Ignore);
            return Closest(count, ignoreRoot, out hit);
        }

        private static bool Closest(int count, Transform ignoreRoot, out RaycastHit closest)
        {
            closest = default;
            var found = false;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < count; i++)
            {
                var hit = Buffer[i];
                if (ignoreRoot != null && hit.transform.IsChildOf(ignoreRoot)) continue;
                // Каст, начавшийся внутри коллайдера, возвращает distance 0 и point (0,0,0) — пропускаем.
                if (hit.distance <= 0f && hit.point == Vector3.zero) continue;
                if (hit.distance >= bestDistance) continue;

                bestDistance = hit.distance;
                closest = hit;
                found = true;
            }

            return found;
        }

        /// <summary>Случайное отклонение направления внутри конуса angleDegrees (для разброса).</summary>
        public static Vector3 RandomInCone(Vector3 direction, float angleDegrees)
        {
            if (angleDegrees <= 0f) return direction;

            var random = Random.insideUnitCircle * Mathf.Tan(angleDegrees * Mathf.Deg2Rad);
            var rotation = Quaternion.LookRotation(direction);
            return (rotation * new Vector3(random.x, random.y, 1f)).normalized;
        }
    }
}
