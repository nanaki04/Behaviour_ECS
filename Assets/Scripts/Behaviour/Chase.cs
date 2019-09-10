using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System;

namespace EcsAI {

  public struct AINodeChase : IComponentData {
    public int TargetId;
    public float SpeedBoost;
    public float MinimumDistance;
  }

  [Serializable]
  public class Chase {

    [SerializeField]
    private int targetId = 0; // TODO find nearest, or use some kind of target tag

    [SerializeField]
    private float speedBoost = 0; // TODO use buffs

    [SerializeField]
    private float minimumDistance = 0;

    public AINodeChase CreateComponentData(AINodeStatus nodeStatus) {
      return new AINodeChase {
        TargetId = targetId,
        SpeedBoost = speedBoost,
        MinimumDistance = minimumDistance,
      };
    }
  }

}
