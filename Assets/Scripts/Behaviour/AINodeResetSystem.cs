using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Messenging;

namespace EcsAI {

  [UpdateAfter(typeof(AINodeRunSystem))]
  class AINodeResetSystem : JobComponentSystem {
    EntityQuery rootQuery;
    EntityQuery nodeStatusQuery;
    EntityQuery moveForwardQuery;
    EntityQuery turnQuery;
    List<TreeId> trees = new List<TreeId>();

    struct ResetNodeStatusJob : IJobForEach<AINodeStatus> {
      public void Execute(ref AINodeStatus status) {
        status.State = NodeState.Selecting;
      }
    }

    struct ResetMoveForwardJob : IJobForEach<AINodeMoveForward> {
      public void Execute(ref AINodeMoveForward node) {
        node.DistanceLeft = node.Distance;
      }
    }

    struct ResetTurnJob : IJobForEach<AINodeTurn> {
      public void Execute(ref AINodeTurn node) {
        node.RotationRemaining = node.Rotation;
      }
    }

    protected override void OnCreate() {
      rootQuery = GetEntityQuery(typeof(TreeId), typeof(AIRoot));
      nodeStatusQuery = GetEntityQuery(typeof(TreeId), typeof(AINodeStatus));
      moveForwardQuery = GetEntityQuery(typeof(TreeId), typeof(AINodeMoveForward));
      turnQuery = GetEntityQuery(typeof(TreeId), typeof(AINodeTurn));
    }

    protected override JobHandle OnUpdate(JobHandle handle) {
      EntityManager.GetAllUniqueSharedComponentData(trees);

      for (int x = 0; x < trees.Count; x++) {
        var treeId = trees[x];
        rootQuery.SetFilter(treeId);

        var roots = rootQuery.ToComponentDataArray<AIRoot>(Allocator.TempJob);
        if (roots.Length <= 0) {
          roots.Dispose();
          continue;
        }
        var root = roots[0];
        if (root.State != NodeState.Success && root.State != NodeState.Failure) {
          roots.Dispose();
          continue;
        }

        if (root.State == NodeState.Success && !root.LoopOnOk) {
          roots.Dispose();
          continue;
        }

        if (root.State == NodeState.Failure && !root.LoopOnFailure) {
          roots.Dispose();
          continue;
        }

        nodeStatusQuery.SetFilter(treeId);
        moveForwardQuery.SetFilter(treeId);
        turnQuery.SetFilter(treeId);

        handle = (new ResetNodeStatusJob()).Schedule(nodeStatusQuery, handle);
        handle = (new ResetMoveForwardJob()).Schedule(moveForwardQuery, handle);
        handle = (new ResetTurnJob()).Schedule(turnQuery, handle);

        roots.Dispose();
      }

      trees.Clear();

      return handle;
    }
  }

}
