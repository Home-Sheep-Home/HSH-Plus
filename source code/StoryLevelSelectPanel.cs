using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000E1 RID: 225
public class StoryLevelSelectPanel : MonoBehaviour
{
	// Token: 0x060005C9 RID: 1481 RVA: 0x0002A810 File Offset: 0x00028A10
	public void SetupPanel(string id, Sprite thumbnailSprite)
	{
		this.levelId = id;
		string levelLocalisationKeyForLevelId = LocalisationManager.Instance().GetLevelLocalisationKeyForLevelId(this.levelId);
		string translation = LocalisationManager.Instance().GetTranslation(levelLocalisationKeyForLevelId);
		this.levelTitleText.text = translation;
		this.previewImage.sprite = thumbnailSprite;
		bool flag = LevelDataHandler.Instance().IsLevelTraining(this.levelId);
		int i = 0;
		int num = this.starImages.Length;
		while (i < num)
		{
			this.starImages[i].gameObject.SetActive(!flag);
			i++;
		}
		int j = 0;
		int num2 = this.collectableImages.Length;
		while (j < num2)
		{
			this.collectableImages[j].gameObject.SetActive(!flag);
			j++;
		}
		this.bestTimeTitleText.gameObject.SetActive(!flag);
		this.bestTimeText.gameObject.SetActive(!flag);
		if (this.levelId != "")
		{
			LevelSaveData levelSaveData = LevelDataHandler.Instance().GetLevelSaveData(this.levelId);
			if (levelSaveData != null)
			{
				bool locked = levelSaveData.locked;
				this.SetLocked(locked);
				if (!locked)
				{
					Color color = levelSaveData.levelCompleted ? this.panelLevelCompletedColour : this.panelLevelUnlockedColour;
					this.panelBottomImage.color = color;
					this.panelTopImage.color = color;
					LevelCollectables levelCollectableData = CollectablesDataHandler.Instance().GetLevelCollectableData(this.levelId);
					int collectedSocks = levelCollectableData.collectedSocks;
					int maxSocks = levelCollectableData.maxSocks;
					int collectedCakes = levelCollectableData.collectedCakes;
					int maxCakes = levelCollectableData.maxCakes;
					int collectedBonuses = levelCollectableData.collectedBonuses;
					int maxBonuses = levelCollectableData.maxBonuses;
					int totalCount = maxSocks + maxCakes + maxBonuses;
					this.SetupCollectables(totalCount, collectedCakes, collectedSocks, collectedBonuses);
					int stars = levelSaveData.stars;
					this.SetStarCount(stars);
					float bestTime = levelSaveData.bestTime;
					this.SetBestTimeText(bestTime);
					return;
				}
				this.lockedLevelTitleText.text = translation;
				return;
			}
			else
			{
				this.SetLocked(true);
				Debug.LogWarning("Level data could not be found. LevelID = " + this.levelId);
			}
		}
	}

	// Token: 0x060005CA RID: 1482 RVA: 0x0000620C File Offset: 0x0000440C
	public void SetLocked(bool locked)
	{
		this.levelLockedRoot.gameObject.SetActive(locked);
		this.levelUnlockedRoot.gameObject.SetActive(!locked);
		this.isLocked = locked;
	}

	// Token: 0x060005CB RID: 1483 RVA: 0x0000623A File Offset: 0x0000443A
	public bool IsLocked()
	{
		return this.isLocked;
	}

	// Token: 0x060005CC RID: 1484 RVA: 0x0002AA10 File Offset: 0x00028C10
	public void SetStarCount(int count)
	{
		for (int i = 0; i < this.starImages.Length; i++)
		{
			Image image = this.starImages[i];
			image.sprite = ((i < count) ? this.starOnSprite : this.starOffSprite);
			image.SetNativeSize();
		}
		this.starOutline.gameObject.SetActive(count > 3 && this.starImages[0].gameObject.activeSelf);
	}

	// Token: 0x060005CD RID: 1485 RVA: 0x0002AA80 File Offset: 0x00028C80
	public void SetupCollectables(int totalCount, int cupcakesFound, int socksFound, int bonusesFound)
	{
		for (int i = 0; i < this.collectableImages.Length; i++)
		{
			Image image = this.collectableImages[i];
			if (i < totalCount)
			{
				if (i < cupcakesFound)
				{
					image.sprite = this.collectableFoundCupcakeSprite;
				}
				else if (i < cupcakesFound + socksFound)
				{
					image.sprite = this.collectableFoundSockSprite;
				}
				else if (i < cupcakesFound + socksFound + bonusesFound)
				{
					image.sprite = this.collectableFoundJoystickSprite;
				}
				else
				{
					image.sprite = this.collectableUnfoundSprite;
				}
				image.SetNativeSize();
			}
			image.gameObject.SetActive(i < totalCount);
		}
	}

	// Token: 0x060005CE RID: 1486
	public void SetBestTimeText(float bestTime)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)bestTime);
		string text = string.Format("{0:D2}:{1:D2}.{2:D3}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
		if (text.Substring(0, 1) == "0")
		{
			text = text.Remove(0, 1);
		}
		this.bestTimeText.text = text;
	}

	// Token: 0x060005CF RID: 1487 RVA: 0x00006242 File Offset: 0x00004442
	public string GetLevelId()
	{
		return this.levelId;
	}

	// Token: 0x040005DB RID: 1499
	public Text levelTitleText;

	// Token: 0x040005DC RID: 1500
	public Image panelTopImage;

	// Token: 0x040005DD RID: 1501
	public Image panelBottomImage;

	// Token: 0x040005DE RID: 1502
	public Color panelLevelCompletedColour;

	// Token: 0x040005DF RID: 1503
	public Color panelLevelUnlockedColour;

	// Token: 0x040005E0 RID: 1504
	public Image previewImage;

	// Token: 0x040005E1 RID: 1505
	public Image[] starImages;

	// Token: 0x040005E2 RID: 1506
	public Image starOutline;

	// Token: 0x040005E3 RID: 1507
	public Sprite starOffSprite;

	// Token: 0x040005E4 RID: 1508
	public Sprite starOnSprite;

	// Token: 0x040005E5 RID: 1509
	public Image[] collectableImages;

	// Token: 0x040005E6 RID: 1510
	public Sprite collectableUnfoundSprite;

	// Token: 0x040005E7 RID: 1511
	public Sprite collectableFoundSockSprite;

	// Token: 0x040005E8 RID: 1512
	public Sprite collectableFoundCupcakeSprite;

	// Token: 0x040005E9 RID: 1513
	public Sprite collectableFoundJoystickSprite;

	// Token: 0x040005EA RID: 1514
	public Text bestTimeTitleText;

	// Token: 0x040005EB RID: 1515
	public Text bestTimeText;

	// Token: 0x040005EC RID: 1516
	public Transform levelUnlockedRoot;

	// Token: 0x040005ED RID: 1517
	public Transform levelLockedRoot;

	// Token: 0x040005EE RID: 1518
	public Text lockedLevelTitleText;

	// Token: 0x040005EF RID: 1519
	private bool isLocked;

	// Token: 0x040005F0 RID: 1520
	private string levelId;
}
