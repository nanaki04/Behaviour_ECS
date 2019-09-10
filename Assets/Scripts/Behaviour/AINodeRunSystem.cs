using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using System;
using Messenging;

namespace EcsAI {

  [UpdateBefore(typeof(AIRootInitializeSystem))]
  public class AINodeRunSystem : JobComponentSystem {
    BeginInitializationEntityCommandBufferSystem commandBufferSystem;
    EntityQuery moveForwardQuery;
    EntityQuery destroyGameObjectQuery;
    EntityQuery turnQuery;
    EntityQuery chaseQuery;
    EntityQuery nodeQuery;
    EntityQuery targetPositionQuery;

    protected override void OnCreate() {
      commandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
      nodeQuery = GetEntityQuery(typeof(TreeId), typeof(AINodeStatus));
      moveForwardQuery = GetEntityQuery(typeof(AINodeStatus), typeof(AINodeMoveForward), typeof(Translation), typeof(Rotation));
      turnQuery = GetEntityQuery(typeof(AINodeStatus), typeof(AINodeTurn), typeof(Rotation));
      chaseQuery = GetEntityQuery(typeof(AINodeStatus), typeof(AINodeChase), typeof(Rotation), typeof(Translation));
      destroyGameObjectQuery = GetEntityQuery(typeof(AIRoot), typeof(AINodeStatus), typeof(AINodeDestroyGameObject));
      targetPositionQuery = GetEntityQuery(typeof(Address), typeof(Translation));
    }

    protected override JobHandle OnUpdate(JobHandle handle) {
      var commandBuffer = commandBufferSystem.CreateCommandBuffer();




      var rootEntities = moveForwardQuery.ToEntityArray(Allocator.TempJob);
      var nodeStatus = moveForwardQuery.ToComponentDataArray<AINodeStatus>(Allocator.TempJob);
      var moveForwardStatus = moveForwardQuery.ToComponentDataArray<AINodeMoveForward>(Allocator.TempJob);
      var translations = moveForwardQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
      var rotations = moveForwardQuery.ToComponentDataArray<Rotation>(Allocator.TempJob);
      var deltaTime = Time.deltaTime;

      for (int i = 0; i < moveForwardQuery.CalculateEntityCount(); i++) {
        var root = rootEntities[i];
        if (nodeStatus[i].State == NodeState.Running) {
          var frameDistance = deltaTime;
          var node = nodeStatus[i];
          var moveForward = moveForwardStatus[i];
          var translation = translations[i];

          if (frameDistance > moveForward.DistanceLeft) {
            frameDistance = moveForward.DistanceLeft;
            node.State = NodeState.Success;
          }
          moveForward.DistanceLeft -= frameDistance;
          translation.Value += math.rotate(rotations[i].Value, new float3(0f, 0f, frameDistance));

          EntityManager.SetComponentData(root, node);
          EntityManager.SetComponentData(root, moveForward);
          EntityManager.SetComponentData(root, translation);
          continue;
        }

        var nodeEntity = EntityManager.CreateEntity();
        EntityManager.AddSharedComponentData(nodeEntity, new TreeId { Id = nodeStatus[i].TreeId });
        EntityManager.AddComponentData(nodeEntity, moveForwardStatus[i]);
        EntityManager.AddComponentData(nodeEntity, nodeStatus[i]);
        EntityManager.RemoveComponent(rootEntities[i], typeof(AINodeMoveForward));
        EntityManager.RemoveComponent(rootEntities[i], typeof(AINodeStatus));
      }
      rootEntities.Dispose();
      nodeStatus.Dispose();
      moveForwardStatus.Dispose();
      translations.Dispose();
      rotations.Dispose();






      rootEntities = destroyGameObjectQuery.ToEntityArray(Allocator.TempJob);
      var roots = destroyGameObjectQuery.ToComponentDataArray<AIRoot>(Allocator.TempJob);
      for (int i = 0; i < destroyGameObjectQuery.CalculateEntityCount(); i++) {
        var treeId = roots[i].TreeId;
        nodeQuery.SetFilter(new TreeId { Id = treeId });
        var nodeEntities = nodeQuery.ToEntityArray(Allocator.TempJob);
        foreach (Entity nodeEntity in nodeEntities) {
          EntityManager.DestroyEntity(nodeEntity);
        }
        EntityManager.DestroyEntity(rootEntities[i]);
        nodeEntities.Dispose();
      }
      rootEntities.Dispose();
      roots.Dispose();







      rootEntities = turnQuery.ToEntityArray(Allocator.TempJob);
      nodeStatus = turnQuery.ToComponentDataArray<AINodeStatus>(Allocator.TempJob);
      var turnStatus = turnQuery.ToComponentDataArray<AINodeTurn>(Allocator.TempJob);
      rotations = turnQuery.ToComponentDataArray<Rotation>(Allocator.TempJob);

      for (int i = 0; i < turnQuery.CalculateEntityCount(); i++) {
        var root = rootEntities[i];
        var node = nodeStatus[i];
        var turn = turnStatus[i];
        var rotation = rotations[i];
        var rotationRemaining = turn.RotationRemaining;

        if (node.State == NodeState.Running) {
          var rotationX = turn.TurnSpeed.x * deltaTime;
          if (turn.TurnSpeed.x < 0 && rotationX < rotationRemaining.x) {
            rotationX = rotationRemaining.x;
          } else if (turn.TurnSpeed.x > 0 && rotationX > rotationRemaining.x) {
            rotationX = rotationRemaining.x;
          }

          var rotationY = turn.TurnSpeed.y * deltaTime;
          if (turn.TurnSpeed.y < 0 && rotationY < rotationRemaining.y) {
            rotationX = rotationRemaining.x;
          } else if (turn.TurnSpeed.y > 0 && rotationY > rotationRemaining.y) {
            rotationY = rotationRemaining.y;
          }

          var rotationZ = turn.TurnSpeed.z * deltaTime;
          if (turn.TurnSpeed.z < 0 && rotationZ < rotationRemaining.z) {
            rotationZ = rotationRemaining.z;
          } else if (turn.TurnSpeed.z > 0 && rotationZ > rotationRemaining.z) {
            rotationZ = rotationRemaining.z;
          }

          var frameRotation = new float3(rotationX, rotationY, rotationZ);
          turn.RotationRemaining -= frameRotation;

          rotation.Value = math.mul(rotation.Value, quaternion.EulerXYZ(frameRotation));

          if (math.Equals(turn.RotationRemaining == new float3(0f), new bool3(true))) {
            node.State = NodeState.Success;
          }

          EntityManager.SetComponentData(root, rotation);
          EntityManager.SetComponentData(root, turn);
          EntityManager.SetComponentData(root, node);

          continue;
        }

        var nodeEntity = EntityManager.CreateEntity();
        EntityManager.AddSharedComponentData(nodeEntity, new TreeId { Id = node.TreeId });
        EntityManager.AddComponentData(nodeEntity, turn);
        EntityManager.AddComponentData(nodeEntity, node);
        EntityManager.RemoveComponent(root, typeof(AINodeTurn));
        EntityManager.RemoveComponent(root, typeof(AINodeStatus));
      }

      rootEntities.Dispose();
      nodeStatus.Dispose();
      turnStatus.Dispose();
      rotations.Dispose();






      rootEntities = chaseQuery.ToEntityArray(Allocator.TempJob);
      nodeStatus = chaseQuery.ToComponentDataArray<AINodeStatus>(Allocator.TempJob);
      var chaseStatus = chaseQuery.ToComponentDataArray<AINodeChase>(Allocator.TempJob);
      rotations = chaseQuery.ToComponentDataArray<Rotation>(Allocator.TempJob);
      translations = chaseQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
      var targetAddresses = targetPositionQuery.ToComponentDataArray<Address>(Allocator.TempJob);
      var targetPositions = targetPositionQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

      for (int i = 0; i < chaseQuery.CalculateEntityCount(); i++) {
        var root = rootEntities[i];
        var node = nodeStatus[i];
        var chase = chaseStatus[i];
        var rotation = rotations[i];
        var translation = translations[i];
        var targetId = chase.TargetId;

        if (targetPositions.Length <= 0) {
          break;
        }

        var targetPosition = targetPositions[0];
        for (int x = 0; x < targetAddresses.Length; x++) {
          if (targetAddresses[x].Id != targetId) {
            continue;
          }
          targetPosition = targetPositions[x];
          break;
        }
        var distance = math.distance(translation.Value, targetPosition.Value);

        if (distance <= chase.MinimumDistance) {
          node.State = NodeState.Success;

          var nodeEntity = EntityManager.CreateEntity();
          EntityManager.AddSharedComponentData(nodeEntity, new TreeId { Id = node.TreeId });
          EntityManager.AddComponentData(nodeEntity, chase);
          EntityManager.AddComponentData(nodeEntity, node);
          EntityManager.RemoveComponent(root, typeof(AINodeChase));
          EntityManager.RemoveComponent(root, typeof(AINodeStatus));
          continue;
        }

        rotation.Value = quaternion.LookRotation(new float3(-1) * (translation.Value - targetPosition.Value), new float3(0f, 1f, 0f));
        translation.Value += math.rotate(rotation.Value, new float3(0f, 0f, chase.SpeedBoost * deltaTime));

        EntityManager.SetComponentData(root, rotation);
        EntityManager.SetComponentData(root, translation);
      }

      rootEntities.Dispose();
      nodeStatus.Dispose();
      chaseStatus.Dispose();
      rotations.Dispose();
      translations.Dispose();
      targetPositions.Dispose();
      targetAddresses.Dispose();








      return handle;
    }
  }

}
