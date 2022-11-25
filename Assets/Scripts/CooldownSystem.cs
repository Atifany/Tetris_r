using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CooldownSystem : MonoBehaviour
{
	private readonly List<CooldownData> cooldowns = new List<CooldownData>();
	private void Update()
	{
		ProcessCooldowns();
	}
	public void PutOnCooldown(int id, float cooldownDuration){
		cooldowns.Add(new CooldownData(id, cooldownDuration));
	}
	private void ProcessCooldowns(){
		float deltaTime = Time.deltaTime;

		for (int i = cooldowns.Count - 1; i >= 0 ; i--)
		{
			if (cooldowns[i].DecrementCooldown(deltaTime)){
				cooldowns.RemoveAt(i);
			}
		}
	}
	public bool IsOnCoolDown(int id){
		foreach (CooldownData cooldown in cooldowns)
		{
			if (cooldown.Id == id) { return (true); }
		}
		return (false);
	}
	public float GetRemainingCooldown(int id){
		foreach (CooldownData cooldown in cooldowns)
		{
			if (cooldown.Id != id) { continue; }
			return (cooldown.RemainingTime);
		}
		return (0f);
	}
}

public class CooldownData
{
	public int		Id { get; }
	public float	RemainingTime { get; private set;}
	public CooldownData(int id, float cooldownDuration){
		Id = id;
		RemainingTime = cooldownDuration;
	}

	public bool DecrementCooldown(float deltatime){
		RemainingTime = Mathf.Max(RemainingTime - deltatime, 0f);
		return (RemainingTime == 0f);
	}
}