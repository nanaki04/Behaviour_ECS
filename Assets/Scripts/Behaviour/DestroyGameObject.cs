using Unity.Entities;
using UnityEngine;
using System;

namespace EcsAI {

  public struct AINodeDestroyGameObject : IComponentData {
    public bool Destroy;
  }

  [Serializable]
  public class DestroyGameObject {
    public AINodeDestroyGameObject CreateComponentData(AINodeStatus nodeStatus) {
      return new AINodeDestroyGameObject {
        Destroy = true, // gives error if empty
      };
    }
  }

}
