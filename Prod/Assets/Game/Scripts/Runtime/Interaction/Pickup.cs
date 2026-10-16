using Game.Character;
using UnityEngine;

namespace Game.Interaction
{
    /// <summary>
    /// Базовый подбираемый предмет. Паттерн Template Method: общий алгоритм
    /// (вошёл игрок → TryApply → спрятать → появиться снова) в базе, а эффект — в наследнике.
    /// Нужен Collider с IsTrigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class Pickup : MonoBehaviour
    {
        [SerializeField] private float respawnTime = 10f;
        [SerializeField] private Transform visual;
        [SerializeField] private float spinSpeed = 90f;
        [SerializeField] private float bobHeight = 0.15f;

        private Collider _trigger;
        private Vector3 _visualStart;
        private float _respawnTimer;

        protected virtual void Awake()
        {
            _trigger = GetComponent<Collider>();
            _trigger.isTrigger = true;
            if (visual != null) _visualStart = visual.localPosition;
        }

        /// <summary>Объект, который крутится и прячется после подбора.</summary>
        public void Configure(Transform visualRoot) => visual = visualRoot;

        private void Update()
        {
            if (_respawnTimer > 0f)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer <= 0f) SetAvailable(true);
                return;
            }

            if (visual == null) return;
            visual.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            visual.localPosition = _visualStart + Vector3.up * (Mathf.Sin(Time.time * 2f) * bobHeight);
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerCharacter>();
            if (player == null || player.IsDead) return;
            if (!TryApply(player)) return;

            SetAvailable(false);
            _respawnTimer = respawnTime;
        }

        /// <summary>Применить эффект. false — предмет не нужен (полное здоровье и т.п.) и остаётся лежать.</summary>
        protected abstract bool TryApply(PlayerCharacter player);

        private void SetAvailable(bool available)
        {
            _trigger.enabled = available;
            if (visual != null) visual.gameObject.SetActive(available);
        }
    }
}
