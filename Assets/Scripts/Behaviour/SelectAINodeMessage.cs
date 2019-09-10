using Messenging;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using System.Runtime.Serialization;

namespace EcsMessages {

  public struct SelectAINodePayload : IPayload {}

  [DataContract]
  public class SelectAINodeMessage : Message {
    public override void Send(EntityCommandBuffer commandBuffer) {
      DoSend<SelectAINodePayload>(commandBuffer);
    }
  }

}
