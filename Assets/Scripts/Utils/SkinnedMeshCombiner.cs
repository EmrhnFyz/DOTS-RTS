using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class SkinnedMeshCombiner : MonoBehaviour
{
	[MenuItem("Tools/Merge Skinned Meshes")]
	private static void CombineSkinnedMeshes()
	{
		var root = Selection.activeGameObject;
		var skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();
		if (skinnedRenderers.Length < 2)
		{
			Debug.LogWarning("Select an object with 2 or more SkinnedMeshRenderers.");
			return;
		}

		var newGO = new GameObject("MergedSkinnedMesh");
		var newSMR = newGO.AddComponent<SkinnedMeshRenderer>();
		newGO.transform.SetParent(root.transform, false);

		// Combine meshes
		var combinedMesh = new Mesh();
		var combine = new CombineInstance[skinnedRenderers.Length];
		var bones = skinnedRenderers[0].bones;

		for (var i = 0; i < skinnedRenderers.Length; i++)
		{
			var smr = skinnedRenderers[i];
			combine[i].mesh = smr.sharedMesh;
			combine[i].transform = smr.transform.localToWorldMatrix;
		}

		combinedMesh.CombineMeshes(combine, true, true);
		combinedMesh.name = "CombinedMesh";
		newSMR.sharedMesh = combinedMesh;
		newSMR.bones = bones;
		newSMR.rootBone = skinnedRenderers[0].rootBone;

		var folderPath = "Assets/MergedMeshes";
		if (!Directory.Exists(folderPath))
		{
			Directory.CreateDirectory(folderPath);
		}

		var assetPath = $"{folderPath}/{combinedMesh.name}.asset";
		AssetDatabase.CreateAsset(combinedMesh, assetPath);
		AssetDatabase.SaveAssets();


		Debug.Log("Merged SkinnedMeshRenderers into one.");
	}
}
#endif