using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using System;
using Messenging;

namespace EcsAI {

  [UpdateAfter(typeof(AIRootInitializeSystem))]
  public class AINodeSelectSystem : JobComponentSystem {
    // protected BeginInitializationEntityCommandBufferSystem commandBufferSystem;

    [BurstCompile]
    struct MoveForwardJob : IJobForEach<AINodeStatus, AINodeMoveForward> {
      public void Execute(ref AINodeStatus status, [ReadOnly] ref AINodeMoveForward node) {
        if (status.State == NodeState.Success) {
          return;
        }

        // no conditions to start
        status.State = NodeState.Ready;
      }
    }

    [BurstCompile]
    struct DestroyGameObjectJob : IJobForEach<AINodeStatus, AINodeDestroyGameObject> {
      public void Execute(ref AINodeStatus status, [ReadOnly] ref AINodeDestroyGameObject node) {
        status.State = NodeState.Ready;
      }
    }

    [BurstCompile]
    struct TurnJob : IJobForEach<AINodeStatus, AINodeTurn> {
      public void Execute(ref AINodeStatus status, [ReadOnly] ref AINodeTurn node) {
        if (status.State == NodeState.Success) {
          return;
        }

        // no conditions to start
        status.State = NodeState.Ready;
      }
    }

    [BurstCompile]
    struct ChaseJob : IJobForEach<AINodeStatus, AINodeChase> {
      public void Execute(ref AINodeStatus status, [ReadOnly] ref AINodeChase node) {
        if (status.State == NodeState.Success) {
          return;
        }

        // no conditions to start
        status.State = NodeState.Ready;
      }
    }

    protected override JobHandle OnUpdate(JobHandle handle) {
      var moveForwardJob = new MoveForwardJob();
      var destroyGameObjectJob = new DestroyGameObjectJob();
      var turnJob = new TurnJob();
      var chaseJob = new ChaseJob();

      handle = moveForwardJob.Schedule(this, handle);
      handle = destroyGameObjectJob.Schedule(this, handle);
      handle = turnJob.Schedule(this, handle);
      handle = chaseJob.Schedule(this, handle);

      return handle;
    }
  }

}
