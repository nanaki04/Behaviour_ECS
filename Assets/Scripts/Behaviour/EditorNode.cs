using UnityEngine;
using Unity.Entities;
using System;
using System.Collections.Generic;

namespace EcsAI {

  [Serializable]
  public class EditorNode {
    public NodeType Type = NodeType.Selector;

    // TODO add specific node data
    public Selector Selector;
    public Sequence Sequence;
    public MoveForward MoveForward;
    public DestroyGameObject DestroyGameObject;
    public Turn Turn;
    public Chase Chase;

    public int Index;
    public List<int> ChildIndices = new List<int>();
    public int ParentId;

    public void UpdateParentId(List<EditorNode> nodes) {
      foreach (int childIndex in ChildIndices) {
        nodes[childIndex].ParentId = Index;
      }
    }

    public void CreateEntity(EntityCommandBuffer commandBuffer, int treeId) {
      var entity = commandBuffer.CreateEntity();
      var nodeStatus = new AINodeStatus {
        Type = Type,
        State = NodeState.Selecting,
        Id = Index,
        ParentId = ParentId,
        TreeId = treeId,
      };

      commandBuffer.AddComponent(entity, nodeStatus);
      commandBuffer.AddSharedComponent(entity, new TreeId { Id = treeId });

      // TODO add specific node data
      switch (Type) {
        case NodeType.Selector:
          commandBuffer.AddComponent(entity, Selector.CreateComponentData(nodeStatus));
          break;
        case NodeType.Sequence:
          commandBuffer.AddComponent(entity, Sequence.CreateComponentData(nodeStatus));
          break;
        case NodeType.MoveForward:
          commandBuffer.AddComponent(entity, MoveForward.CreateComponentData(nodeStatus));
          break;
        case NodeType.DestroyGameObject:
          commandBuffer.AddComponent(entity, DestroyGameObject.CreateComponentData(nodeStatus));
          break;
        case NodeType.Turn:
          commandBuffer.AddComponent(entity, Turn.CreateComponentData(nodeStatus));
          break;
        case NodeType.Chase:
          commandBuffer.AddComponent(entity, Chase.CreateComponentData(nodeStatus));
          break;
        default:
          break;
      }
    }
  }

}
