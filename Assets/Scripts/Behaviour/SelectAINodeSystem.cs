using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Messenging;
using EcsMessages;

namespace EcsAI {

  [UpdateAfter(typeof(AINodeRunSystem))]
  class SelectAINodeSystem : MessageSystem<SelectAINodePayload> {
    EntityQuery rootQuery;
    EntityQuery moveForwardQuery;
    EntityQuery destroyGameObjectQuery;
    EntityQuery turnQuery;
    EntityQuery chaseQuery;

    protected override void Init() {
      rootQuery = GetEntityQuery(typeof(TreeId), typeof(Address), typeof(AIRoot));
      moveForwardQuery = GetEntityQuery(typeof(TreeId), typeof(AINodeMoveForward), typeof(AINodeStatus));
      destroyGameObjectQuery = GetEntityQuery(typeof(TreeId), typeof(AINodeDestroyGameObject), typeof(AINodeStatus));
      turnQuery = GetEntityQuery(typeof(TreeId), typeof(AINodeTurn), typeof(AINodeStatus));
      chaseQuery = GetEntityQuery(typeof(TreeId), typeof(AINodeChase), typeof(AINodeStatus));
    }

    protected override JobHandle HandleMessage(JobHandle handle, EntityCommandBuffer commandBuffer) {
      var receivers = Receivers;
      foreach (Receiver receiver in receivers) {
        var treeId = new TreeId { Id = receiver.Id };
        rootQuery.SetFilter(treeId);

        var roots = rootQuery.ToEntityArray(Allocator.TempJob);
        if (roots.Length == 0) {
          continue;
        }
        var root = roots[0];


        moveForwardQuery.SetFilter(treeId);

        var nodeStatus = moveForwardQuery.ToComponentDataArray<AINodeStatus>(Allocator.TempJob);
        var moveForwardStatus = moveForwardQuery.ToComponentDataArray<AINodeMoveForward>(Allocator.TempJob);
        var moveForwardNodes = moveForwardQuery.ToEntityArray(Allocator.TempJob);

        for (int x = 0; x < nodeStatus.Length; x++) {
          // MEMO: this may not be deterministic, so replays may be off
          if (nodeStatus[x].State != NodeState.Running) {
            continue;
          }

          EntityManager.AddComponentData(root, nodeStatus[x]);
          EntityManager.AddComponentData(root, moveForwardStatus[x]);
          EntityManager.DestroyEntity(moveForwardNodes[x]);
          break;
        }

        nodeStatus.Dispose();
        moveForwardStatus.Dispose();
        moveForwardNodes.Dispose();




        destroyGameObjectQuery.SetFilter(treeId);

        nodeStatus = destroyGameObjectQuery.ToComponentDataArray<AINodeStatus>(Allocator.TempJob);
        var destroyGameObjectStatus = destroyGameObjectQuery.ToComponentDataArray<AINodeDestroyGameObject>(Allocator.TempJob);
        var destroyGameObjectNodes = destroyGameObjectQuery.ToEntityArray(Allocator.TempJob);

        for (int x = 0; x < nodeStatus.Length; x++) {
          if (nodeStatus[x].State != NodeState.Running) {
            continue;
          }

          EntityManager.AddComponentData(root, nodeStatus[x]);
          EntityManager.AddComponentData(root, destroyGameObjectStatus[x]);
          EntityManager.DestroyEntity(destroyGameObjectNodes[x]);
          break;
        }

        nodeStatus.Dispose();
        destroyGameObjectStatus.Dispose();
        destroyGameObjectNodes.Dispose();






        turnQuery.SetFilter(treeId);

        nodeStatus = turnQuery.ToComponentDataArray<AINodeStatus>(Allocator.TempJob);
        var turnStatus = turnQuery.ToComponentDataArray<AINodeTurn>(Allocator.TempJob);
        var turnNodes = turnQuery.ToEntityArray(Allocator.TempJob);

        for (int x = 0; x < nodeStatus.Length; x++) {
          if (nodeStatus[x].State != NodeState.Running) {
            continue;
          }

          EntityManager.AddComponentData(root, nodeStatus[x]);
          EntityManager.AddComponentData(root, turnStatus[x]);
          EntityManager.DestroyEntity(turnNodes[x]);
          break;
        }

        nodeStatus.Dispose();
        turnStatus.Dispose();
        turnNodes.Dispose();



        chaseQuery.SetFilter(treeId);

        nodeStatus = chaseQuery.ToComponentDataArray<AINodeStatus>(Allocator.TempJob);
        var chaseStatus = chaseQuery.ToComponentDataArray<AINodeChase>(Allocator.TempJob);
        var chaseNodes = chaseQuery.ToEntityArray(Allocator.TempJob);

        for (int x = 0; x < nodeStatus.Length; x++) {
          if (nodeStatus[x].State != NodeState.Running) {
            continue;
          }

          EntityManager.AddComponentData(root, nodeStatus[x]);
          EntityManager.AddComponentData(root, chaseStatus[x]);
          EntityManager.DestroyEntity(chaseNodes[x]);
          break;
        }

        nodeStatus.Dispose();
        chaseStatus.Dispose();
        chaseNodes.Dispose();





    





        roots.Dispose();
      }
      receivers.Dispose();

      return handle;
    }
  }

}

