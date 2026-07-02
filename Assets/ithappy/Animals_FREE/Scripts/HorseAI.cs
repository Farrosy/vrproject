using System;
using UnityEngine;

namespace ithappy.Animals_FREE
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CreatureMover))]
    [DisallowMultipleComponent]
    public class HorseAI : MonoBehaviour
    {
        [Header("Wander")]
        [SerializeField, Min(0f)]
        private float m_WanderRadius = 5f;

        [SerializeField, Min(0f)]
        private float m_AreaRadius = 10f;

        [SerializeField]
        private Transform m_AreaCenter;

        [SerializeField, Min(0f)]
        private float m_IdleDurationMin = 2f;

        [SerializeField, Min(0f)]
        private float m_IdleDurationMax = 5f;

        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float m_WalkSpeed = 1f;

        [SerializeField, Min(0f)]
        private float m_RotateSpeed = 245f;

        [Header("Food")]
        [SerializeField, Min(0f)]
        private float m_FoodDetectionRadius = 5f;

        [SerializeField, Min(0f)]
        private float m_EatingDistance = 1f;

        [SerializeField, Min(0f)]
        private float m_EatingDuration = 3f;

        [SerializeField]
        private LayerMask m_FoodLayerMask = Physics.DefaultRaycastLayers;

        [SerializeField]
        private string[] m_FoodNameKeywords = new[] { "Hay", "Feeder", "Food" };

        private CreatureMover m_Mover;
        private Vector3 m_TargetPosition;
        private Transform m_FoodTarget;
        private float m_StateTimer;
        private bool m_HasTarget;
        private Collider[] m_OverlapBuffer = new Collider[32];
        private State m_State;

        private enum State
        {
            Idle,
            Wander,
            Move,
            MoveToFood,
            Eat
        }

        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
            m_Mover.SetSpeeds(m_WalkSpeed, m_RotateSpeed);
            SetIdle();
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;

            if (m_State == State.Eat)
            {
                m_StateTimer -= deltaTime;
                if (m_StateTimer <= 0f)
                {
                    m_FoodTarget = null;
                    SetIdle();
                }
            }
            else
            {
                var nearestFood = FindNearestFood();
                if (nearestFood != null)
                {
                    if (m_State != State.Eat && m_State != State.MoveToFood)
                    {
                        SetMoveToFood(nearestFood);
                    }
                }

                if (m_State == State.MoveToFood)
                {
                    if (m_FoodTarget == null)
                    {
                        SetIdle();
                    }
                    else if (Vector3.Distance(transform.position, m_TargetPosition) <= m_EatingDistance)
                    {
                        StartEating();
                    }
                    else if (IsOutsideArea())
                    {
                        SetIdle();
                    }
                }
                else if (m_State == State.Move)
                {
                    if (HasArrived() || IsOutsideArea())
                    {
                        SetIdle();
                    }
                }
                else if (m_State == State.Idle)
                {
                    m_StateTimer -= deltaTime;
                    if (m_StateTimer <= 0f)
                    {
                        PickWanderTarget();
                    }
                }
            }

            ApplyMovement();
        }

        private void OnValidate()
        {
            m_WanderRadius = Mathf.Max(m_WanderRadius, 0f);
            m_AreaRadius = Mathf.Max(m_AreaRadius, 0f);
            m_IdleDurationMin = Mathf.Max(m_IdleDurationMin, 0f);
            m_IdleDurationMax = Mathf.Max(m_IdleDurationMax, m_IdleDurationMin);
            m_FoodDetectionRadius = Mathf.Max(m_FoodDetectionRadius, 0f);
            m_EatingDistance = Mathf.Max(m_EatingDistance, 0.01f);
            m_EatingDuration = Mathf.Max(m_EatingDuration, 0f);
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RotateSpeed = Mathf.Max(m_RotateSpeed, 0f);

            if (m_Mover == null)
            {
                m_Mover = GetComponent<CreatureMover>();
            }

            if (m_Mover != null)
            {
                m_Mover.SetSpeeds(m_WalkSpeed, m_RotateSpeed);
            }
        }

        private Vector3 GetAreaCenter()
        {
            if (m_AreaCenter != null)
            {
                return m_AreaCenter.position;
            }

            return transform.position;
        }

        private void PickWanderTarget()
        {
            var center = GetAreaCenter();
            var sample = UnityEngine.Random.insideUnitSphere * m_WanderRadius;
            sample.y = 0f;
            m_TargetPosition = center + sample;
            m_TargetPosition = ClampToArea(m_TargetPosition, center);
            m_HasTarget = true;
            m_State = State.Move;
        }

        private void SetMoveToFood(Transform food)
        {
            m_FoodTarget = food;
            m_TargetPosition = ClampToArea(food.position, GetAreaCenter());
            m_HasTarget = true;
            m_State = State.MoveToFood;
        }

        private void StartEating()
        {
            m_State = State.Eat;
            m_StateTimer = m_EatingDuration;
            if (m_FoodTarget != null)
            {
                m_TargetPosition = m_FoodTarget.position;
            }
        }

        private bool HasArrived()
        {
            return m_HasTarget && Vector3.Distance(transform.position, m_TargetPosition) <= m_EatingDistance;
        }

        private bool IsOutsideArea()
        {
            if (m_AreaRadius <= 0f)
            {
                return false;
            }

            var center = GetAreaCenter();
            return Vector3.Distance(transform.position, center) > m_AreaRadius;
        }

        private bool IsWithinArea(Vector3 point)
        {
            if (m_AreaRadius <= 0f)
            {
                return true;
            }

            return Vector3.Distance(point, GetAreaCenter()) <= m_AreaRadius;
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

        private void SetIdle()
        {
            m_State = State.Idle;
            m_HasTarget = false;
            m_StateTimer = UnityEngine.Random.Range(m_IdleDurationMin, m_IdleDurationMax);
            m_TargetPosition = transform.position;
            m_FoodTarget = null;
        }

        private void ApplyMovement()
        {
            var axis = Vector2.zero;
            var target = transform.position;
            var isRun = false;

            if ((m_State == State.Move || m_State == State.MoveToFood) && m_HasTarget)
            {
                axis = Vector2.up;
                target = m_TargetPosition;
            }
            else if (m_State == State.Eat && m_FoodTarget != null)
            {
                target = m_FoodTarget.position;
            }

            m_Mover.SetInput(axis, target, isRun, false);
        }

        private Transform FindNearestFood()
        {
            if (m_FoodDetectionRadius <= 0f)
            {
                return null;
            }

            var count = Physics.OverlapSphereNonAlloc(transform.position, m_FoodDetectionRadius, m_OverlapBuffer, m_FoodLayerMask, QueryTriggerInteraction.Collide);
            Transform nearest = null;
            var nearestDistance = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var collider = m_OverlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                var candidate = collider.transform;
                if (candidate == transform)
                {
                    continue;
                }

                if (!IsFoodSource(candidate.gameObject.name))
                {
                    continue;
                }

                if (!IsWithinArea(candidate.position))
                {
                    continue;
                }

                var distance = Vector3.SqrMagnitude(candidate.position - transform.position);
                if (distance >= nearestDistance)
                {
                    continue;
                }

                nearestDistance = distance;
                nearest = candidate;
            }

            return nearest;
        }

        private bool IsFoodSource(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (var keyword in m_FoodNameKeywords)
            {
                if (name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            var center = GetAreaCenter();

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center, m_WanderRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_FoodDetectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_EatingDistance);

            if (m_HasTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, m_TargetPosition);
                Gizmos.DrawSphere(m_TargetPosition, 0.1f);
            }
        }
    }
}
