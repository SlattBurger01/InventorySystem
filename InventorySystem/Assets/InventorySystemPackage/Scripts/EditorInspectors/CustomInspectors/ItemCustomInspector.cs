using InventorySystem.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InventorySystem.Editor_
{
    [CustomEditor(typeof(Item))]
    public class ItemInspectorHelper : Editor
    {
        private Type[] valueTypes = { typeof(bool), typeof(byte), typeof(int), typeof(float), typeof(string) };
        private int targetType;

        private object targetValue = (bool)false; // SO IT'S NOT NULL AT THE START
        private string targetName;

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            if (!CustomInspectorHelper.CustomInspectors) return;

            DrawCustomVarsCreator();

            CheckCustomVars();
        }

        // CUSTOM VARS CREATOR
        private void DrawCustomVarsCreator()
        {
            GUILayout.Space(20);

            GUILayout.Label("CUSTOM VARIABLES CREATOR");

            targetName = EditorGUILayout.TextField("VARIABLE NAME", targetName);
            targetType = EditorGUILayout.IntPopup("VARIABLE TYPE", targetType, GetPopupContent(), null);

            DrawTargetInput();

            if (GUILayout.Button("Create custom value")) CreateCustomValue();
        }

        private void CreateCustomValue()
        {
            Item item = (Item)target;
            int arrayPos = 0;

            switch (GetTypeS())
            {
                case "bool": AddValToArray<bool>(ref item.boolValues, out arrayPos); break;
                case "byte": AddValToArray<byte>(ref item.byteValues, out arrayPos); break;
                case "int": AddValToArray<int>(ref item.intValues, out arrayPos); break;
                case "float": AddValToArray<float>(ref item.floatValues, out arrayPos); break;
                case "string": AddValToArray<string>(ref item.stringValues, out arrayPos); break;
            }

            AddValToArray<string>(ref item.valueNames, targetName);
            AddValToArray<string>(ref item.valuesIds, $"{targetType}-{arrayPos}");
        }

        private void DrawTargetInput()
        {
            switch (GetTypeS())
            {
                case "bool":
                    CheckType<bool>();
                    targetValue = (bool)EditorGUILayout.Toggle("VARIABLE VALUE", (bool)targetValue, GUILayout.Width(15));
                    break;

                case "byte":
                    CheckType<byte>();
                    targetValue = (byte)EditorGUILayout.IntField("VARIABLE VALUE", (byte)targetValue);
                    break;

                case "int":
                    CheckType<int>();
                    targetValue = (int)EditorGUILayout.IntField("VARIABLE VALUE", (int)targetValue);
                    break;

                case "float":
                    CheckType<float>();
                    targetValue = (float)EditorGUILayout.FloatField("VARIABLE VALUE", (float)targetValue);
                    break;

                case "string":
                    CheckType<string>("");
                    targetValue = (string)EditorGUILayout.TextField("VARIABLE VALUE", (string)targetValue);
                    break;
            }
        }

        private string GetTypeS()
        {
            if (valueTypes[targetType] == typeof(bool)) return "bool";
            else if (valueTypes[targetType] == typeof(byte)) return "byte";
            else if (valueTypes[targetType] == typeof(int)) return "int";
            else if (valueTypes[targetType] == typeof(float)) return "float";
            else if (valueTypes[targetType] == typeof(string)) return "string";

            return "unknown";
        }

        private string NormalizeContentName(string content)
        {
            if (content == "Boolean") content = "bool";
            else if (content == "Int32") content = "int";
            else if (content == "Single") content = "float";

            return content.ToLower();
        }

        private void CheckType<T>(object customVal = null)
        {
            if (targetValue.GetType() == typeof(T)) return;
            targetValue = customVal == null ? (T)default : customVal;
        }

        private string[] GetPopupContent()
        {
            string[] content = new string[valueTypes.Length];

            for (int i = 0; i < content.Length; i++)
            {
                content[i] = valueTypes[i].ToString();
                content[i] = content[i].Remove(0, 7); // REMOVE 'System.'

                content[i] = NormalizeContentName(content[i]);
            }

            return content;
        }

        private void AddValToArray<T>(ref T[] array, out int arrayPos)
        {
            arrayPos = array.Length;
            AddValToArray<T>(ref array, (T)targetValue);
        }

        private static void AddValToArray<T>(ref T[] array, T itemToAdd)
        {
            List<T> l = array.ToList();
            l.Add(itemToAdd);
            array = l.ToArray();
        }
        // ----

        private void CheckCustomVars()
        {
            Item item = (Item)target;

            if (item.valueNames == null) return;

            for (int i = 0; i < item.valueNames.Length; i++)
            {
                for (int y = 0; y < item.valueNames.Length; y++)
                {
                    if (y == i) continue;

                    if (item.valueNames[i] == item.valueNames[y])
                    {
                        int addNum = 1;

                        while (item.valueNames[y] + $"{addNum}" == item.valueNames[i]) addNum++;
                        item.valueNames[y] += $"{addNum}";

                        Debug.LogWarning($"Each custom value has to have unique name! (changing name to {item.valueNames[y]})");
                    }
                }
            }
        }
    }
}
