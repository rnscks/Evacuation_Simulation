using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WallMaterialSet : MonoBehaviour
{
   [SerializeField] private Material newMaterial;

   void OnValidate()
   {
       Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
       foreach (Renderer renderer in renderers) {
           renderer.sharedMaterial = newMaterial;
       }
        
       #if UNITY_EDITOR
           UnityEditor.EditorUtility.SetDirty(gameObject);
           UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
       #endif
   }
}