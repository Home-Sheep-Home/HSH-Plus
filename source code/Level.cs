using System;
using System.Collections.Generic;
using ClockStone;
using UnityEngine;

// Token: 0x020000EE RID: 238
public class Level : MonoBehaviour
{
	// Token: 0x06000607 RID: 1543 RVA: 0x00006464 File Offset: 0x00004664
	private void Awake()
	{
		if (Level.instance == null)
		{
			Level.instance = this;
		}
		this.obtainedCollectables = new List<Collectable>();
		if (this.hints != null)
		{
			this.hints.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000608 RID: 1544 RVA: 0x000064A3 File Offset: 0x000046A3
	private void Start()
	{
		if (GameManager.instance.IsPartyGame() || (LevelLoader.instance.levelRestarted && !LevelLoader.instance.loadingScreen.gameObject.activeInHierarchy))
		{
			this.StartLevel();
			return;
		}
		this.PauseTime();
	}

	// Token: 0x06000609 RID: 1545 RVA: 0x0002BDF8 File Offset: 0x00029FF8
	public void StartLevel()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			if (TouchControlsManager.instance != null)
			{
				TouchControlsManager.instance.OnLevelLoaded();
			}
		}
		else if (PlayerManager.instance != null)
		{
			PlayerManager.instance.OnLevelLoaded();
		}
		if (GameManager.instance != null && GameManager.instance.IsPartyGame())
		{
			if (Application.platform == RuntimePlatform.Android && PartyTouchControlsManager.instance != null)
			{
				PartyTouchControlsManager.instance.OnLevelLoaded();
			}
			MultiPlayerManager.instance.OnLevelLoaded();
		}
		if (this.shakeLevel)
		{
			if (CameraShake.instance != null)
			{
				CameraShake.instance.SetConstantShake(0.03f, 1f, 1f);
			}
		}
		else if (CameraShake.instance != null)
		{
			CameraShake.instance.StopContantShake();
		}
		if (this.scaffoldLevel)
		{
			foreach (Sheep sheep in this.sheepManager.sheepArray)
			{
				sheep.walkType = "walkscaffold";
			}
		}
		if (LevelLoader.instance)
		{
			string levelId = LevelLoader.instance.levelId;
			SoundLib.instance.StartLevelSounds(levelId);
			if (!LevelLoader.instance.levelRestarted)
			{
				string musicForLevel = SoundLib.instance.GetMusicForLevel(levelId);
				if (musicForLevel != null)
				{
					this.music = AudioController.PlayMusic(musicForLevel, 0.9f, 0f, 0f);
				}
				this.obtainedCollectables.Clear();
			}
			LevelLoader.instance.levelStarted = true;
		}
		AchievementManager.Instance().ClearTempVars();
		this.UnpauseTime();
	}

	// Token: 0x0600060A RID: 1546 RVA: 0x000064E0 File Offset: 0x000046E0
	public void AddLevelObject(LevelObject objectToAdd)
	{
		this.objectList.Add(objectToAdd);
	}

	// Token: 0x0600060B RID: 1547 RVA: 0x000064EE File Offset: 0x000046EE
	public void RemoveLevelObject(LevelObject objectToRemove)
	{
		this.objectList.Remove(objectToRemove);
	}

	// Token: 0x0600060C RID: 1548 RVA: 0x000064FD File Offset: 0x000046FD
	public void PauseTime()
	{
		Time.timeScale = 0f;
	}

	// Token: 0x0600060D RID: 1549 RVA: 0x0002BF9C File Offset: 0x0002A19C
	public void UnpauseTime()
	{
		if (HackManager.Instance().hacks.turboMode.active)
		{
			Time.timeScale = 2f;
			return;
		}
		if (HackManager.Instance().hacks.superTurboMode.active)
		{
			Time.timeScale = 3f;
			return;
		}
		Time.timeScale = 1f;
	}

	// Token: 0x0600060E RID: 1550 RVA: 0x0002BFF8 File Offset: 0x0002A1F8
	public LevelObject FindObject(string name)
	{
		LevelObject result = null;
		for (int i = 0; i < this.objectList.Count; i++)
		{
			if (this.objectList[i] != null && this.objectList[i].name == name)
			{
				result = this.objectList[i];
				break;
			}
		}
		return result;
	}

	// Token: 0x0600060F RID: 1551 RVA: 0x00006509 File Offset: 0x00004709
	public void PickedUpCollectable(Collectable collectable)
	{
		this.obtainedCollectables.Add(collectable);
	}

	// Token: 0x06000610 RID: 1552 RVA: 0x00006517 File Offset: 0x00004717
	private void Update()
	{
		this.gameTime += Time.deltaTime;
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
		{
			LevelLoader.instance.ReloadLevel(false);
		}
	}

	// Token: 0x06000611 RID: 1553 RVA: 0x0002C05C File Offset: 0x0002A25C
	public void SaveLevelData()
	{
		for (int i = 0; i < this.obtainedCollectables.Count; i++)
		{
			CollectablesDataHandler.Instance().GotCollectable(this.obtainedCollectables[i].id);
			string text = CollectablesDataHandler.Instance().BonusLevelUnlocked(this.obtainedCollectables[i].id);
			if (text != "")
			{
				LevelDataHandler.Instance().UnlockLevel(text);
			}
		}
		string levelId = LevelLoader.instance.levelId;
		float num = this.gameTime;
		float bestTime = LevelDataHandler.Instance().GetLevelSaveData(levelId).bestTime;
		float num2 = 0f;
		bool firstPlay = bestTime == 0f;
		int starCount = LevelDataHandler.Instance().CalculateLevelStarCount(levelId, num);
		if (bestTime != 999f)
		{
			num2 = LevelDataHandler.Instance().GetLevelSaveData(levelId).bestTime - num;
		}
		int attempts = 0;
		Debug.Log(string.Concat(new object[]
		{
			"Level complete..time: ",
			num,
			", bestTime: ",
			bestTime,
			", timeDifference: ",
			num2
		}));
		bool flag = LevelDataHandler.Instance().IsLevelBonus(levelId);
		LevelDataHandler.Instance().UpdateLevelData(levelId, starCount, num, attempts);
		if (!flag)
		{
			LevelDataHandler.Instance().UnlockLevels(levelId);
		}
		bool flag2 = LevelDataHandler.Instance().IsLevelIdLastInEpisode(levelId);
		LevelLoader.instance.levelUI.levelCompletePopup.SetIsFinalLevelForChapter(flag2 || flag);
		LevelLoader.instance.levelUI.levelCompletePopup.UpdateLevelInformation(levelId, num, num2, starCount, firstPlay);
		LevelLoader.instance.levelUI.autoSaveIcon.ShowAutoSaveIcon(1f);
		LevelDataHandler.Instance().Save();
		CollectablesDataHandler.Instance().Save();
		AchievementManager.Instance().CheckAchievements(levelId);
	}

	// Token: 0x06000612 RID: 1554 RVA: 0x0000654B File Offset: 0x0000474B
	private void OnDestroy()
	{
		if (this.music != null)
		{
			this.music.Stop();
		}
		Time.timeScale = 1f;
	}

	// Token: 0x0400064A RID: 1610
	public static Level instance;

	// Token: 0x0400064B RID: 1611
	public LevelBackground levelBackground;

	// Token: 0x0400064C RID: 1612
	public HiddenRoom hiddenRoom;

	// Token: 0x0400064D RID: 1613
	public LevelCanvas levelCanvas;

	// Token: 0x0400064E RID: 1614
	public LevelParticles levelParticles;

	// Token: 0x0400064F RID: 1615
	public SheepManager sheepManager;

	// Token: 0x04000650 RID: 1616
	public Hints hints;

	// Token: 0x04000651 RID: 1617
	public bool shakeLevel;

	// Token: 0x04000652 RID: 1618
	public bool scaffoldLevel;

	// Token: 0x04000653 RID: 1619
	private MeshFilter objectMesh;

	// Token: 0x04000654 RID: 1620
	public List<LevelObject> objectList;

	// Token: 0x04000655 RID: 1621
	public bool restartAllowed;

	// Token: 0x04000656 RID: 1622
	public GoalData goalData;

	// Token: 0x04000657 RID: 1623
	public TriggerVolume goal;

	// Token: 0x04000658 RID: 1624
	public TriggerVolume sheepLock;

	// Token: 0x04000659 RID: 1625
	public bool changeSheepOnLock;

	// Token: 0x0400065A RID: 1626
	public TriggerVolume sheepUnlock;

	// Token: 0x0400065B RID: 1627
	public TriggerVolume triggerBreakage;

	// Token: 0x0400065C RID: 1628
	public TriggerVolume triggerFalling;

	// Token: 0x0400065D RID: 1629
	public LevelObject[] planks;

	// Token: 0x0400065E RID: 1630
	public Bucket bucket;

	// Token: 0x0400065F RID: 1631
	public LevelObject windsock;

	// Token: 0x04000660 RID: 1632
	public ScissorLift scissorLift;

	// Token: 0x04000661 RID: 1633
	public TractorBeam aliens;

	// Token: 0x04000662 RID: 1634
	public Farmer farmer;

	// Token: 0x04000663 RID: 1635
	public GameObject sign;

	// Token: 0x04000664 RID: 1636
	private AudioObject music;

	// Token: 0x04000665 RID: 1637
	private List<Collectable> obtainedCollectables;

	// Token: 0x04000666 RID: 1638
	private float gameTime;
}
