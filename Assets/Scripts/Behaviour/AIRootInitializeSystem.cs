using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Messenging;

namespace EcsAI {

  class AIRootInitializeSystem : ComponentSystem {

    protected override void OnUpdate() {
      Entities.ForEach((Entity entity, ref AIRootInitializer initializer, ref AIRoot root, ref Address address) => {
        if (initializer.TreeId != root.TreeId) {
          PostUpdateCommands.RemoveComponent<AIRootInitializer>(entity);
          return;
        }

        var nodes = AIRootProxy.AINodePrototypes[root.TreeId];
        root.TreeId = address.Id;

        foreach (EditorNode node in nodes) {
          node.UpdateParentId(nodes);
        }

        foreach (EditorNode node in nodes) {
          node.CreateEntity(PostUpdateCommands, address.Id);
        }

        PostUpdateCommands.RemoveComponent<AIRootInitializer>(entity);
        PostUpdateCommands.AddSharedComponent<TreeId>(entity, new TreeId { Id = root.TreeId });
      });
    }
  }

}
