using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;
using Messenging;

namespace EcsAI {

  public struct AIRoot : IComponentData {
    public int TreeId;
    public NodeType RunningNodeType;
    public int RunningNodeId;
    public bool LoopOnOk;
    public bool LoopOnFailure;
    public NodeState State;
  }

  public struct AIRootInitializer : IComponentData {
    public int TreeId;
  }

  public struct TreeId : ISharedComponentData {
    public int Id;
  }

  public class AIRootProxy : MonoBehaviour, IConvertGameObjectToEntity {

    public static Dictionary<int, List<EditorNode>> AINodePrototypes = new Dictionary<int, List<EditorNode>>();

    [SerializeField]
    private List<EditorNode> nodes = new List<EditorNode>();

    [SerializeField]
    private bool loopOnOk = false;

    [SerializeField]
    private bool loopOnFailure = false;

    [SerializeField]
    private AddressProxy address;

    public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem) {
//      foreach (EditorNode node in nodes) {
//        node.UpdateParentId(nodes);
//      }
//
//      foreach (EditorNode node in nodes) {
//        node.CreateEntity(entityManager, address.Id);
//      }

      if (!AINodePrototypes.ContainsKey(address.Id)) {
        AINodePrototypes.Add(address.Id, nodes);
      }

      var root = new AIRoot {
        TreeId = address.Id,
        RunningNodeType = NodeType.Root,
        RunningNodeId = 0,
        LoopOnOk = loopOnOk,
        LoopOnFailure = loopOnFailure,
        State = NodeState.Selecting,
      };

      var initializer = new AIRootInitializer {
        TreeId = address.Id,
      };

      entityManager.AddComponentData(entity, root);
      entityManager.AddComponentData(entity, initializer);
    }
  }

}
