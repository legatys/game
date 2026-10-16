using Game.Combat;
using UnityEngine;

namespace Game.AI
{
    /// <summary>
    /// Мишень для тренировки: вспыхивает при попадании, "умирает" (скрывается) и возрождается.
    /// Голова — отдельный коллайдер с <see cref="Hitbox"/> (×2 урона).
    /// </summary>
    [RequireComponent(typeof(Health))]
    public sealed class TargetDummy : MonoBehaviour
    {
        /// <summary>Через сколько секунд мишень появляется снова.</summary>
        [SerializeField] private float respawnDelay = 3f;
        /// <summary>Цвет вспышки при попадании.</summary>
        [SerializeField] private Color flashColor = Color.white;
        /// <summary>Длительность вспышки, сек.</summary>
        [SerializeField] private float flashDuration = 0.08f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private Health _health;
        private Renderer[] _renderers;
        private Collider[] _colliders;
        private MaterialPropertyBlock _block;
        private float _flashTimer;
        private float _respawnTimer;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _colliders = GetComponentsInChildren<Collider>(true);
            _block = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
        }

        private void Update()
        {
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f) SetFlash(false);
            }

            if (_respawnTimer > 0f)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer <= 0f) Respawn();
            }
        }

        private void OnDamaged(DamageInfo info)
        {
            _flashTimer = flashDuration;
            SetFlash(true);
        }

        private void OnDied(DamageInfo info)
        {
            SetActive(false);
            _respawnTimer = respawnDelay;
        }

        private void Respawn()
        {
            _health.Revive();
            SetActive(true);
        }

        private void SetActive(bool active)
        {
            foreach (var renderer in _renderers) renderer.enabled = active;
            foreach (var collider in _colliders) collider.enabled = active;
        }

        private void SetFlash(bool on)
        {
            foreach (var renderer in _renderers)
            {
                if (on)
                {
                    renderer.GetPropertyBlock(_block);
                    _block.SetColor(BaseColorId, flashColor);
                    _block.SetColor(ColorId, flashColor);
                    renderer.SetPropertyBlock(_block);
                }
                else
                {
                    renderer.SetPropertyBlock(null);
                }
            }
        }
    }
}
