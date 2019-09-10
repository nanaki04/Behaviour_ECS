using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System;

namespace EcsAI {

  public struct AINodeTurn : IComponentData {
    public float3 TurnSpeed;
    public float3 Rotation;
    public float3 RotationRemaining;
  }

  [Serializable]
  public class Turn {

    [SerializeField]
    private Vector3 turnSpeed = new Vector3(0f, 10f, 0f);

    [SerializeField]
    private Vector3 rotation = new Vector3(0f, 90f, 0f);

    public AINodeTurn CreateComponentData(AINodeStatus nodeStatus) {
      return new AINodeTurn {
        TurnSpeed = Utility.Math.Vector3ToFloat3(turnSpeed),
        Rotation = Utility.Math.Vector3ToFloat3(rotation),
        RotationRemaining = Utility.Math.Vector3ToFloat3(rotation),
      };
    }
  }

}
