using Unity.Entities;
using System;

namespace EcsAI {

  public struct AINodeSequence : IComponentData {}

  [Serializable]
  public class Sequence {
    public AINodeSequence CreateComponentData(AINodeStatus nodeStatus) {
      return new AINodeSequence();
    }
  }

}
