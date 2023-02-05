using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Modifier", menuName = "ScriptableObjects/Modifiers", order = 1)]
public class Modifier : ScriptableObject
{
    public List<Stat> Changes;

    [Serializable]
    public struct Stat{
        public enum Modifies{ Accel, CoolDown, Impact, Steering, Jump, Swim_Speed};
        public Modifies to;

        public float value;
    }

    public static string DisplayStat(Stat stat)
    {
        switch (stat.to)
        {
            case Stat.Modifies.Accel:
                if (stat.value > 0)
                    return $"Your Acceleration is increased by {stat.value}";
                else
                    return $"Your Acceleration is decreased by {-stat.value}";
            case Stat.Modifies.CoolDown:
                if (stat.value > 0)
                    return $"Your Boost recharges {stat.value}% faster";
                else
                    return $"Your Boost recharges {-stat.value}% slower";
            case Stat.Modifies.Impact:
                if (stat.value > 0)
                    return $"Your Impact is increased by {stat.value}";
                else
                    return $"Your Impact is decreased by {-stat.value}";
            case Stat.Modifies.Steering:
                if (stat.value > 0)
                    return $"Your Steering is increased by {stat.value}";
                else
                    return $"Your Steering is decreased by {-stat.value}";
            case Stat.Modifies.Jump:
                if (stat.value > 0)
                    return $"Your Jump Force is increased by {stat.value}";
                else
                    return $"Van Jump Force is decreased by {-stat.value}";
            case Stat.Modifies.Swim_Speed:
                if (stat.value > 0)
                    return $"Van Swim Speed is increased by {stat.value}";
                else
                    return $"Van Swim Speed is decreased by {-stat.value}";
            default:
                return "NULL";
        }
    }

    public void Apply(GameObject m_player)
    {
        Car car = m_player.GetComponent<Car>();

        car.ApplyStat(Changes[0].to, Changes[0].value);
        if(Changes.Count > 1)
            car.ApplyStat(Changes[1].to, -Changes[1].value);
    }
}
