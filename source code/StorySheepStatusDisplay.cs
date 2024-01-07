using System;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000161 RID: 353
public class StorySheepStatusDisplay : MonoBehaviour
{
	// Token: 0x06000874 RID: 2164 RVA: 0x00007F4A File Offset: 0x0000614A
	private void Awake()
	{
		this.rootRectTransform = base.GetComponent<RectTransform>();
	}

	// Token: 0x06000875 RID: 2165 RVA: 0x00007F58 File Offset: 0x00006158
	private void OnEnable()
	{
		this.AnimateOnScreenComplete();
	}

	// Token: 0x06000876 RID: 2166 RVA: 0x00038454 File Offset: 0x00036654
	private void AnimateOffScreen()
	{
		this.currentDisplayState = StorySheepStatusDisplay.DisplayState.AnimatingOut;
		this.movementTween = Tween.AnchoredPosition(this.rootRectTransform, new Vector3(0f, 280f, 0f), 0.5f, 0f, Tween.EaseIn, Tween.LoopType.None, null, new Action(this.AnimateOffScreenComplete), true);
	}

	// Token: 0x06000877 RID: 2167 RVA: 0x00007F60 File Offset: 0x00006160
	private void AnimateOffScreenComplete()
	{
		this.currentDisplayState = StorySheepStatusDisplay.DisplayState.OffScreen;
	}

	// Token: 0x06000878 RID: 2168 RVA: 0x000384B0 File Offset: 0x000366B0
	private void AnimateOnScreen()
	{
		this.currentDisplayState = StorySheepStatusDisplay.DisplayState.AnimatingIn;
		if (this.movementTween != null)
		{
			this.movementTween.Stop();
		}
		this.movementTween = Tween.AnchoredPosition(this.rootRectTransform, new Vector3(0f, 0f, 0f), 0.5f, 0f, Tween.EaseOut, Tween.LoopType.None, null, new Action(this.AnimateOnScreenComplete), true);
	}

	// Token: 0x06000879 RID: 2169 RVA: 0x00007F69 File Offset: 0x00006169
	private void AnimateOnScreenComplete()
	{
		this.currentDisplayState = StorySheepStatusDisplay.DisplayState.OnScreen;
		this.rootRectTransform.anchoredPosition = Vector3.zero;
		this.onScreenTimer = 2f;
	}

	// Token: 0x0600087A RID: 2170 RVA: 0x00038520 File Offset: 0x00036720
	private void Update()
	{
		if (this.state == 0 && this.onScreenTimer > 0f)
		{
			this.onScreenTimer -= Time.deltaTime;
			if (this.onScreenTimer <= 0f)
			{
				this.AnimateOffScreen();
			}
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			if (this.state == 0)
			{
				this.state = 1;
				this.AnimateOnScreen();
				return;
			}
			this.state = 0;
   			this.AnimateOffScreen();
		}
	}

	// Token: 0x0600087B RID: 2171 RVA: 0x00038590 File Offset: 0x00036790
	public void UpdateSheepPlayerIndexes(int shaunPlayerIndex, int timmyPlayerIndex, int shirleyPlayerIndex)
	{
		this.sheepPlayerIndex[0] = shaunPlayerIndex;
		this.sheepPlayerIndex[1] = timmyPlayerIndex;
		this.sheepPlayerIndex[2] = shirleyPlayerIndex;
		this.UpdateSheepDisplay(0, shaunPlayerIndex);
		this.UpdateSheepDisplay(1, timmyPlayerIndex);
		this.UpdateSheepDisplay(2, shirleyPlayerIndex);
		if (this.currentDisplayState == StorySheepStatusDisplay.DisplayState.OffScreen || this.currentDisplayState == StorySheepStatusDisplay.DisplayState.AnimatingOut)
		{
			this.AnimateOnScreen();
			return;
		}
		this.onScreenTimer = 2f;
	}

	// Token: 0x0600087C RID: 2172 RVA: 0x000385F4 File Offset: 0x000367F4
	private void UpdateSheepDisplay(int sheepIndex, int playerIndex)
	{
		Color colourForPlayerIndex = this.GetColourForPlayerIndex(playerIndex);
		Sprite numberSpriteForPlayerIndex = this.GetNumberSpriteForPlayerIndex(playerIndex);
		Image image = this.colouredTabImages[sheepIndex];
		this.colouredMarkerImages[sheepIndex].color = colourForPlayerIndex;
		image.color = colourForPlayerIndex;
		Image image2 = this.numberImages[sheepIndex];
		if (numberSpriteForPlayerIndex != null)
		{
			image2.sprite = numberSpriteForPlayerIndex;
			image2.SetNativeSize();
			image2.gameObject.SetActive(true);
			image.gameObject.SetActive(true);
			return;
		}
		image2.gameObject.SetActive(false);
		image.gameObject.SetActive(false);
	}

	// Token: 0x0600087D RID: 2173 RVA: 0x00007F92 File Offset: 0x00006192
	private Color GetColourForPlayerIndex(int index)
	{
		switch (index)
		{
		case 0:
			return this.playerOneColour;
		case 1:
			return this.playerTwoColour;
		case 2:
			return this.playerThreeColour;
		default:
			return this.noPlayerColour;
		}
	}

	// Token: 0x0600087E RID: 2174 RVA: 0x00007FC3 File Offset: 0x000061C3
	private Sprite GetNumberSpriteForPlayerIndex(int index)
	{
		switch (index)
		{
		case 0:
			return this.numberOneSprite;
		case 1:
			return this.numberTwoSprite;
		case 2:
			return this.numberThreeSprite;
		default:
			return null;
		}
	}

	// Token: 0x0400091F RID: 2335
	public Color playerOneColour;

	// Token: 0x04000920 RID: 2336
	public Color playerTwoColour;

	// Token: 0x04000921 RID: 2337
	public Color playerThreeColour;

	// Token: 0x04000922 RID: 2338
	public Color noPlayerColour;

	// Token: 0x04000923 RID: 2339
	public Image[] colouredMarkerImages;

	// Token: 0x04000924 RID: 2340
	public Image[] colouredTabImages;

	// Token: 0x04000925 RID: 2341
	public Image[] numberImages;

	// Token: 0x04000926 RID: 2342
	public Sprite numberOneSprite;

	// Token: 0x04000927 RID: 2343
	public Sprite numberTwoSprite;

	// Token: 0x04000928 RID: 2344
	public Sprite numberThreeSprite;

	// Token: 0x04000929 RID: 2345
	private StorySheepStatusDisplay.DisplayState currentDisplayState;

	// Token: 0x0400092A RID: 2346
	private int[] sheepPlayerIndex = new int[]
	{
		0,
		-1,
		-1
	};

	// Token: 0x0400092B RID: 2347
	private RectTransform rootRectTransform;

	// Token: 0x0400092C RID: 2348
	private TweenBase movementTween;

	// Token: 0x0400092D RID: 2349
	private float onScreenTimer;

	// Token: 0x0400092E RID: 2350
	private const float onScreenDuration = 2f;

	// Token: 0x0400092F RID: 2351
	private const float movementTweenDuration = 0.5f;

	// Token: 0x04000930 RID: 2352
	private const float onScreenPosY = 0f;

	// Token: 0x04000931 RID: 2353
	private const float offScreenPosY = 280f;

	// Token: 0x04000932 RID: 2354
	private int state;

	// Token: 0x02000162 RID: 354
	private enum DisplayState
	{
		// Token: 0x04000934 RID: 2356
		OnScreen,
		// Token: 0x04000935 RID: 2357
		AnimatingOut,
		// Token: 0x04000936 RID: 2358
		OffScreen,
		// Token: 0x04000937 RID: 2359
		AnimatingIn
	}
}
