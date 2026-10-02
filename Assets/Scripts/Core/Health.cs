using System;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class Health : MonoBehaviour
    {
        public event Action<Health> Changed;
        public event Action<Health> Died;

        public float Maximum { get; private set; } = 100f;
        public float Current { get; private set; } = 100f;
        public bool IsDead => Current <= 0f;

        public void ResetHealth(float maximum)
        {
            Maximum = Mathf.Max(1f, maximum);
            Current = Maximum;
            Changed?.Invoke(this);
        }

        public void Damage(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            Current = Mathf.Max(0f, Current - amount);
            Changed?.Invoke(this);
            if (IsDead)
            {
                Died?.Invoke(this);
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            Current = Mathf.Min(Maximum, Current + amount);
            Changed?.Invoke(this);
        }
    }
}

