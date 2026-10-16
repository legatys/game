using UnityEngine;

namespace Game.Combat.Weapons
{
    /// <summary>
    /// Визуал оружия (View в связке Weapon-модель / WeaponView-представление): модель, дуло, вспышка.
    /// Ничего не знает о патронах и уроне. Ось +Z модели = направление ствола.
    /// </summary>
    public sealed class WeaponView : MonoBehaviour
    {
        /// <summary>Точка дула (откуда летят пули).</summary>
        [SerializeField] private Transform muzzle;
        /// <summary>Объект вспышки (включается на время выстрела).</summary>
        [SerializeField] private GameObject muzzleFlash;
        /// <summary>Длительность вспышки, сек.</summary>
        [SerializeField] private float muzzleFlashDuration = 0.05f;
        /// <summary>Источник звука. Пусто — AudioSource.PlayClipAtPoint.</summary>
        [SerializeField] private AudioSource audioSource;

        private float _flashTimer;

        /// <summary>Точка дула (или сам объект, если не задана).</summary>
        public Transform Muzzle => muzzle != null ? muzzle : transform;

        private void Awake()
        {
            if (muzzleFlash != null) muzzleFlash.SetActive(false);
        }

        /// <summary>Задаёт дуло и вспышку. Вызывать до активации объекта.</summary>
        public void Configure(Transform muzzlePoint, GameObject flash)
        {
            muzzle = muzzlePoint;
            muzzleFlash = flash;
        }

        /// <summary>Показать/спрятать модель.</summary>
        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        /// <summary>Вспышка и звук выстрела.</summary>
        public void PlayFireEffects(AudioClip clip)
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.SetActive(true);
                muzzleFlash.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                _flashTimer = muzzleFlashDuration;
            }

            PlaySound(clip);
        }

        /// <summary>Проиграть звук, если он задан.</summary>
        public void PlaySound(AudioClip clip)
        {
            if (clip == null) return;
            if (audioSource != null) audioSource.PlayOneShot(clip);
            else AudioSource.PlayClipAtPoint(clip, Muzzle.position);
        }

        private void Update()
        {
            if (_flashTimer <= 0f) return;

            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f && muzzleFlash != null) muzzleFlash.SetActive(false);
        }

        /// <summary>Запасной визуал, если у оружия нет префаба: кубик-ствол из примитивов.</summary>
        public static WeaponView CreatePlaceholder(Transform parent, string name)
        {
            var root = new GameObject($"{name} (placeholder)");
            root.transform.SetParent(parent, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.08f, 0.12f, 0.45f);
            body.transform.localPosition = new Vector3(0f, 0f, 0.15f);

            var muzzlePoint = new GameObject("Muzzle").transform;
            muzzlePoint.SetParent(root.transform, false);
            muzzlePoint.localPosition = new Vector3(0f, 0f, 0.4f);

            var view = root.AddComponent<WeaponView>();
            view.muzzle = muzzlePoint;
            return view;
        }
    }
}
