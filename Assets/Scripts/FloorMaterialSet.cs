using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FloorMaterialSet : MonoBehaviour
{
    [SerializeField] private Material newMaterial;

    void OnValidate()
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.sharedMaterial = newMaterial;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
#endif
    }
}
