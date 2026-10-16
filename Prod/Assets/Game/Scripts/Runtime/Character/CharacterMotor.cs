using System;
using Game.Core;
using UnityEngine;

namespace Game.Character
{
    /// <summary>
    /// Низкоуровневое перемещение поверх CharacterController: скорость, гравитация, прыжок,
    /// рост капсулы, проверка потолка. Ничего не знает про ввод и состояния — ими управляет
    /// <see cref="PlayerCharacter"/> через состояния. Этот же мотор можно использовать для NPC.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterMotor : MonoBehaviour
    {
        /// <summary>Скорость прижатия к земле на склонах и ступеньках, м/с.</summary>
        [SerializeField] private float groundStickVelocity = -2f;
        /// <summary>Предельная скорость падения, м/с.</summary>
        [SerializeField] private float maxFallSpeed = 50f;
        /// <summary>Слои для проверки потолка.</summary>
        [SerializeField] private LayerMask collisionMask = ~0;

        private CharacterController _controller;
        private Vector3 _velocity;
        private float _ungroundedLock;
        private float _targetHeight;
        private float _heightChangeSpeed = 6f;

        /// <summary>Используемый CharacterController.</summary>
        public CharacterController Controller => _controller;
        /// <summary>Желаемая скорость (то, что мы задаём).</summary>
        public Vector3 Velocity => _velocity;
        /// <summary>Фактическая скорость после столкновений — для анимаций.</summary>
        public Vector3 ActualVelocity { get; private set; }
        /// <summary>Горизонтальная составляющая скорости.</summary>
        public Vector3 PlanarVelocity => new Vector3(_velocity.x, 0f, _velocity.z);
        /// <summary>Стоит ли на земле.</summary>
        public bool IsGrounded { get; private set; }
        /// <summary>Сколько секунд персонаж в воздухе (0 на земле).</summary>
        public float AirTime { get; private set; }
        /// <summary>Текущий рост капсулы.</summary>
        public float Height => _controller.height;
        /// <summary>Радиус капсулы.</summary>
        public float Radius => _controller.radius;

        /// <summary>Приземлился; параметр — вертикальная скорость в момент удара (отрицательная).</summary>
        public event Action<float> Landed;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _targetHeight = _controller.height;
        }

        /// <summary>Плавно разгоняет горизонтальную скорость к target.</summary>
        public void AccelerateTowards(Vector3 targetPlanarVelocity, float acceleration, float deltaTime)
        {
            var planar = Vector3.MoveTowards(PlanarVelocity, targetPlanarVelocity, acceleration * deltaTime);
            _velocity.x = planar.x;
            _velocity.z = planar.z;
        }

        /// <summary>Мгновенно задать горизонтальную скорость.</summary>
        public void SetPlanarVelocity(Vector3 planarVelocity)
        {
            _velocity.x = planarVelocity.x;
            _velocity.z = planarVelocity.z;
        }

        /// <summary>Прыжок на высоту height при гравитации gravity (отрицательной).</summary>
        public void Jump(float height, float gravity)
        {
            _velocity.y = Mathf.Sqrt(2f * height * Mathf.Abs(gravity));
            ForceUngrounded();
        }

        /// <summary>Мгновенно задать скорость (батут, отдача взрыва).</summary>
        public void Launch(Vector3 velocity)
        {
            _velocity = velocity;
            ForceUngrounded();
        }

        /// <summary>Добавить скорость (толчок).</summary>
        public void AddImpulse(Vector3 impulse)
        {
            _velocity += impulse;
            if (impulse.y > 0f) ForceUngrounded();
        }

        /// <summary>Двигает контроллер с учётом гравитации. Вызывается раз в кадр текущим состоянием.</summary>
        public void Simulate(float gravity, float deltaTime)
        {
            if (deltaTime <= 0f) return;

            UpdateHeight(deltaTime);

            if (IsGrounded && _velocity.y < 0f) _velocity.y = groundStickVelocity;
            else _velocity.y = Mathf.Max(_velocity.y + gravity * deltaTime, -maxFallSpeed);

            var fallSpeed = _velocity.y;
            var before = transform.position;
            var flags = _controller.Move(_velocity * deltaTime);
            ActualVelocity = (transform.position - before) / deltaTime;

            if ((flags & CollisionFlags.Above) != 0 && _velocity.y > 0f) _velocity.y = 0f;

            var wasGrounded = IsGrounded;
            _ungroundedLock -= deltaTime;
            IsGrounded = _ungroundedLock <= 0f && ((flags & CollisionFlags.Below) != 0 || _controller.isGrounded);
            AirTime = IsGrounded ? 0f : AirTime + deltaTime;

            if (!wasGrounded && IsGrounded) Landed?.Invoke(fallSpeed);
        }

        /// <summary>Плавно меняет рост капсулы (присед/подкат). Пол остаётся на месте.</summary>
        public void SetTargetHeight(float height, float changeSpeed)
        {
            _targetHeight = height;
            _heightChangeSpeed = changeSpeed;
        }

        /// <summary>Есть ли над головой место, чтобы вырасти до height.</summary>
        public bool CanStandUp(float height)
        {
            if (height <= _controller.height + 0.01f) return true;

            var radius = _controller.radius * 0.95f;
            var origin = transform.position + Vector3.up * (_controller.height - radius);
            var distance = height - _controller.height;
            return !PhysicsQuery.SphereCast(new Ray(origin, Vector3.up), radius, distance, collisionMask, transform, out _);
        }

        /// <summary>Повернуть персонажа к горизонтальному направлению с ограничением скорости.</summary>
        public void RotateTowards(Vector3 direction, float degreesPerSecond, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;

            var target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, degreesPerSecond * deltaTime);
        }

        /// <summary>Телепорт (CharacterController перезаписывает transform, поэтому временно выключаем его).</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _controller.enabled = true;
            _velocity = Vector3.zero;
            ActualVelocity = Vector3.zero;
        }

        private void ForceUngrounded()
        {
            IsGrounded = false;
            _ungroundedLock = 0.1f;
        }

        private void UpdateHeight(float deltaTime)
        {
            var current = _controller.height;
            if (Mathf.Approximately(current, _targetHeight)) return;

            var next = Mathf.MoveTowards(current, _targetHeight, _heightChangeSpeed * deltaTime);
            if (next > current && !CanStandUp(next)) return; // упёрлись в потолок

            _controller.height = next;
            _controller.center = new Vector3(0f, next * 0.5f, 0f);
        }
    }
}
