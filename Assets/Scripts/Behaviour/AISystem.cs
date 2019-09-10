using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Messenging;
using EcsMessages;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace EcsAI {

  [UpdateAfter(typeof(AINodeSelectSystem))]
  class AISystem : JobComponentSystem {
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;
    EntityQuery treeQuery;
    EntityQuery rootQuery;
    List<TreeId> trees = new List<TreeId>();

    protected override void OnCreate() {
      commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

      treeQuery = GetEntityQuery(new EntityQueryDesc {
        All = new [] { ComponentType.ReadOnly<TreeId>(), ComponentType.ReadWrite<AINodeStatus>() },
      });

      rootQuery = GetEntityQuery(new EntityQueryDesc {
        All = new [] { ComponentType.ReadOnly<TreeId>(), ComponentType.ReadWrite<AIRoot>() },
      });
    }

    protected override JobHandle OnUpdate(JobHandle handle) {
      var commandBuffer = commandBufferSystem.CreateCommandBuffer();
      EntityManager.GetAllUniqueSharedComponentData(trees);

      // TODO find a way to delegate some of the work to jobs
      for (int x = 0; x < trees.Count; x++) { // MEMO the first item will be default(TreeId), so ignore it // MEMO2 they seemed to have changed that
        var treeId = trees[x];
        treeQuery.SetFilter(treeId);
        rootQuery.SetFilter(treeId);

        var count = Mathf.Max(treeQuery.CalculateEntityCount(), 0);
        var nodeStatus = new NativeArray<AINodeStatus>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var entities = new NativeArray<Entity>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var unsortedNodeStatus = treeQuery.ToComponentDataArray<AINodeStatus>(Allocator.TempJob);
        var unsortedEntities = treeQuery.ToEntityArray(Allocator.TempJob);
        for (int y = 0; y < count; y++) {
          AINodeStatus status = unsortedNodeStatus[y];
          if (status.Id >= count) {
            continue;
          }
          Entity entity = unsortedEntities[y];
          nodeStatus[status.Id] = status;
          entities[status.Id] = entity;
        }
        unsortedNodeStatus.Dispose();
        unsortedEntities.Dispose();

        var roots = rootQuery.ToComponentDataArray<AIRoot>(Allocator.TempJob);
        var rootEntities = rootQuery.ToEntityArray(Allocator.TempJob);

        if (roots.Length <= 0) {
          nodeStatus.Dispose();
          entities.Dispose();
          roots.Dispose();
          rootEntities.Dispose();
          continue;
        }

        var root = roots[0];
        var rootEntity = rootEntities[0];
        roots.Dispose();
        rootEntities.Dispose();


        // Update NodeState for control nodes
        int lastControlNode = nodeStatus.Length - 1;
        AINodeStatus node;

        for (int y = nodeStatus.Length - 1; y >= 0; y--) {
          node = nodeStatus[y];
          if (node.Type == NodeType.Selector) {
            node.State = NodeState.Failure;
            for (int z = node.Id + 1; z <= lastControlNode; z++) {
              var child = nodeStatus[z];
              if (child.ParentId != node.Id) {
                continue;
              }
              if (child.State != NodeState.Failure) {
                node.State = child.State;
                break;
              }
            }
            EntityManager.SetComponentData<AINodeStatus>(entities[y], node);
            lastControlNode = node.Id;
          }

          if (node.Type == NodeType.Sequence) {
            node.State = NodeState.Success;
            for (int z = node.Id + 1; z <= lastControlNode; z++) {
              var child = nodeStatus[z];
              if (child.ParentId != node.Id) {
                continue;
              }
              if (child.State != NodeState.Success) {
                node.State = child.State;
                break;
              }
            }
            EntityManager.SetComponentData<AINodeStatus>(entities[y], node);
            lastControlNode = node.Id;
          }

        }

        // Find node to run, and update root
        int idx = 0;
        int parentId = 0;
        node = nodeStatus[idx];
        if (node.State != NodeState.Ready) {
          root.State = node.State;
          root.RunningNodeType = NodeType.Root;
          root.RunningNodeId = idx;
          EntityManager.SetComponentData<AIRoot>(rootEntity, root);
          nodeStatus.Dispose();
          entities.Dispose();
          continue;
        }

        for (; idx < nodeStatus.Length; idx++) {
          node = nodeStatus[idx];
          if (node.ParentId != parentId) {
            continue;
          }

          if (node.State != NodeState.Ready) {
            continue;
          }

          if (node.Type == NodeType.Selector || node.Type == NodeType.Sequence) {
            parentId = idx;
            continue;
          }

          node.State = NodeState.Running;
          EntityManager.SetComponentData<AINodeStatus>(entities[idx], node);

          if (root.RunningNodeType == node.Type && root.RunningNodeId == node.Id) {
            break;
          }

          root.RunningNodeType = node.Type;
          root.RunningNodeId = node.Id;
          root.State = NodeState.Running;
          EntityManager.SetComponentData<AIRoot>(rootEntity, root);

          var selectNodeMessage = new SelectAINodeMessage {
            From = node.TreeId,
            To = node.TreeId,
            Payload = new SelectAINodePayload {},
          };
          selectNodeMessage.Send(commandBuffer);

          break;
        }

        nodeStatus.Dispose();
        entities.Dispose();
      }

      trees.Clear();
      return handle;
    }
  }

}
