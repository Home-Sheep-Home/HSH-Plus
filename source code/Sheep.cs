using System;
using System.Collections.Generic;
using ClockStone;
using DG.Tweening;
using UnityEngine;

// Token: 0x0200012A RID: 298
public class Sheep : LevelObject
{
	// Token: 0x1700003A RID: 58
	// (get) Token: 0x0600073B RID: 1851 RVA: 0x0000716C File Offset: 0x0000536C
	// (set) Token: 0x0600073C RID: 1852 RVA: 0x00007174 File Offset: 0x00005374
	[HideInInspector]
	public string animState
	{
		get
		{
			return this._animState;
		}
		set
		{
			this._animState = value;
		}
	}

	// Token: 0x1700003B RID: 59
	// (get) Token: 0x0600073D RID: 1853 RVA: 0x0000717D File Offset: 0x0000537D
	// (set) Token: 0x0600073E RID: 1854 RVA: 0x00007185 File Offset: 0x00005385
	public DashAction DashAction { get; private set; }

	// Token: 0x1700003C RID: 60
	// (get) Token: 0x0600073F RID: 1855 RVA: 0x0000718E File Offset: 0x0000538E
	// (set) Token: 0x06000740 RID: 1856 RVA: 0x00007196 File Offset: 0x00005396
	public BaaAction BaaAction { get; private set; }

	// Token: 0x06000741 RID: 1857 RVA: 0x00032F30 File Offset: 0x00031130
	private void Awake()
	{
		this.body = base.gameObject.GetComponent<Rigidbody2D>();
		this.body.constraints = RigidbodyConstraints2D.FreezeRotation;
		this.body.sleepMode = RigidbodySleepMode2D.NeverSleep;
		this.sheepMass = this.body.mass;
		this.airJump = HackManager.Instance().hacks.airJump.active;
		this.fastWalk = HackManager.Instance().hacks.fastWalk.active;
		this.superJump = HackManager.Instance().hacks.superJump.active;
		this.wallJump = HackManager.Instance().hacks.wallJump.active;
		this.strongPush = HackManager.Instance().hacks.strongPush.active;
		this.sheepAlwaysSmoking = HackManager.Instance().hacks.sheepAlwaysSmoking.active;
		this.useTint = !HackManager.Instance().hacks.pinkSheep.active;
	}

	// Token: 0x06000742 RID: 1858 RVA: 0x00033030 File Offset: 0x00031230
	private void Start()
	{
		this.tint = 1f;
		this.animDirection = this.startingAnimDirection;
		this.animState = "idle";
		switch (this.identity)
		{
		case Sheep.SheepName.Shirley:
			this.id = "shirley";
			return;
		case Sheep.SheepName.Shaun:
			this.id = "shaun";
			return;
		case Sheep.SheepName.Timmy:
			this.id = "timmy";
			return;
		default:
			return;
		}
	}

	// Token: 0x06000743 RID: 1859 RVA: 0x0003309C File Offset: 0x0003129C
	protected override void Update()
	{
		if (this.megaJump && this.megaJumpTimer < this.megaJumpDuration)
		{
			this.megaJumpTimer += Time.deltaTime;
			if (this.megaJumpTimer >= this.megaJumpDuration)
			{
				this.megaJumpTimer = 0f;
				this.megaJump = false;
			}
		}
		if (this.speedBoost && this.speedBoostTimer < this.speedBoostDuration)
		{
			this.speedBoostTimer += Time.deltaTime;
			if (this.speedBoostTimer >= this.speedBoostDuration)
			{
				this.speedBoostTimer = 0f;
				this.speedBoost = false;
			}
		}
		if (this.grow && this.growTimer < this.growDuration)
		{
			this.growTimer += Time.deltaTime;
			if (this.growTimer >= this.growDuration)
			{
				this.body.mass = this.sheepMass;
				base.transform.DOScale(1f, 0.5f);
				this.growTimer = 0f;
				this.grow = false;
			}
		}
		if (this.lightning && this.lightningTimer < this.lightningDuration)
		{
			this.lightningTimer += Time.deltaTime;
			if (this.lightningTimer >= this.lightningDuration)
			{
				this.lightningTimer = 0f;
				this.lightning = false;
				this.electrocuted = false;
			}
		}
		if (this.dragging)
		{
			base.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + this.offset;
		}
	}

	// Token: 0x06000744 RID: 1860 RVA: 0x00033224 File Offset: 0x00031424
	private void LateUpdate()
	{
		if (this.helmet != null && this.helmet.gameObject.activeSelf)
		{
			if (this.spriteRenderer.flipX)
			{
				Vector3 localPosition = this.helmet.gameObject.transform.localPosition;
				localPosition.x *= -1f;
				this.helmet.gameObject.transform.localPosition = localPosition;
			}
			float num = Mathf.Abs(this.helmet.gameObject.transform.localScale.x);
			Vector3 localScale = this.helmet.gameObject.transform.localScale;
			localScale.x = (this.spriteRenderer.flipX ? (-num) : num);
			this.helmet.gameObject.transform.localScale = localScale;
		}
	}

	// Token: 0x06000745 RID: 1861 RVA: 0x00033308 File Offset: 0x00031508
	private void FixedUpdate()
	{
		this.EvtEnterFrame();
		this.EvtCheckJump();
		if (this.onRoad)
		{
			this.walkForce.x = -0.75f;
		}
		this.body.AddRelativeForce(this.walkForce, ForceMode2D.Impulse);
		this.walkForce.Set(0f, 0f);
	}

	// Token: 0x06000746 RID: 1862 RVA: 0x0000719F File Offset: 0x0000539F
	public void PostProcess()
	{
		this.PostProcessJump();
		this.UpdateAnimationState();
	}

	// Token: 0x06000747 RID: 1863 RVA: 0x00033360 File Offset: 0x00031560
	public void PostProcessJump()
	{
		if (this.identity != Sheep.SheepName.Shirley && this.inWater)
		{
			this.canJump = true;
		}
		if ((this.shouldJump && this.canJump) || (this.shouldJump && this.stuckJump) || (this.shouldJump && this.airJump))
		{
			this.CommenceJump();
		}
		this.shouldJump = false;
	}

	// Token: 0x06000748 RID: 1864 RVA: 0x000333C4 File Offset: 0x000315C4
	private void CommenceJump()
	{
		Vector2 velocity = this.body.velocity;
		float num = Vector2.Dot(velocity, this.body.transform.right) / this.body.transform.right.magnitude;
		float num2 = -Mathf.Max(Mathf.Min(-Vector2.Dot(velocity, this.body.transform.up) / this.body.transform.up.magnitude, 0f) - (this.inWater ? this.impulseJumpWater : this.impulseJump), -17f);
		if (this.superJump)
		{
			num2 *= 2f;
		}
		if (this.megaJump)
		{
			num2 *= 1.5f;
		}
		if (this.touchingHat)
		{
			num2 *= 1.4f;
		}
		velocity.x = this.body.transform.right.x * num + this.body.transform.up.x * num2 + (float)this.wallJumpDir * this.body.transform.right.x * this.wallJumpSpeed;
		velocity.y = this.body.transform.right.y * num + this.body.transform.up.y * num2 + (float)this.wallJumpDir * this.body.transform.right.y * this.wallJumpSpeed;
		this.body.velocity = velocity;
		AudioController.Play("jump-" + this.id);
		this.animState = "jump_up";
		this.animHitL = false;
		this.animHitR = false;
		this.animHitT = false;
	}

	// Token: 0x06000749 RID: 1865 RVA: 0x000071AD File Offset: 0x000053AD
	public bool GetCanJump()
	{
		return this.canJump;
	}

	// Token: 0x0600074A RID: 1866 RVA: 0x000071B5 File Offset: 0x000053B5
	public void SetCanJump(bool newVal)
	{
		this.canJump = newVal;
	}

	// Token: 0x0600074B RID: 1867 RVA: 0x000071BE File Offset: 0x000053BE
	public void ResetAirFrames()
	{
		this.framesInAir = 0;
	}

	// Token: 0x0600074C RID: 1868 RVA: 0x0003359C File Offset: 0x0003179C
	public void Spawn(ItemData itemdata)
	{
		this.tags = new List<string>();
		this.tags.Add("levelObject");
		if (itemdata.tags != null)
		{
			this.tags.AddRange(itemdata.tags);
		}
		this.locked = false;
		this.animState = "idle";
		this.animDirection = "r";
		base.transform.position = new Vector2(itemdata.position.x, itemdata.position.y);
		base.transform.Rotate(new Vector3(0f, 0f, itemdata.position.degrees));
	}

	// Token: 0x0600074D RID: 1869 RVA: 0x000071C7 File Offset: 0x000053C7
	public void Lock(bool changeSheepOnLock)
	{
		if (!this.locked)
		{
			this.locked = true;
			if (changeSheepOnLock)
			{
				if (PlayerManager.instance)
				{
					PlayerManager.instance.ChangeSheepOnLock(this);
					return;
				}
				SheepManager.instance.ChangeToNextUnlockedSheep();
			}
		}
	}

	// Token: 0x0600074E RID: 1870 RVA: 0x0003364C File Offset: 0x0003184C
	private void EvtEnterFrame()
	{
		if (this.hasParachute)
		{
			Vector2 v = this.parachute.transform.localScale;
			if (this.animDirection == "r")
			{
				v.x = 1f;
			}
			if (this.animDirection == "l")
			{
				v.x = -1f;
			}
			this.parachute.transform.localScale = v;
			if (this.parachuteDeployed)
			{
				this.body.AddForce(new Vector2(0f, 16f));
			}
		}
		if (this.animState == "victory" || this.animState == "shocked")
		{
			return;
		}
		if (this.canJump && this.jumpDelay <= 0)
		{
			if (this.animState == "jump_down")
			{
				this.animState = "jump_land";
			}
			else if (this.inWater && this.identity != Sheep.SheepName.Shirley)
			{
				this.animState = "swim";
			}
			else if (this.selected)
			{
				if (this.haventPushedSince >= 2)
				{
					this.animState = "alert";
				}
			}
			else
			{
				if (this.animState != "bump_face" && this.animState != "bump_tail" && this.animState != "bump_back")
				{
					this.animState = "idle";
				}
				if (this.animHitL && !this.animPrevHitL && !this.electrocuted)
				{
					this.animState = ((this.animDirection == "r") ? "bump_tail" : "bump_face");
				}
				if (this.animHitR && !this.animPrevHitR && !this.electrocuted)
				{
					this.animState = ((this.animDirection == "r") ? "bump_face" : "bump_tail");
				}
				if (this.animHitT && !this.animPrevHitT && !this.electrocuted)
				{
					this.animState = "bump_back";
				}
			}
			this.framesInAir = 0;
		}
		else if (this.inWater && this.identity != Sheep.SheepName.Shirley)
		{
			this.animState = "swim";
		}
		else
		{
			if (!this.dragging)
			{
				this.framesInAir++;
			}
			if (this.framesInAir > 4)
			{
				float num = -Vector2.Dot(this.body.velocity, this.body.transform.up) / this.body.transform.up.magnitude;
				if ((double)num < -0.5)
				{
					this.animState = "jump_up";
				}
				if ((double)num >= -0.5 && (double)num <= 0.5)
				{
					this.animState = "jump_apex";
				}
				if ((double)num > 0.5)
				{
					this.animState = "jump_down";
				}
			}
		}
		if (this.wasInWater && !this.inWater)
		{
			this.bubbleParticles.Stop();
			this.drippingParticles.Play();
			this.wasInWater = false;
		}
		if (!this.wasInWater && this.inWater)
		{
			this.bubbleParticles.Play();
		}
		if (this.inWater)
		{
			this.tint += Time.deltaTime;
			Vector2 vector = this.bubbleParticles.transform.localPosition;
			if (this.animDirection == "r")
			{
				vector.x = Mathf.Abs(vector.x);
			}
			if (this.animDirection == "l")
			{
				vector.x = -Mathf.Abs(vector.x);
			}
			this.bubbleParticles.transform.localPosition = vector;
			this.wasInWater = true;
		}
		if (this.touchingField)
		{
			if (this.zap != null)
			{
				if (!this.zap.activeSelf)
				{
					this.zap.SetActive(true);
				}
				this.FlipObject(this.zap, false);
			}
			if (this.smokeParticles != null)
			{
				if (!this.smokeParticles.isPlaying)
				{
					this.smokeParticles.Play();
				}
				this.FlipObject(this.smokeParticles.gameObject, true);
			}
			this.tint -= Time.deltaTime * 0.2f;
		}
		else
		{
			if (this.zap != null && this.zap.activeSelf)
			{
				this.zap.SetActive(false);
			}
			if (this.sheepAlwaysSmoking)
			{
				if (this.smokeParticles != null)
				{
					if (!this.smokeParticles.isPlaying)
					{
						this.smokeParticles.Play();
					}
					this.FlipObject(this.smokeParticles.gameObject, true);
				}
			}
			else if (this.smokeParticles != null && this.smokeParticles.isPlaying)
			{
				this.smokeParticles.Stop();
			}
			this.tint += Time.deltaTime * 0.1f;
		}
		if (!this.disableStuckJump)
		{
			this.stuckJump = (this.framesInAir > 30 && this.animHitL && this.animHitR);
		}
		if (this.framesInAir > 30 && this.animHitT && this.stoodUnder && this.stoodUnder.oneWay)
		{
			this.canJump = true;
		}
	}

	// Token: 0x0600074F RID: 1871 RVA: 0x00033BC8 File Offset: 0x00031DC8
	private void FlipObject(GameObject gameObject, bool reverse)
	{
		Vector2 vector = gameObject.transform.localPosition;
		Vector2 vector2 = gameObject.transform.localScale;
		if (this.animDirection == "r")
		{
			vector.x = Mathf.Abs(vector.x);
			vector2.x = Mathf.Abs(vector2.x);
		}
		if (this.animDirection == "l")
		{
			vector.x = -Mathf.Abs(vector.x);
			vector2.x = -Mathf.Abs(vector2.x);
		}
		vector.x *= (float)(reverse ? -1 : 1);
		vector2.x *= (float)(reverse ? -1 : 1);
		gameObject.transform.localPosition = vector;
		gameObject.transform.localScale = vector2;
	}

	// Token: 0x06000750 RID: 1872 RVA: 0x00033CB0 File Offset: 0x00031EB0
	private void EvtCheckJump()
	{
		if (this.spaceSheep)
		{
			if (this.framesInAir > 1)
			{
				this.canJump = false;
			}
		}
		else
		{
			this.canJump = false;
		}
		if (this.jumpDelay == 9)
		{
			this.DoJump();
		}
		this.jumpDelay--;
		this.haventPushedSince++;
	}

	// Token: 0x06000751 RID: 1873 RVA: 0x00033D0C File Offset: 0x00031F0C
	public void TryJump()
	{
		if (this.jumpDelay > 0)
		{
			return;
		}
		this.jumpDelay = 10;
		this.wallJumpDir = 0;
		if (this.wallJump && this.animHitL)
		{
			this.wallJumpDir = 1;
		}
		if (this.wallJump && this.animHitR)
		{
			this.wallJumpDir = -1;
		}
	}

	// Token: 0x06000752 RID: 1874 RVA: 0x000071FD File Offset: 0x000053FD
	public void ForceJump()
	{
		this.DoJump();
		this.TryJump();
	}

	// Token: 0x06000753 RID: 1875 RVA: 0x0000720B File Offset: 0x0000540B
	private void DoJump()
	{
		if (this.locked || this.lightning)
		{
			return;
		}
		this.shouldJump = true;
	}

	// Token: 0x06000754 RID: 1876 RVA: 0x00007225 File Offset: 0x00005425
	public void Select()
	{
		this.selected = true;
		this.highlight.SetActive(true);
	}

	// Token: 0x06000755 RID: 1877 RVA: 0x0000723A File Offset: 0x0000543A
	public void Deselect()
	{
		this.selected = false;
		this.highlight.SetActive(false);
	}

	// Token: 0x06000756 RID: 1878 RVA: 0x00033D60 File Offset: 0x00031F60
	public void UpdateIndicator(Color newColor, Sprite newNumberSprite)
	{
		this.highlight.GetComponent<SpriteRenderer>().color = newColor;
		if (this.playerNumberSprite != null)
		{
			this.playerNumberSprite.sprite = newNumberSprite;
		}
		if (!GameManager.instance.IsPartyGame())
		{
			this.highlight.GetComponent<IndicatorFade>().FadeOnScreen(newColor);
		}
	}

	// Token: 0x06000757 RID: 1879 RVA: 0x0000724F File Offset: 0x0000544F
	public void ShowIndicator()
	{
		if (!GameManager.instance.IsPartyGame())
		{
			this.highlight.GetComponent<IndicatorFade>().FadeOnOrExtend();
		}
	}

	// Token: 0x06000758 RID: 1880 RVA: 0x00033DB8 File Offset: 0x00031FB8
	public void SetPartyHatSprite(Sprite sprite)
	{
		if (sprite != null)
		{
			SpriteRenderer component = this.helmet.GetComponent<SpriteRenderer>();
			if (component != null)
			{
				component.sprite = sprite;
				this.helmet.gameObject.SetActive(true);
				return;
			}
		}
		else
		{
			this.helmet.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000759 RID: 1881 RVA: 0x00033E10 File Offset: 0x00032010
	public void SetTint()
	{
		this.tint = Mathf.Clamp(this.tint, 0f, 1f);
		if (this.tint != this.prevTint)
		{
			float num = this.tint / 2f + 0.5f;
			Color color = new Color(num, num, num);
			this.sheepAnimation.SetColor(color);
			this.prevTint = this.tint;
		}
	}

	// Token: 0x0600075A RID: 1882 RVA: 0x00033E7C File Offset: 0x0003207C
	public void WalkForce(float dir)
	{
		if (this.CheckSheepIsLockedOrShocked())
		{
			return;
		}
		dir *= (float)(this.invertControls ? -1 : 1);
		float num = this.canJump ? this.impulseWalk : this.impulseAir;
		float num2 = this.canJump ? this.maxWalk : this.maxAir;
		float num3 = 0f;
		new Vector2(0f, 0f);
		float num4 = 0f;
		if (this.fastWalk)
		{
			num2 *= 2f;
			num *= 2f;
		}
		else if (this.speedBoost)
		{
			num2 *= 1.5f;
			num *= 1.5f;
		}
		if (this.body != null)
		{
			float num5 = Vector2.Dot(this.body.velocity, this.body.transform.right) / this.body.transform.right.magnitude;
			if (dir < 0f && num5 > -num2 * this.speedCompensation + num4)
			{
				num3 = -num * this.speedCompensation;
			}
			if (dir > 0f && num5 < num2 * this.speedCompensation + num4)
			{
				num3 = num * this.speedCompensation;
			}
		}
		this.animDirection = ((dir > 0f) ? "r" : "l");
		if (this.animState != "jump_up" && this.animState != "jump_apex" && this.animState != "jump_down" && this.animState != "jump_land")
		{
			if (this.haventPushedSince >= 2)
			{
				this.animState = ((this.inWater && this.identity != Sheep.SheepName.Shirley) ? "swim" : "walk");
			}
			if (this.animDirection == "l" && this.animHitL && this.animState != "swim")
			{
				this.animState = "push";
				this.haventPushedSince = 0;
			}
			if (this.animDirection == "r" && this.animHitR && this.animState != "swim")
			{
				this.animState = "push";
				this.haventPushedSince = 0;
			}
		}
		if (this.strongPush)
		{
			num3 *= 5f;
		}
		this.walkForce.x = Vector2.right.x * num3 * 1.3f;
		this.walkForce.y = Vector2.right.y * num3 * 1.3f;
	}

	// Token: 0x0600075B RID: 1883 RVA: 0x00034110 File Offset: 0x00032310
	public void Shocked()
	{
		if (this.identity == Sheep.SheepName.Shaun)
		{
			base.Invoke("SetAnimShocked", 0f);
		}
		if (this.identity == Sheep.SheepName.Timmy)
		{
			base.Invoke("SetAnimShocked", 0.5f);
		}
		if (this.identity == Sheep.SheepName.Shirley)
		{
			base.Invoke("SetAnimShocked", 1f);
		}
	}

	// Token: 0x0600075C RID: 1884 RVA: 0x0000726D File Offset: 0x0000546D
	public void LookLeft()
	{
		this.animDirection = "l";
		this.startingAnimDirection = "l";
	}

	// Token: 0x0600075D RID: 1885 RVA: 0x00007285 File Offset: 0x00005485
	public void LookRight()
	{
		this.animDirection = "r";
		this.startingAnimDirection = "r";
	}

	// Token: 0x0600075E RID: 1886 RVA: 0x0000729D File Offset: 0x0000549D
	private void SetAnimShocked()
	{
		this.animState = "shocked";
	}

	// Token: 0x0600075F RID: 1887 RVA: 0x00034168 File Offset: 0x00032368
	private void UpdateAnimationState()
	{
		if (this.electrocuted)
		{
			this.animState = "electrocute";
			this.tint = 0f;
			this.sheepAnimation.SetColor(Color.white);
			string audioID = (AudioController.GetAudioItem("power_up-electrocute") != null) ? "power_up-electrocute" : "shock";
			if (!AudioController.IsPlaying(audioID))
			{
				AudioController.Play(audioID);
			}
		}
		else if (this.useTint)
		{
			this.SetTint();
		}
		if ((this.sheepAnimation.state == "electrocute_l" || this.sheepAnimation.state == "electrocute_r") && this.sheepAnimation.AnimatorIsPlaying())
		{
			return;
		}
		string text = this.animState + "_" + this.animDirection;
		if (this.animPrevState == text)
		{
			return;
		}
		string animState = this.animState;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(animState);
		if (num <= 1910738160U)
		{
			if (num <= 955079637U)
			{
				if (num != 372915389U)
				{
					if (num != 742744119U)
					{
						if (num != 955079637U)
						{
							goto IL_747;
						}
						if (!(animState == "victory"))
						{
							goto IL_747;
						}
						if (this.walkLoopAudio != null)
						{
							this.walkLoopAudio.Stop();
						}
						if (this.sleepingZeds != null)
						{
							this.sleepingZeds.SetActive(true);
							this.FlipObject(this.sleepingZeds, false);
						}
						this.sheepAnimation.SetState(text);
						this.sheepAnimation.SetDirection(this.animDirection);
						goto IL_747;
					}
					else
					{
						if (!(animState == "jump_land"))
						{
							goto IL_747;
						}
						if (!this.locked)
						{
							AudioController.Play(this.id + "land");
						}
					}
				}
				else
				{
					if (!(animState == "bump_back"))
					{
						goto IL_747;
					}
					goto IL_586;
				}
			}
			else if (num <= 1562476514U)
			{
				if (num != 1502078987U)
				{
					if (num != 1562476514U)
					{
						goto IL_747;
					}
					if (!(animState == "jump_apex"))
					{
						goto IL_747;
					}
				}
				else if (!(animState == "jump_up"))
				{
					goto IL_747;
				}
			}
			else if (num != 1910117389U)
			{
				if (num != 1910738160U)
				{
					goto IL_747;
				}
				if (!(animState == "electrocute"))
				{
					goto IL_747;
				}
				if (this.walkLoopAudio != null)
				{
					this.walkLoopAudio.Stop();
				}
				this.sheepAnimation.SetState(text);
				this.sheepAnimation.SetDirection(this.animDirection);
				goto IL_747;
			}
			else
			{
				if (!(animState == "swim"))
				{
					goto IL_747;
				}
				if (this.walkLoopAudio != null)
				{
					this.walkLoopAudio.Stop();
				}
				this.sheepAnimation.SetState(text);
				this.sheepAnimation.SetDirection(this.animDirection);
				goto IL_747;
			}
		}
		else if (num <= 2686853648U)
		{
			if (num <= 2266792328U)
			{
				if (num != 2130638366U)
				{
					if (num != 2266792328U)
					{
						goto IL_747;
					}
					if (!(animState == "shocked"))
					{
						goto IL_747;
					}
					if (this.walkLoopAudio != null)
					{
						this.walkLoopAudio.Stop();
					}
					this.sheepAnimation.SetState(text);
					this.sheepAnimation.SetDirection(this.animDirection);
					goto IL_747;
				}
				else if (!(animState == "jump_down"))
				{
					goto IL_747;
				}
			}
			else
			{
				if (num != 2272264157U)
				{
					if (num != 2686853648U)
					{
						goto IL_747;
					}
					if (!(animState == "walk"))
					{
						goto IL_747;
					}
				}
				else if (!(animState == "push"))
				{
					goto IL_747;
				}
				if (!this.canJump)
				{
					return;
				}
				if (this.walkLoopAudio != null)
				{
					this.walkLoopAudio.Stop();
				}
				if (!this.locked)
				{
					this.walkLoopAudio = AudioController.Play(this.id + this.walkType + "loop");
				}
				if (this.animPrevDirection != this.animDirection)
				{
					this.sheepAnimation.SetState("turn_" + this.animPrevDirection);
					goto IL_747;
				}
				this.sheepAnimation.SetState(text);
				this.sheepAnimation.SetDirection(this.animDirection);
				goto IL_747;
			}
		}
		else if (num <= 3083032825U)
		{
			if (num != 2738399968U)
			{
				if (num != 3083032825U)
				{
					goto IL_747;
				}
				if (!(animState == "alert"))
				{
					goto IL_747;
				}
				if (this.sheepAnimation.AnimatorStateIsPlaying("jump_land"))
				{
					return;
				}
				if (this.alwaysRunning)
				{
					this.animDirection = "r";
					this.sheepAnimation.SetState("walk_" + this.animDirection);
					this.sheepAnimation.SetDirection(this.animDirection);
					goto IL_747;
				}
				if (this.walkLoopAudio != null)
				{
					this.walkLoopAudio.Stop();
				}
				if (this.prevSelected)
				{
					if (this.sheepAnimation.GetCurrentState("turn_l") || this.sheepAnimation.GetCurrentState("turn_r"))
					{
						return;
					}
					this.sheepAnimation.SetState("alert_static_loop_" + this.animDirection);
				}
				else
				{
					this.sheepAnimation.SetState(text);
				}
				this.sheepAnimation.SetDirection(this.animDirection);
				goto IL_747;
			}
			else
			{
				if (!(animState == "bump_tail"))
				{
					goto IL_747;
				}
				goto IL_586;
			}
		}
		else if (num != 3238464967U)
		{
			if (num != 3271675795U)
			{
				goto IL_747;
			}
			if (!(animState == "idle"))
			{
				goto IL_747;
			}
			if (this.walkLoopAudio != null)
			{
				this.walkLoopAudio.Stop();
			}
			this.sheepAnimation.SetState(text);
			this.sheepAnimation.SetDirection(this.animDirection);
			goto IL_747;
		}
		else
		{
			if (!(animState == "bump_face"))
			{
				goto IL_747;
			}
			goto IL_586;
		}
		if (this.walkLoopAudio != null)
		{
			this.walkLoopAudio.Stop();
		}
		this.sheepAnimation.SetState(text);
		this.sheepAnimation.SetDirection(this.animDirection);
		goto IL_747;
		IL_586:
		if (this.walkLoopAudio != null)
		{
			this.walkLoopAudio.Stop();
		}
		if (this.sheepAnimation.state == "idle_l" || this.sheepAnimation.state == "idle_r" || this.sheepAnimation.state == "swim_l" || this.sheepAnimation.state == "swim_r")
		{
			this.sheepAnimation.SetState(text);
			this.sheepAnimation.SetDirection(this.animDirection);
		}
		if (this.sheepAnimation.state == "electrocute_l" || this.sheepAnimation.state == "electrocute_r")
		{
			this.sheepAnimation.SetState("idle_" + this.animDirection);
			this.sheepAnimation.SetDirection(this.animDirection);
		}
		if (!AudioController.IsPlaying(this.id + "voice") && !this.locked)
		{
			AudioController.Play(this.id + "bump");
		}
		IL_747:
		this.animPrevState = text;
		this.animPrevDirection = this.animDirection;
		this.prevSelected = this.selected;
		this.animPrevHitL = this.animHitL;
		this.animPrevHitR = this.animHitR;
		this.animPrevHitT = this.animHitT;
		this.animHitL = false;
		this.animHitR = false;
		this.animHitT = false;
	}

	// Token: 0x06000760 RID: 1888 RVA: 0x000072AA File Offset: 0x000054AA
	public void ResetAnimationhits()
	{
		this.sheepAnimation.SetDirection(this.animDirection);
		this.animHitL = false;
		this.animHitR = false;
		this.animHitT = false;
	}

	// Token: 0x06000761 RID: 1889 RVA: 0x00034914 File Offset: 0x00032B14
	public void ResetPowerups()
	{
		this.megaJumpTimer = this.megaJumpDuration;
		this.speedBoostTimer = this.speedBoostDuration;
		this.growTimer = this.growDuration;
		this.lightningTimer = this.lightningDuration;
		this.megaJump = false;
		this.speedBoost = false;
		this.grow = false;
		this.lightning = false;
		this.electrocuted = false;
		base.transform.DOKill(false);
		base.transform.DOScale(1f, 0f);
		base.GetComponent<Rigidbody2D>().mass = this.sheepMass;
	}

	// Token: 0x06000762 RID: 1890 RVA: 0x000072D2 File Offset: 0x000054D2
	public void MegaJump(float duration)
	{
		this.megaJump = true;
		this.megaJumpTimer = 0f;
		this.megaJumpDuration = duration;
		AudioController.Play("power_up-jump");
	}

	// Token: 0x06000763 RID: 1891 RVA: 0x000072F8 File Offset: 0x000054F8
	public void SpeedBoost(float duration)
	{
		this.speedBoost = true;
		this.speedBoostTimer = 0f;
		this.speedBoostDuration = duration;
		AudioController.Play("power_up-speed");
	}

	// Token: 0x06000764 RID: 1892 RVA: 0x000349A8 File Offset: 0x00032BA8
	public void Grow(float duration)
	{
		base.transform.DOScale(2f, 0.5f);
		this.body.mass = this.sheepMass * 1.5f;
		this.grow = true;
		this.growTimer = 0f;
		this.growDuration = duration;
		AudioController.Play("power_up-super");
	}

	// Token: 0x06000765 RID: 1893 RVA: 0x0000731E File Offset: 0x0000551E
	public void Lightning()
	{
		this.lightning = true;
		this.electrocuted = true;
		this.lightningTimer = 0f;
		this.lightningDuration = 1f;
	}

	// Token: 0x06000766 RID: 1894 RVA: 0x00007344 File Offset: 0x00005544
	public bool CheckSheepIsLockedOrShocked()
	{
		return this.locked || this.lightning;
	}

	// Token: 0x06000767 RID: 1895 RVA: 0x00034A08 File Offset: 0x00032C08
	public void Dash()
	{
		this.DashAction = base.GetComponent<DashAction>();
		if (this.DashAction != null)
		{
			this.DashAction.Dash();
			return;
		}
		Debug.Log(base.gameObject.name + " is trying to Dash, but no Dash component could be found!");
	}

	// Token: 0x06000768 RID: 1896 RVA: 0x00034A58 File Offset: 0x00032C58
	public void Baa()
	{
		this.BaaAction = base.GetComponent<BaaAction>();
		if (this.BaaAction != null)
		{
			this.BaaAction.Baa();
			return;
		}
		Debug.Log(base.gameObject.name + " is trying to Baa, but no Baa component could be found!");
	}

	// Token: 0x0600076A RID: 1898 RVA: 0x00007359 File Offset: 0x00005559
	private void OnMouseDown()
	{
		this.offset = base.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
		this.dragging = true;
	}

	// Token: 0x0600076B RID: 1899 RVA: 0x00007387 File Offset: 0x00005587
	private void OnMouseUp()
	{
		this.dragging = false;
	}

	// Token: 0x040007C9 RID: 1993
	[Header("Sheep")]
	public Sheep.SheepName identity;

	// Token: 0x040007CA RID: 1994
	public SheepAnimation sheepAnimation;

	// Token: 0x040007CB RID: 1995
	public GameObject topShape;

	// Token: 0x040007CC RID: 1996
	public GameObject leftShape;

	// Token: 0x040007CD RID: 1997
	public GameObject rightShape;

	// Token: 0x040007CE RID: 1998
	public GameObject bottomShape;

	// Token: 0x040007CF RID: 1999
	public GameObject highlight;

	// Token: 0x040007D0 RID: 2000
	public SpriteRenderer playerNumberSprite;

	// Token: 0x040007D1 RID: 2001
	public GameObject parachute;

	// Token: 0x040007D2 RID: 2002
	public GameObject zap;

	// Token: 0x040007D3 RID: 2003
	public GameObject smoke;

	// Token: 0x040007D4 RID: 2004
	public SpriteRenderer helmet;

	// Token: 0x040007D5 RID: 2005
	public GameObject sleepingZeds;

	// Token: 0x040007D6 RID: 2006
	public ParticleSystem smokeParticles;

	// Token: 0x040007D7 RID: 2007
	public ParticleSystem drippingParticles;

	// Token: 0x040007D8 RID: 2008
	public ParticleSystem bubbleParticles;

	// Token: 0x040007D9 RID: 2009
	public bool disableStuckJump;

	// Token: 0x040007DA RID: 2010
	public bool invertControls;

	// Token: 0x040007DB RID: 2011
	public bool useTint = true;

	// Token: 0x040007DC RID: 2012
	public string startingAnimDirection = "r";

	// Token: 0x040007DD RID: 2013
	public float impulseJump = 5f;

	// Token: 0x040007DE RID: 2014
	public float impulseJumpWater = 4f;

	// Token: 0x040007DF RID: 2015
	public float impulseWalk = 0.1f;

	// Token: 0x040007E0 RID: 2016
	public float impulseAir = 0.05f;

	// Token: 0x040007E1 RID: 2017
	public float wallJumpSpeed = 11f;

	// Token: 0x040007E2 RID: 2018
	public float maxWalk = 2f;

	// Token: 0x040007E3 RID: 2019
	public float maxAir = 1f;

	// Token: 0x040007E4 RID: 2020
	public int bubbleDelay = 75;

	// Token: 0x040007E5 RID: 2021
	public bool spaceSheep;

	// Token: 0x040007E6 RID: 2022
	public bool alwaysRunning;

	// Token: 0x040007E7 RID: 2023
	private string _animState = "idle";

	// Token: 0x040007E8 RID: 2024
	[HideInInspector]
	public string animDirection = "r";

	// Token: 0x040007E9 RID: 2025
	[HideInInspector]
	public bool animHitL;

	// Token: 0x040007EA RID: 2026
	[HideInInspector]
	public bool animHitR;

	// Token: 0x040007EB RID: 2027
	[HideInInspector]
	public bool animHitT;

	// Token: 0x040007EC RID: 2028
	[HideInInspector]
	public bool animPrevHitL;

	// Token: 0x040007ED RID: 2029
	[HideInInspector]
	public bool animPrevHitR;

	// Token: 0x040007EE RID: 2030
	[HideInInspector]
	public bool animPrevHitT;

	// Token: 0x040007EF RID: 2031
	[HideInInspector]
	public bool electrocuted;

	// Token: 0x040007F0 RID: 2032
	[HideInInspector]
	public bool electricOverride;

	// Token: 0x040007F1 RID: 2033
	[HideInInspector]
	public bool selected;

	// Token: 0x040007F2 RID: 2034
	[HideInInspector]
	public bool PlayerControlled;

	// Token: 0x040007F3 RID: 2035
	[HideInInspector]
	public string id = "";

	// Token: 0x040007F4 RID: 2036
	[HideInInspector]
	public int jumpDelay;

	// Token: 0x040007F5 RID: 2037
	[HideInInspector]
	public bool wasInWater;

	// Token: 0x040007F6 RID: 2038
	[HideInInspector]
	public LevelObject standingOn;

	// Token: 0x040007F7 RID: 2039
	[HideInInspector]
	public float speedCompensation = 1f;

	// Token: 0x040007F8 RID: 2040
	[HideInInspector]
	public bool locked;

	// Token: 0x040007F9 RID: 2041
	[HideInInspector]
	public bool touchingOneWay;

	// Token: 0x040007FA RID: 2042
	[HideInInspector]
	public LevelObject stoodUnder;

	// Token: 0x040007FB RID: 2043
	[HideInInspector]
	public string walkType = "walk";

	// Token: 0x040007FC RID: 2044
	[HideInInspector]
	public bool touchingField;

	// Token: 0x040007FD RID: 2045
	[HideInInspector]
	public bool inDanger;

	// Token: 0x040007FE RID: 2046
	[HideInInspector]
	public bool inBeam;

	// Token: 0x040007FF RID: 2047
	[HideInInspector]
	public bool inBox;

	// Token: 0x04000800 RID: 2048
	[HideInInspector]
	public bool touchingHat;

	// Token: 0x04000801 RID: 2049
	[HideInInspector]
	public int wallJumpDir;

	// Token: 0x04000802 RID: 2050
	[HideInInspector]
	public bool airborne;

	// Token: 0x04000803 RID: 2051
	[HideInInspector]
	public bool hasParachute;

	// Token: 0x04000804 RID: 2052
	[HideInInspector]
	public bool parachuteDeployed;

	// Token: 0x04000805 RID: 2053
	[HideInInspector]
	public bool onRoad;

	// Token: 0x04000806 RID: 2054
	[HideInInspector]
	public float tint = 1f;

	// Token: 0x04000809 RID: 2057
	private bool canJump;

	// Token: 0x0400080A RID: 2058
	private bool shouldJump;

	// Token: 0x0400080B RID: 2059
	private bool stuckJump;

	// Token: 0x0400080C RID: 2060
	private bool airJump;

	// Token: 0x0400080D RID: 2061
	private bool fastWalk;

	// Token: 0x0400080E RID: 2062
	private bool superJump;

	// Token: 0x0400080F RID: 2063
	private bool wallJump;

	// Token: 0x04000810 RID: 2064
	private bool strongPush;

	// Token: 0x04000811 RID: 2065
	private bool sheepAlwaysSmoking;

	// Token: 0x04000812 RID: 2066
	private bool megaJump;

	// Token: 0x04000813 RID: 2067
	private float megaJumpTimer;

	// Token: 0x04000814 RID: 2068
	private float megaJumpDuration;

	// Token: 0x04000815 RID: 2069
	private bool speedBoost;

	// Token: 0x04000816 RID: 2070
	private float speedBoostTimer;

	// Token: 0x04000817 RID: 2071
	private float speedBoostDuration;

	// Token: 0x04000818 RID: 2072
	private bool grow;

	// Token: 0x04000819 RID: 2073
	private float growTimer;

	// Token: 0x0400081A RID: 2074
	private float growDuration;

	// Token: 0x0400081B RID: 2075
	private bool lightning;

	// Token: 0x0400081C RID: 2076
	private float lightningTimer;

	// Token: 0x0400081D RID: 2077
	private float lightningDuration;

	// Token: 0x0400081E RID: 2078
	private float sheepMass;

	// Token: 0x0400081F RID: 2079
	private Vector2 walkForce;

	// Token: 0x04000820 RID: 2080
	private float prevTint;

	// Token: 0x04000821 RID: 2081
	private string animPrevState = "";

	// Token: 0x04000822 RID: 2082
	private int haventPushedSince = 10;

	// Token: 0x04000823 RID: 2083
	private string animPrevDirection = "r";

	// Token: 0x04000824 RID: 2084
	private bool prevSelected;

	// Token: 0x04000825 RID: 2085
	private int framesInAir;

	// Token: 0x04000826 RID: 2086
	private Vector3 raycastOffset = new Vector3(0f, 0f, 0f);

	// Token: 0x04000827 RID: 2087
	private AudioObject walkLoopAudio;

	// Token: 0x04000828 RID: 2088
	public bool dragging = false;

	// Token: 0x04000829 RID: 2089
	public Vector3 offset;

	// Token: 0x0200012B RID: 299
	public enum SheepName
	{
		// Token: 0x0400082B RID: 2091
		Shirley,
		// Token: 0x0400082C RID: 2092
		Shaun,
		// Token: 0x0400082D RID: 2093
		Timmy
	}
}
