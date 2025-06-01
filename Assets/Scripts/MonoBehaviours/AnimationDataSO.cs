using UnityEngine;

public enum AnimationType
{
	None,
	SoldierIdle,
	SoldierWalk,
	SoldierAim,
	SoldierShoot,
	ZombieIdle,
	ZombieWalk,
	ZombieAttack,
	ScoutIdle,
	ScoutWalk,
	ScoutAim,
	ScoutShoot
}

[CreateAssetMenu(fileName = "AnimationDataSO", menuName = "ScriptableObjects/Animations/AnimationDataSO")]
public class AnimationDataSO : ScriptableObject
{
	public AnimationType AnimationType;
	public Mesh[] meshArray;
	public float frameTimerMax;

	public static bool IsAnimationUninterruptible(AnimationType animationType)
	{
		return animationType switch
		{
			AnimationType.ScoutShoot or AnimationType.SoldierShoot or AnimationType.ZombieAttack => true,
			_ => false
		};
	}
}