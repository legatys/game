using Game.Character;
using UnityEngine;

namespace Game.Interaction
{
    /// <summary>
    /// Батут: подбрасывает игрока и восстанавливает прыжки в воздухе
    /// (развитие идеи JumpableSurface из старого наброска). Нужен Collider с IsTrigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class JumpPad : MonoBehaviour
    {
        [Tooltip("Скорость запуска в локальных координатах батута")]
        [SerializeField] private Vector3 launchVelocity = new Vector3(0f, 14f, 4f);

        private void Awake() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerCharacter>();
            if (player != null && !player.IsDead)
                player.Launch(transform.TransformDirection(launchVelocity));
        }
    }
}
