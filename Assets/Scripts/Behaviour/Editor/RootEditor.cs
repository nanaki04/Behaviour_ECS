using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EcsAI {

  [CustomEditor(typeof(AIRootProxy))]
  public class RootEditor : Editor {
    private readonly List<Color> colors = new List<Color>() {
      new Color(0.0f, 0.5f, 0.0f, 1.0f),
      new Color(0.0f, 0.0f, 0.5f, 1.0f),
      new Color(0.5f, 0.0f, 0.0f, 1.0f),
      new Color(0.0f, 0.5f, 0.5f, 1.0f),
      new Color(0.5f, 0.0f, 0.5f, 1.0f),
      new Color(0.5f, 0.5f, 0.0f, 1.0f),
      new Color(0.5f, 0.5f, 0.5f, 1.0f),
      new Color(0.0f, 0.0f, 0.0f, 1.0f),
    };

    private void RemoveAt(SerializedProperty node, int at) {
      var idx = node.FindPropertyRelative("Index").intValue;
      if (idx == at) {
        return;
      }
      node.FindPropertyRelative("Index").intValue -= at < idx ? 1 : 0;

      var children = node.FindPropertyRelative("ChildIndices");
      for (int i = 0; i < children.arraySize; i++) {
        var child = children.GetArrayElementAtIndex(i).intValue;
        if (child == at) {
          children.DeleteArrayElementAtIndex(i);
          UpdateArrayValue(children, i, c => c - 1);
        } else if (child > at) {
          UpdateArrayValue(children, i, c => c - 1);
        }
      }
    }

    private void InsertAt(SerializedProperty node, int at) {
      var idx = node.FindPropertyRelative("Index").intValue;
      node.FindPropertyRelative("Index").intValue += at <= idx ? 1 : 0;

      var children = node.FindPropertyRelative("ChildIndices");
      for (int i = 0; i < children.arraySize; i++) {
        var child = children.GetArrayElementAtIndex(i).intValue;
        if (child >= at) {
          UpdateArrayValue(children, i, c => c + 1);
        }
      }
    }

    private void UpdateArrayValue(SerializedProperty arr, int idx, Func<int, int> handler) {
      if (arr.arraySize <= idx) {
        return;
      }
      var element = arr.GetArrayElementAtIndex(idx);
      element.intValue = handler(element.intValue);
    } 

    private void AddChild(SerializedProperty node, int idx) {
      var children = node.FindPropertyRelative("ChildIndices");
      var count = children.arraySize;
      children.InsertArrayElementAtIndex(count);
      UpdateArrayValue(children, count, _ => idx);
    }

    private void Remove(SerializedProperty node, SerializedProperty nodes) {
      var serializedChildren = node.FindPropertyRelative("ChildIndices");
      var children = new List<int>() {};
      for (var i = 0; i < serializedChildren.arraySize; i++) {
        children.Add(serializedChildren.GetArrayElementAtIndex(i).intValue);
      }

      children.Sort();
      children.Reverse();

      children
        .ForEach(child => {
          Remove(nodes.GetArrayElementAtIndex(child), nodes);
        });

      var idx = node.FindPropertyRelative("Index").intValue;
      nodes.DeleteArrayElementAtIndex(idx);
    }

    private void ForEachNode(SerializedProperty nodes, Action<SerializedProperty, int> handler) {
      var count = nodes.arraySize;
      for (int i = 0; i < count; i++) {
        var node = nodes.GetArrayElementAtIndex(i);
        handler(node, i);
      }
    }

    private void Indent(int indentLevel, Action action) {
      GUILayout.BeginHorizontal();
      GUILayout.Space(indentLevel * 10);

      action();

      GUILayout.EndHorizontal();
    }

    private bool IndentButton(int indentLevel, string title) {
      GUILayout.BeginHorizontal();
      GUILayout.Space(indentLevel * 10);
      var pressed = GUILayout.Button(title);
      GUILayout.EndHorizontal();

      return pressed;
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();

      var nodes = serializedObject.FindProperty("nodes");
      var count = nodes.arraySize;
      var colorIndex = 0;
      var originalColor = GUI.backgroundColor;
      var originalContentColor = GUI.contentColor;
      var indentLevel = 0;

      var colorStack = new Stack<Color>();
      var indentStack = new Stack<int>();

      colorStack.Push(originalColor);
      indentStack.Push(indentLevel);

      EditorGUILayout.PropertyField(serializedObject.FindProperty("loopOnOk"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("loopOnFailure"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("address"));

      if (count == 0) {
        if (GUILayout.Button("Add Root Node")) {
          nodes.InsertArrayElementAtIndex(0);
        }
      }

      for (int i = 0; i < nodes.arraySize; i++) {
        var node = nodes.GetArrayElementAtIndex(i);
        var type = node.FindPropertyRelative("Type").enumValueIndex;
        var color = colorStack.Count > 0 ? colorStack.Peek() : originalColor;
        indentLevel = indentStack.Count > 0 ? indentStack.Pop() : 0;
        GUI.backgroundColor = color;

        Indent(indentLevel, () => EditorGUILayout.PropertyField(node.FindPropertyRelative("Type")));
        // DEBUG
        //Indent(indentLevel, () => EditorGUILayout.PropertyField(node.FindPropertyRelative("Index")));

        type = node.FindPropertyRelative("Type").enumValueIndex;

        if (i == 0 && (NodeType)type != NodeType.Selector && (NodeType)type != NodeType.Sequence) {
          node.FindPropertyRelative("Type").enumValueIndex = (int)NodeType.Selector;
          Debug.LogWarning("Root node should be a control node!");
        }

        var typeString = Enum.GetName(typeof(NodeType), type);
        var innerNode = node.FindPropertyRelative(typeString);

        if ((NodeType)type == NodeType.Selector || (NodeType)type == NodeType.Sequence) {
          var childCount = node.FindPropertyRelative("ChildIndices").arraySize;
          color = colors[colorIndex % colors.Count];
          colorIndex++;
          var prevColor = colorStack.Count > 0 ? colorStack.Pop() : originalColor;

          GUI.backgroundColor = prevColor;

          Indent(indentLevel, () => EditorGUILayout.PropertyField(innerNode, new GUIContent(typeString), true));

          if (IndentButton(indentLevel, "Add Node")) {
            var insertPosition = i + 1 + childCount;
            ForEachNode(nodes, (n, _) => InsertAt(n, insertPosition));
            AddChild(node, insertPosition);
            nodes.InsertArrayElementAtIndex(insertPosition);
            var newNode = nodes.GetArrayElementAtIndex(insertPosition);
            newNode.FindPropertyRelative("ChildIndices").ClearArray();
            newNode.FindPropertyRelative("Index").intValue = insertPosition;
          }

          if (IndentButton(indentLevel, "Remove Control Node")) {
            ForEachNode(nodes, (n, _) => RemoveAt(n, i));
            Remove(node, nodes);
          } else {
            childCount = node.FindPropertyRelative("ChildIndices").arraySize;
            // Debug
            //Indent(indentLevel, () => GUILayout.Label("child count: " + childCount, EditorStyles.boldLabel));

            for (int x = 0; x < childCount; x++) {
              // debug
              //Indent(indentLevel, () => GUILayout.Label("idx: " + node.FindPropertyRelative("ChildIndices").GetArrayElementAtIndex(x).intValue));

              colorStack.Push(color);
              indentStack.Push(indentLevel + 1);
            }
          }

        } else {
          if (colorStack.Count > 0) {
            GUI.backgroundColor = colorStack.Pop();
          }

          Indent(indentLevel, () => EditorGUILayout.PropertyField(innerNode, new GUIContent(typeString), true));

          if (IndentButton(indentLevel, "Remove")) {
            ForEachNode(nodes, (n, _) => RemoveAt(n, i));
            Remove(node, nodes);
          }

        }
      }

      GUI.backgroundColor = originalColor;

      serializedObject.ApplyModifiedProperties();
    }
  }
}
