using Unity.Entities;
using UnityEngine;
using System;

namespace EcsAI {

  public struct AINodeMoveForward : IComponentData {
    public float Distance;
    public float DistanceLeft;
  }

  [Serializable]
  public class MoveForward {

    [SerializeField]
    private float distance = 100.0f;

    public AINodeMoveForward CreateComponentData(AINodeStatus nodeStatus) {
      return new AINodeMoveForward {
        Distance = distance,
        DistanceLeft = distance,
      };
    }
  }

}
