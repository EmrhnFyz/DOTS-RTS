using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class BakeAndCombineAnimationMesh : MonoBehaviour
{
	[SerializeField] private Animator animator;
	[SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;
	[SerializeField] private int frameCount = 30;
	[SerializeField] private float timePerFrame = 1f / 30f;
	[SerializeField] private string animationName = "Soldier_Idle";

	private void Start()
	{
		animator.Update(0f);

		var folderPath = $"Assets/MeshBakeOutput/{animationName}";
		Directory.CreateDirectory(folderPath);

		for (var frame = 0; frame < frameCount; frame++)
		{
			var combinedMeshes = new List<CombineInstance>();

			foreach (var smr in skinnedMeshRenderers)
			{
				var bakedMesh = new Mesh();
				smr.BakeMesh(bakedMesh);

				// Step 1: Flatten submeshes (combine all into one)
				var submeshCombiners = new List<CombineInstance>();
				for (var sub = 0; sub < bakedMesh.subMeshCount; sub++)
				{
					submeshCombiners.Add(new CombineInstance
					                     {
						                     mesh = bakedMesh,
						                     subMeshIndex = sub,
						                     transform = Matrix4x4.identity
					                     });
				}

				var singleSubmesh = new Mesh();
				singleSubmesh.CombineMeshes(submeshCombiners.ToArray(), true, false);

				// Step 2: Add to final combined mesh list
				combinedMeshes.Add(new CombineInstance
				                   {
					                   mesh = singleSubmesh,
					                   transform = smr.transform.localToWorldMatrix
				                   });
			}

			// Step 3: Combine everything into final mesh
			var finalMesh = new Mesh();
			finalMesh.CombineMeshes(combinedMeshes.ToArray(), true, true); // one single submesh

			var assetPath = $"{folderPath}/{animationName}_{frame}.asset";
			AssetDatabase.CreateAsset(finalMesh, assetPath);

			animator.Update(timePerFrame);
		}

		Debug.Log("All frames baked and submeshes merged.");
	}
}
#endif