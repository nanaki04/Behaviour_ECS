using Unity.Entities;

namespace EcsAI {

  public struct AINodeStatus : IComponentData {
    public NodeType Type;
    public NodeState State;
    public int Id;
    public int ParentId;
    public int TreeId;
  }

}
