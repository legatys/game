using UnityEngine;

namespace Game.Combat.Weapons
{
    /// <summary>
    /// Визуал оружия (View в связке Weapon-модель / WeaponView-представление): модель, дуло, вспышка.
    /// Ничего не знает о патронах и уроне. Ось +Z модели = направление ствола.
    /// </summary>
    public sealed class WeaponView : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private GameObject muzzleFlash;
        [SerializeField] private float muzzleFlashDuration = 0.05f;
        [SerializeField] private AudioSource audioSource;

        private float _flashTimer;

        public Transform Muzzle => muzzle != null ? muzzle : transform;

        private void Awake()
        {
            if (muzzleFlash != null) muzzleFlash.SetActive(false);
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

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
