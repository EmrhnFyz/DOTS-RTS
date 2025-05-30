using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AnimationDataListSO : ScriptableObject
{
	public List<AnimationDataSO> animationDataSOList;

	public AnimationDataSO GetAnimationDataSO(AnimationType animationType)
	{
		foreach (var animationDataSO in animationDataSOList)
		{
			if (animationDataSO.AnimationType == animationType)
			{
				return animationDataSO;
			}
		}

		return null;
	}
}