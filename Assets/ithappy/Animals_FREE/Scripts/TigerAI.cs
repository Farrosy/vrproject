using System;
using UnityEngine;

namespace ithappy.Animals_FREE
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CreatureMover))]
    [DisallowMultipleComponent]
    public class TigerAI : MonoBehaviour
    {
        [Header("Wander")]
        [SerializeField, Min(0f)]
        private float m_WanderRadius = 8f;

        [SerializeField, Min(0f)]
        private float m_AreaRadius = 15f;

        [SerializeField]
        private Transform m_AreaCenter;

        [SerializeField, Min(0f)]
        private float m_IdleTimeMin = 2f;

        [SerializeField, Min(0f)]
        private float m_IdleTimeMax = 5f;

        [Header("Detection")]
        [SerializeField, Min(0f)]
        private float m_DetectionRadius = 12f;

        [SerializeField]
        private LayerMask m_TargetLayerMask = Physics.DefaultRaycastLayers;

        [SerializeField]
        private string[] m_TargetNameKeywords = new[] { "Horse", "Cow" };

        [Header("Combat")]
        [SerializeField, Min(0f)]
        private float m_AttackRadius = 1.5f;

        [SerializeField, Min(0f)]
        private float m_AttackDelay = 1.2f;

        [SerializeField, Min(0f)]
        private float m_EatingTime = 3f;

        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float m_WalkSpeed = 1f;

        [SerializeField, Min(0f)]
        private float m_RunSpeed = 4f;

        [SerializeField, Range(0f, 360f)]
        private float m_RotationSpeed = 180f;

        private CreatureMover m_Mover;
        private Collider[] m_OverlapBuffer = new Collider[32];
        private Transform m_TargetTransform;
        private Vector3 m_TargetPosition;
        private Vector3 m_EatPosition;
        private float m_StateTimer;
        private bool m_HasTarget;
        private State m_State;

        private enum State
        {
            Idle,
            Wander,
            Detect,
            Chase,
            Attack,
            Eat,
            Return
        }

        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
            m_Mover.SetSpeeds(m_WalkSpeed, m_RotationSpeed);
            SetIdle();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;

            if (m_State != State.Detect && m_State != State.Attack && m_State != State.Eat)
            {
                var nearestTarget = FindNearestTarget();
                if (nearestTarget != null)
                {
                    SetDetect(nearestTarget);
                }
            }

            switch (m_State)
            {
                case State.Idle:
                    m_StateTimer -= deltaTime;
                    if (m_StateTimer <= 0f)
                    {
                        PickWanderTarget();
                    }
                    break;
                case State.Wander:
                    if (HasArrived())
                    {
                        SetIdle();
                    }
                    else if (IsOutsideArea())
                    {
                        SetReturn();
                    }
                    break;
                case State.Detect:
                    m_StateTimer -= deltaTime;
                    if (m_TargetTransform == null || !IsTargetValid(m_TargetTransform) || !IsWithinDetectionRadius(m_TargetTransform.position))
                    {
                        SetIdle();
                    }
                    else if (m_StateTimer <= 0f)
                    {
                        SetChase();
                    }
                    break;
                case State.Chase:
                    if (m_TargetTransform == null || !IsTargetValid(m_TargetTransform) || !IsWithinDetectionRadius(m_TargetTransform.position))
                    {
                        SetIdle();
                    }
                    else if (Vector3.Distance(transform.position, m_TargetTransform.position) <= m_AttackRadius)
                    {
                        SetAttack();
                    }
                    break;
                case State.Attack:
                    if (m_TargetTransform == null)
                    {
                        SetIdle();
                    }
                    else
                    {
                        m_StateTimer -= deltaTime;
                        if (m_StateTimer <= 0f)
                        {
                            ResolveAttack();
                        }
                    }
                    break;
                case State.Eat:
                    m_StateTimer -= deltaTime;
                    if (m_StateTimer <= 0f)
                    {
                        if (IsOutsideArea())
                        {
                            SetReturn();
                        }
                        else
                        {
                            SetIdle();
                        }
                    }
                    break;
                case State.Return:
                    if (HasArrived())
                    {
                        SetIdle();
                    }
                    break;
            }

            ApplyMovement();
        }

        private void OnValidate()
        {
            m_WanderRadius = Mathf.Max(m_WanderRadius, 0f);
            m_AreaRadius = Mathf.Max(m_AreaRadius, 0f);
            m_IdleTimeMin = Mathf.Max(m_IdleTimeMin, 0f);
            m_IdleTimeMax = Mathf.Max(m_IdleTimeMax, m_IdleTimeMin);
            m_DetectionRadius = Mathf.Max(m_DetectionRadius, 0f);
            m_AttackRadius = Mathf.Max(m_AttackRadius, 0f);
            m_AttackDelay = Mathf.Max(m_AttackDelay, 0f);
            m_EatingTime = Mathf.Max(m_EatingTime, 0f);
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RunSpeed = Mathf.Max(m_RunSpeed, 0f);
            m_RotationSpeed = Mathf.Max(m_RotationSpeed, 0f);

            if (m_Mover == null)
            {
                m_Mover = GetComponent<CreatureMover>();
            }

            if (m_Mover != null)
            {
                m_Mover.SetSpeeds(m_WalkSpeed, m_RotationSpeed);
            }
        }

        private void ApplyMovement()
        {
            var axis = Vector2.zero;
            var target = transform.position;
            var isRun = false;

            if (m_State == State.Wander || m_State == State.Return)
            {
                if (m_HasTarget)
                {
                    axis = Vector2.up;
                    target = m_TargetPosition;
                    isRun = false;
                }
            }
            else if (m_State == State.Chase)
            {
                if (m_TargetTransform != null)
                {
                    axis = Vector2.up;
                    target = m_TargetTransform.position;
                    isRun = true;
                }
                else if (m_HasTarget)
                {
                    axis = Vector2.up;
                    target = m_TargetPosition;
                    isRun = true;
                }
            }
            else if (m_State == State.Eat)
            {
                if (Vector3.Distance(transform.position, m_TargetPosition) > 0.25f)
                {
                    axis = Vector2.up;
                    target = m_TargetPosition;
                }
            }
            else if (m_State == State.Detect || m_State == State.Attack)
            {
                if (m_TargetTransform != null)
                {
                    target = m_TargetTransform.position;
                }
                else if (m_HasTarget)
                {
                    target = m_TargetPosition;
                }
            }

            m_Mover.SetInput(axis, target, isRun, false);
        }

        private Transform FindNearestTarget()
        {
            if (m_DetectionRadius <= 0f)
            {
                return null;
            }

            var hitCount = Physics.OverlapSphereNonAlloc(transform.position, m_DetectionRadius, m_OverlapBuffer, m_TargetLayerMask, QueryTriggerInteraction.Collide);
            Transform nearest = null;
            var nearestSqr = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var collider = m_OverlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                var candidate = collider.transform.root;
                if (candidate == transform.root)
                {
                    continue;
                }

                if (!IsTargetName(candidate.name))
                {
                    continue;
                }

                var distanceSqr = (candidate.position - transform.position).sqrMagnitude;
                if (distanceSqr >= nearestSqr)
                {
                    continue;
                }

                nearestSqr = distanceSqr;
                nearest = candidate;
            }

            return nearest;
        }

        private bool IsTargetName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (var keyword in m_TargetNameKeywords)
            {
                if (name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTargetValid(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            return IsTargetName(target.name);
        }

        private bool IsWithinDetectionRadius(Vector3 position)
        {
            return (position - transform.position).sqrMagnitude <= m_DetectionRadius * m_DetectionRadius;
        }

        private bool IsWithinArea(Vector3 point)
        {
            if (m_AreaRadius <= 0f)
            {
                return true;
            }

            return Vector3.Distance(point, GetAreaCenter()) <= m_AreaRadius;
        }

        private bool IsOutsideArea()
        {
            if (m_AreaRadius <= 0f)
            {
                return false;
            }

            return Vector3.Distance(transform.position, GetAreaCenter()) > m_AreaRadius;
        }

        private bool HasArrived()
        {
            return m_HasTarget && Vector3.Distance(transform.position, m_TargetPosition) <= 0.5f;
        }

        private Vector3 GetAreaCenter()
        {
            return m_AreaCenter != null ? m_AreaCenter.position : transform.position;
        }

        private void PickWanderTarget()
        {
            var center = GetAreaCenter();
            var sample = UnityEngine.Random.insideUnitSphere * m_WanderRadius;
            sample.y = 0f;

            m_TargetPosition = center + sample;
            m_TargetPosition = ClampToArea(m_TargetPosition, center);
            m_HasTarget = true;
            m_State = State.Wander;
        }

        private void SetDetect(Transform target)
        {
            m_TargetTransform = target;
            m_TargetPosition = target.position;
            m_HasTarget = false;
            m_StateTimer = 0.5f;
            m_State = State.Detect;
        }

        private void SetChase()
        {
            m_State = State.Chase;
            m_HasTarget = true;
        }

        private void SetAttack()
        {
            m_State = State.Attack;
            m_HasTarget = false;
            m_StateTimer = m_AttackDelay;
        }

        private void ResolveAttack()
        {
            if (m_TargetTransform != null)
            {
                m_EatPosition = m_TargetTransform.position;
                Destroy(m_TargetTransform.gameObject);
                m_TargetTransform = null;
            }
            else
            {
                m_EatPosition = transform.position;
            }

            SetEat();
        }

        private void SetEat()
        {
            m_State = State.Eat;
            m_TargetPosition = m_EatPosition;
            m_HasTarget = true;
            m_StateTimer = m_EatingTime;
        }

        private void SetReturn()
        {
            m_State = State.Return;
            m_TargetPosition = GetAreaCenter();
            m_HasTarget = true;
        }

        private void SetIdle()
        {
            m_State = State.Idle;
            m_HasTarget = false;
            m_StateTimer = UnityEngine.Random.Range(m_IdleTimeMin, m_IdleTimeMax);
            m_TargetPosition = transform.position;
            m_TargetTransform = null;
        }

        private Vector3 ClampToArea(Vector3 point, Vector3 center)
        {
            if (m_AreaRadius <= 0f)
            {
                return point;
            }

            var offset = point - center;
            if (offset.magnitude <= m_AreaRadius)
            {
                return point;
            }

            offset = offset.normalized * m_AreaRadius;
            point = center + offset;
            point.y = transform.position.y;
            return point;
        }

        private void OnDrawGizmosSelected()
        {
            var center = GetAreaCenter();

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center, m_WanderRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_DetectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_AttackRadius);
        }
    }
}
