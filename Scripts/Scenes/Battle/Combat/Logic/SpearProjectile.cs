using System;
using System.Collections;
using UnityEngine;

namespace Scenes.Battle.Combat.Logic
{
    public class SpearProjectile : MonoBehaviour
    {
        private enum State { Spawning, Orbiting, Firing }

        private State _currentState = State.Spawning;

        [Header("Spin Settings")]
        [SerializeField] private float normalSpinSpeed = 360f;
        [SerializeField] private float fastSpinSpeed = 1440f;
        private float _currentSpinSpeed;

        private Transform _orbitTarget;
        private Vector3 _orbitOffset;
        private Transform _attackTarget;
        private float _fireSpeed;
        private Action<SpearProjectile> _onHitCallback;

        public void Initialize(Transform orbitTarget, float angle, float radius, float heightOffset)
        {
            _orbitTarget = orbitTarget;
            _currentSpinSpeed = normalSpinSpeed;

            float rad = angle * Mathf.Deg2Rad;
            _orbitOffset = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius + heightOffset, 0);

            StartCoroutine(SpawnSequence());
        }

        private IEnumerator SpawnSequence()
        {
            _currentState = State.Spawning;
            float duration = 0.4f;
            float elapsed = 0f;

            Vector3 startPos = _orbitTarget.position;
            Quaternion startRot = Quaternion.identity;
            Quaternion endRot = Quaternion.Euler(0, 0, 90f);

            while (elapsed < duration)
            {
                if (_orbitTarget == null) yield break;
                // 타임스케일 무시
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float curveT = 1f - (1f - t) * (1f - t);

                transform.position = Vector3.Lerp(startPos, _orbitTarget.position + _orbitOffset, curveT);
                transform.rotation = Quaternion.Lerp(startRot, endRot, curveT);
                yield return null;
            }

            _currentState = State.Orbiting;
        }

        public void Fire(Transform target, float fireSpeed, Action<SpearProjectile> onHit)
        {
            _attackTarget = target;
            _fireSpeed = fireSpeed;
            _onHitCallback = onHit;
            StartCoroutine(FireSequence());
        }

        private IEnumerator FireSequence()
        {
            _currentState = State.Firing;
            _currentSpinSpeed = fastSpinSpeed;

            // 타임스케일 무시
            yield return new WaitForSecondsRealtime(0.1f);

            while (_attackTarget != null)
            {
                Vector3 currentPos = transform.position;
                Vector3 targetPos = _attackTarget.position;
                Vector3 dir = (targetPos - currentPos).normalized;

                // 타임스케일 무시
                transform.position += dir * _fireSpeed * Time.unscaledDeltaTime;

                if (Vector3.Distance(transform.position, targetPos) < 0.3f)
                {
                    _onHitCallback?.Invoke(this);
                    Destroy(gameObject);
                    yield break;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private void Update()
        {
            if (_currentState == State.Orbiting || _currentState == State.Firing)
            {
                // 타임스케일 무시
                transform.Rotate(Vector3.right * _currentSpinSpeed * Time.unscaledDeltaTime, Space.Self);
            }

            if (_currentState == State.Orbiting && _orbitTarget != null)
            {
                transform.position = _orbitTarget.position + _orbitOffset;
            }
        }
    }
}