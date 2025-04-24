using UnityEngine;
using System;
using System.Reflection;
using HarmonyLib;
using System.Collections.Generic;

namespace Ardot.REPO.MapUpgrade;

public static class Utils
{
    private record struct FieldKey(Type Type, string Field);

    private static Dictionary<FieldKey, FieldInfo> Fields = new ();

    public static object Get<O>(this O obj, string field)
    {
        return GetField<O>(field).GetValue(obj);
    }

    public static T Get<T, O>(this O obj, string field)
    {
        return (T)Get(obj, field);
    }

    public static void Set<T>(this T obj, string field, object value)
    {
        GetField<T>(field).SetValue(obj, value);
    }

    public static FieldInfo GetField<T>(string field)
    {
        FieldKey fieldKey = new (typeof(T), field);

        if(!Fields.TryGetValue(fieldKey, out FieldInfo fieldInfo))
        {
            fieldInfo = AccessTools.Field(typeof(T), field);
            Fields.Add(fieldKey, fieldInfo);
        }

        return fieldInfo;
    }

    public static bool IsHost()
    {
        return SemiFunc.IsMasterClientOrSingleplayer();
    }

    public static void ForObjectsInTree(Transform root, Predicate<Transform> action)
    {
        Recurse(root);

        void Recurse(Transform transform)
        {
            if(!action(transform))
                return;

            for(int x = transform.childCount - 1; x >= 0; x --)
                Recurse(transform.GetChild(x));
        }
    }
}