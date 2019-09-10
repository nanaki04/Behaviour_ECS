using Unity.Entities;
using System;

namespace EcsAI {

  public struct AINodeSelector : IComponentData {}

  [Serializable]
  public class Selector {
    public AINodeSelector CreateComponentData(AINodeStatus nodeStatus) {
      return new AINodeSelector();
    }
  }

}
