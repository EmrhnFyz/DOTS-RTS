using System.IO;
using UnityEditor;
using UnityEngine;

public class BatchSubmeshMerger
{
	[MenuItem("Tools/Merge Submeshes In Selected Folder")]
	private static void MergeSubmeshesInFolder()
	{
		var selectedAssets = Selection.GetFiltered(typeof(Mesh), SelectionMode.DeepAssets);

		if (selectedAssets.Length == 0)
		{
			Debug.LogError("Select a folder or mesh assets to process.");
			return;
		}

		var mergedCount = 0;

		foreach (var obj in selectedAssets)
		{
			if (obj is Mesh mesh && mesh.subMeshCount > 1)
			{
				var originalPath = AssetDatabase.GetAssetPath(mesh);
				var directory = Path.GetDirectoryName(originalPath);
				var meshName = Path.GetFileNameWithoutExtension(originalPath);

				// Ensure Merged subfolder exists
				var mergedFolderPath = Path.Combine(directory, "Merged");
				if (!AssetDatabase.IsValidFolder(mergedFolderPath))
				{
					var parentFolder = Path.GetDirectoryName(mergedFolderPath);
					AssetDatabase.CreateFolder(parentFolder, "Merged");
				}

				// Create new merged mesh
				var mergedMesh = new Mesh
				                 {
					                 name = mesh.name + "_Merged"
				                 };

				// Copy vertex data
				mergedMesh.vertices = mesh.vertices;
				mergedMesh.normals = mesh.normals;
				mergedMesh.tangents = mesh.tangents;
				mergedMesh.uv = mesh.uv;
				mergedMesh.colors = mesh.colors;
				mergedMesh.boneWeights = mesh.boneWeights;
				mergedMesh.bindposes = mesh.bindposes;

				// Combine triangles
				var allIndices = new int[0];
				for (var i = 0; i < mesh.subMeshCount; i++)
				{
					var subIndices = mesh.GetTriangles(i);
					var combined = new int[allIndices.Length + subIndices.Length];
					allIndices.CopyTo(combined, 0);
					subIndices.CopyTo(combined, allIndices.Length);
					allIndices = combined;
				}

				mergedMesh.SetTriangles(allIndices, 0);
				mergedMesh.RecalculateBounds();

				// Save to Merged folder
				var mergedAssetPath = Path.Combine(mergedFolderPath, meshName + "_Merged.asset").Replace("\\", "/");
				AssetDatabase.CreateAsset(mergedMesh, mergedAssetPath);
				mergedCount++;
			}
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log($"✅ Merged {mergedCount} mesh(es) into their respective Merged subfolders.");
	}
}