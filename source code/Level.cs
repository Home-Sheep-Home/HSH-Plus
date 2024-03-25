using System;
using System.Collections.Generic;
using ClockStone;
using UnityEngine;

// Token: 0x020000ED RID: 237
public class Level : MonoBehaviour
{
	// Token: 0x060005FA RID: 1530 RVA: 0x00006411 File Offset: 0x00004611
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

	// Token: 0x060005FB RID: 1531 RVA: 0x00006450 File Offset: 0x00004650
	private void Start()
	{
		if (GameManager.instance.IsPartyGame() || (LevelLoader.instance.levelRestarted && !LevelLoader.instance.loadingScreen.gameObject.activeInHierarchy))
		{
			this.StartLevel();
			return;
		}
		this.PauseTime();
	}

	// Token: 0x060005FC RID: 1532 RVA: 0x0002BA28 File Offset: 0x00029C28
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

	// Token: 0x060005FD RID: 1533 RVA: 0x0000648D File Offset: 0x0000468D
	public void AddLevelObject(LevelObject objectToAdd)
	{
		this.objectList.Add(objectToAdd);
	}

	// Token: 0x060005FE RID: 1534 RVA: 0x0000649B File Offset: 0x0000469B
	public void RemoveLevelObject(LevelObject objectToRemove)
	{
		this.objectList.Remove(objectToRemove);
	}

	// Token: 0x060005FF RID: 1535 RVA: 0x000064AA File Offset: 0x000046AA
	public void PauseTime()
	{
		Time.timeScale = 0f;
	}

	// Token: 0x06000600 RID: 1536 RVA: 0x0002BBCC File Offset: 0x00029DCC
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

	// Token: 0x06000601 RID: 1537 RVA: 0x0002BC28 File Offset: 0x00029E28
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

	// Token: 0x06000602 RID: 1538 RVA: 0x000064B6 File Offset: 0x000046B6
	public void PickedUpCollectable(Collectable collectable)
	{
		this.obtainedCollectables.Add(collectable);
	}

	// Token: 0x06000603 RID: 1539 RVA: 0x000064C4 File Offset: 0x000046C4
	private void Update()
	{
		this.gameTime += Time.deltaTime;
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
		{
			LevelLoader.instance.ReloadLevel(false);
		}
	}

	// Token: 0x06000604 RID: 1540 RVA: 0x0002BC8C File Offset: 0x00029E8C
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

	// Token: 0x06000605 RID: 1541 RVA: 0x000064F8 File Offset: 0x000046F8
	private void OnDestroy()
	{
		if (this.music != null)
		{
			this.music.Stop();
		}
		Time.timeScale = 1f;
	}

	// Token: 0x0400063E RID: 1598
	public static Level instance;

	// Token: 0x0400063F RID: 1599
	public LevelBackground levelBackground;

	// Token: 0x04000640 RID: 1600
	public HiddenRoom hiddenRoom;

	// Token: 0x04000641 RID: 1601
	public LevelCanvas levelCanvas;

	// Token: 0x04000642 RID: 1602
	public LevelParticles levelParticles;

	// Token: 0x04000643 RID: 1603
	public SheepManager sheepManager;

	// Token: 0x04000644 RID: 1604
	public Hints hints;

	// Token: 0x04000645 RID: 1605
	public bool shakeLevel;

	// Token: 0x04000646 RID: 1606
	public bool scaffoldLevel;

	// Token: 0x04000647 RID: 1607
	private MeshFilter objectMesh;

	// Token: 0x04000648 RID: 1608
	public List<LevelObject> objectList;

	// Token: 0x04000649 RID: 1609
	public bool restartAllowed;

	// Token: 0x0400064A RID: 1610
	public GoalData goalData;

	// Token: 0x0400064B RID: 1611
	public TriggerVolume goal;

	// Token: 0x0400064C RID: 1612
	public TriggerVolume sheepLock;

	// Token: 0x0400064D RID: 1613
	public bool changeSheepOnLock;

	// Token: 0x0400064E RID: 1614
	public TriggerVolume sheepUnlock;

	// Token: 0x0400064F RID: 1615
	public TriggerVolume triggerBreakage;

	// Token: 0x04000650 RID: 1616
	public TriggerVolume triggerFalling;

	// Token: 0x04000651 RID: 1617
	public LevelObject[] planks;

	// Token: 0x04000652 RID: 1618
	public Bucket bucket;

	// Token: 0x04000653 RID: 1619
	public LevelObject windsock;

	// Token: 0x04000654 RID: 1620
	public ScissorLift scissorLift;

	// Token: 0x04000655 RID: 1621
	public TractorBeam aliens;

	// Token: 0x04000656 RID: 1622
	public Farmer farmer;

	// Token: 0x04000657 RID: 1623
	public GameObject sign;

	// Token: 0x04000658 RID: 1624
	private AudioObject music;

	// Token: 0x04000659 RID: 1625
	private List<Collectable> obtainedCollectables;

	// Token: 0x0400065A RID: 1626
	private float gameTime;
}
