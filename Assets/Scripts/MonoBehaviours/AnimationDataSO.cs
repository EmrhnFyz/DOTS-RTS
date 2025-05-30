using UnityEngine;

public enum AnimationType
{
	None,
	SoldierIdle,
	SoldierWalk,
	ZombieIdle,
	ZombieWalk,
	SoldierShoot,
	SoldierAim,
	ZombieAttack
}

[CreateAssetMenu]
public class AnimationDataSO : ScriptableObject
{
	public AnimationType AnimationType;
	public Mesh[] meshArray;
	public float frameTimerMax;
}