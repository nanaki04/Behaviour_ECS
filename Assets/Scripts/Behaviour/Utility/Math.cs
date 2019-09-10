using UnityEngine;
using Unity.Mathematics;

namespace Utility {
  public static class Math {
    public static Vector3 Abs(Vector3 v3) {
      return new Vector3(
        System.Math.Abs(v3.x),
        System.Math.Abs(v3.y),
        System.Math.Abs(v3.z)
      );
    }

    public static Vector3 Cap(Vector3 v3, Vector3 cap, Vector3 direction) {
      return new Vector3(
        Cap(v3.x, cap.x, (int)direction.x),
        Cap(v3.y, cap.y, (int)direction.y),
        Cap(v3.z, cap.z, (int)direction.z)
      );
    }

    public static float Cap(float f, float cap, int direction) {
      if (direction * f >= direction * cap) {
        return cap;
      }
      return f;
    }

    public static float3 Vector3ToFloat3(Vector3 v3) {
      return new float3(
        v3.x,
        v3.y,
        v3.z
      );
    }

    public static Vector3 Float3ToVector3(float3 f3) {
      return new Vector3(
        f3.x,
        f3.y,
        f3.z
      );
    }
  }
}
