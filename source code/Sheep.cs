using System;
using System.Collections.Generic;
using ClockStone;
using DG.Tweening;
using UnityEngine;

// Token: 0x020000E6 RID: 230
public class Sheep : LevelObject
{
	// Token: 0x17000006 RID: 6
	// (get) Token: 0x06000678 RID: 1656 RVA: 0x0002917F File Offset: 0x0002737F
	// (set) Token: 0x06000679 RID: 1657 RVA: 0x00029187 File Offset: 0x00027387
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

	// Token: 0x17000007 RID: 7
	// (get) Token: 0x0600067A RID: 1658 RVA: 0x00029190 File Offset: 0x00027390
	// (set) Token: 0x0600067B RID: 1659 RVA: 0x00029198 File Offset: 0x00027398
	public DashAction DashAction { get; private set; }

	// Token: 0x17000008 RID: 8
	// (get) Token: 0x0600067C RID: 1660 RVA: 0x000291A1 File Offset: 0x000273A1
	// (set) Token: 0x0600067D RID: 1661 RVA: 0x000291A9 File Offset: 0x000273A9
	public BaaAction BaaAction { get; private set; }

	// Token: 0x0600067E RID: 1662 RVA: 0x000291B4 File Offset: 0x000273B4
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

	// Token: 0x0600067F RID: 1663 RVA: 0x000292B4 File Offset: 0x000274B4
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

	// Token: 0x06000680 RID: 1664
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

	// Token: 0x06000681 RID: 1665 RVA: 0x0002947C File Offset: 0x0002767C
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

	// Token: 0x06000682 RID: 1666 RVA: 0x00029560 File Offset: 0x00027760
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

	// Token: 0x06000683 RID: 1667 RVA: 0x000295B8 File Offset: 0x000277B8
	public void PostProcess()
	{
		this.PostProcessJump();
		this.UpdateAnimationState();
	}

	// Token: 0x06000684 RID: 1668 RVA: 0x000295C8 File Offset: 0x000277C8
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

	// Token: 0x06000685 RID: 1669 RVA: 0x0002962C File Offset: 0x0002782C
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

	// Token: 0x06000686 RID: 1670 RVA: 0x00029804 File Offset: 0x00027A04
	public bool GetCanJump()
	{
		return this.canJump;
	}

	// Token: 0x06000687 RID: 1671 RVA: 0x0002980C File Offset: 0x00027A0C
	public void SetCanJump(bool newVal)
	{
		this.canJump = newVal;
	}

	// Token: 0x06000688 RID: 1672 RVA: 0x00029815 File Offset: 0x00027A15
	public void ResetAirFrames()
	{
		this.framesInAir = 0;
	}

	// Token: 0x06000689 RID: 1673 RVA: 0x00029820 File Offset: 0x00027A20
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

	// Token: 0x0600068A RID: 1674 RVA: 0x000298CE File Offset: 0x00027ACE
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

	// Token: 0x0600068B RID: 1675 RVA: 0x00029904 File Offset: 0x00027B04
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
			this.framesInAir++;
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

	// Token: 0x0600068C RID: 1676 RVA: 0x00029E78 File Offset: 0x00028078
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

	// Token: 0x0600068D RID: 1677 RVA: 0x00029F60 File Offset: 0x00028160
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

	// Token: 0x0600068E RID: 1678 RVA: 0x00029FBC File Offset: 0x000281BC
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

	// Token: 0x0600068F RID: 1679 RVA: 0x0002A010 File Offset: 0x00028210
	public void ForceJump()
	{
		this.DoJump();
		this.TryJump();
	}

	// Token: 0x06000690 RID: 1680 RVA: 0x0002A01E File Offset: 0x0002821E
	private void DoJump()
	{
		if (this.locked || this.lightning)
		{
			return;
		}
		this.shouldJump = true;
	}

	// Token: 0x06000691 RID: 1681 RVA: 0x0002A038 File Offset: 0x00028238
	public void Select()
	{
		this.selected = true;
		this.highlight.SetActive(true);
	}

	// Token: 0x06000692 RID: 1682 RVA: 0x0002A04D File Offset: 0x0002824D
	public void Deselect()
	{
		this.selected = false;
		this.highlight.SetActive(false);
	}

	// Token: 0x06000693 RID: 1683 RVA: 0x0002A064 File Offset: 0x00028264
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

	// Token: 0x06000694 RID: 1684 RVA: 0x0002A0B9 File Offset: 0x000282B9
	public void ShowIndicator()
	{
		if (!GameManager.instance.IsPartyGame())
		{
			this.highlight.GetComponent<IndicatorFade>().FadeOnOrExtend();
		}
	}

	// Token: 0x06000695 RID: 1685 RVA: 0x0002A0D8 File Offset: 0x000282D8
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

	// Token: 0x06000696 RID: 1686 RVA: 0x0002A130 File Offset: 0x00028330
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

	// Token: 0x06000697 RID: 1687 RVA: 0x0002A19C File Offset: 0x0002839C
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
		this.walkForce.x = Vector2.right.x * num3;
		this.walkForce.y = Vector2.right.y * num3;
	}

	// Token: 0x06000698 RID: 1688 RVA: 0x0002A424 File Offset: 0x00028624
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

	// Token: 0x06000699 RID: 1689 RVA: 0x0002A47B File Offset: 0x0002867B
	public void LookLeft()
	{
		this.animDirection = "l";
		this.startingAnimDirection = "l";
	}

	// Token: 0x0600069A RID: 1690 RVA: 0x0002A493 File Offset: 0x00028693
	public void LookRight()
	{
		this.animDirection = "r";
		this.startingAnimDirection = "r";
	}

	// Token: 0x0600069B RID: 1691 RVA: 0x0002A4AB File Offset: 0x000286AB
	private void SetAnimShocked()
	{
		this.animState = "shocked";
	}

	// Token: 0x0600069C RID: 1692 RVA: 0x0002A4B8 File Offset: 0x000286B8
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

	// Token: 0x0600069D RID: 1693 RVA: 0x0002AC64 File Offset: 0x00028E64
	public void ResetAnimationhits()
	{
		this.sheepAnimation.SetDirection(this.animDirection);
		this.animHitL = false;
		this.animHitR = false;
		this.animHitT = false;
	}

	// Token: 0x0600069E RID: 1694 RVA: 0x0002AC8C File Offset: 0x00028E8C
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

	// Token: 0x0600069F RID: 1695 RVA: 0x0002AD20 File Offset: 0x00028F20
	public void MegaJump(float duration)
	{
		this.megaJump = true;
		this.megaJumpTimer = 0f;
		this.megaJumpDuration = duration;
		AudioController.Play("power_up-jump");
	}

	// Token: 0x060006A0 RID: 1696 RVA: 0x0002AD46 File Offset: 0x00028F46
	public void SpeedBoost(float duration)
	{
		this.speedBoost = true;
		this.speedBoostTimer = 0f;
		this.speedBoostDuration = duration;
		AudioController.Play("power_up-speed");
	}

	// Token: 0x060006A1 RID: 1697 RVA: 0x0002AD6C File Offset: 0x00028F6C
	public void Grow(float duration)
	{
		base.transform.DOScale(2f, 0.5f);
		this.body.mass = this.sheepMass * 1.5f;
		this.grow = true;
		this.growTimer = 0f;
		this.growDuration = duration;
		AudioController.Play("power_up-super");
	}

	// Token: 0x060006A2 RID: 1698 RVA: 0x0002ADCA File Offset: 0x00028FCA
	public void Lightning()
	{
		this.lightning = true;
		this.electrocuted = true;
		this.lightningTimer = 0f;
		this.lightningDuration = 1f;
	}

	// Token: 0x060006A3 RID: 1699 RVA: 0x0002ADF0 File Offset: 0x00028FF0
	public bool CheckSheepIsLockedOrShocked()
	{
		return this.locked || this.lightning;
	}

	// Token: 0x060006A4 RID: 1700 RVA: 0x0002AE08 File Offset: 0x00029008
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

	// Token: 0x060006A5 RID: 1701 RVA: 0x0002AE58 File Offset: 0x00029058
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

	// Token: 0x060013AC RID: 5036
	private void OnMouseDown()
	{
		this.offset = base.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
		this.dragging = true;
	}

	// Token: 0x060013AD RID: 5037
	private void OnMouseUp()
	{
		this.dragging = false;
	}

	// Token: 0x04000678 RID: 1656
	[Header("Sheep")]
	public Sheep.SheepName identity;

	// Token: 0x04000679 RID: 1657
	public SheepAnimation sheepAnimation;

	// Token: 0x0400067A RID: 1658
	public GameObject topShape;

	// Token: 0x0400067B RID: 1659
	public GameObject leftShape;

	// Token: 0x0400067C RID: 1660
	public GameObject rightShape;

	// Token: 0x0400067D RID: 1661
	public GameObject bottomShape;

	// Token: 0x0400067E RID: 1662
	public GameObject highlight;

	// Token: 0x0400067F RID: 1663
	public SpriteRenderer playerNumberSprite;

	// Token: 0x04000680 RID: 1664
	public GameObject parachute;

	// Token: 0x04000681 RID: 1665
	public GameObject zap;

	// Token: 0x04000682 RID: 1666
	public GameObject smoke;

	// Token: 0x04000683 RID: 1667
	public SpriteRenderer helmet;

	// Token: 0x04000684 RID: 1668
	public GameObject sleepingZeds;

	// Token: 0x04000685 RID: 1669
	public ParticleSystem smokeParticles;

	// Token: 0x04000686 RID: 1670
	public ParticleSystem drippingParticles;

	// Token: 0x04000687 RID: 1671
	public ParticleSystem bubbleParticles;

	// Token: 0x04000688 RID: 1672
	public bool disableStuckJump;

	// Token: 0x04000689 RID: 1673
	public bool invertControls;

	// Token: 0x0400068A RID: 1674
	public bool useTint = true;

	// Token: 0x0400068B RID: 1675
	public string startingAnimDirection = "r";

	// Token: 0x0400068C RID: 1676
	public float impulseJump = 5f;

	// Token: 0x0400068D RID: 1677
	public float impulseJumpWater = 4f;

	// Token: 0x0400068E RID: 1678
	public float impulseWalk = 0.1f;

	// Token: 0x0400068F RID: 1679
	public float impulseAir = 0.05f;

	// Token: 0x04000690 RID: 1680
	public float wallJumpSpeed = 11f;

	// Token: 0x04000691 RID: 1681
	public float maxWalk = 2f;

	// Token: 0x04000692 RID: 1682
	public float maxAir = 1f;

	// Token: 0x04000693 RID: 1683
	public int bubbleDelay = 75;

	// Token: 0x04000694 RID: 1684
	public bool spaceSheep;

	// Token: 0x04000695 RID: 1685
	public bool alwaysRunning;

	// Token: 0x04000696 RID: 1686
	private string _animState = "idle";

	// Token: 0x04000697 RID: 1687
	[HideInInspector]
	public string animDirection = "r";

	// Token: 0x04000698 RID: 1688
	[HideInInspector]
	public bool animHitL;

	// Token: 0x04000699 RID: 1689
	[HideInInspector]
	public bool animHitR;

	// Token: 0x0400069A RID: 1690
	[HideInInspector]
	public bool animHitT;

	// Token: 0x0400069B RID: 1691
	[HideInInspector]
	public bool animPrevHitL;

	// Token: 0x0400069C RID: 1692
	[HideInInspector]
	public bool animPrevHitR;

	// Token: 0x0400069D RID: 1693
	[HideInInspector]
	public bool animPrevHitT;

	// Token: 0x0400069E RID: 1694
	[HideInInspector]
	public bool electrocuted;

	// Token: 0x0400069F RID: 1695
	[HideInInspector]
	public bool electricOverride;

	// Token: 0x040006A0 RID: 1696
	[HideInInspector]
	public bool selected;

	// Token: 0x040006A1 RID: 1697
	[HideInInspector]
	public bool PlayerControlled;

	// Token: 0x040006A2 RID: 1698
	[HideInInspector]
	public string id = "";

	// Token: 0x040006A3 RID: 1699
	[HideInInspector]
	public int jumpDelay;

	// Token: 0x040006A4 RID: 1700
	[HideInInspector]
	public bool wasInWater;

	// Token: 0x040006A5 RID: 1701
	[HideInInspector]
	public LevelObject standingOn;

	// Token: 0x040006A6 RID: 1702
	[HideInInspector]
	public float speedCompensation = 1f;

	// Token: 0x040006A7 RID: 1703
	[HideInInspector]
	public bool locked;

	// Token: 0x040006A8 RID: 1704
	[HideInInspector]
	public bool touchingOneWay;

	// Token: 0x040006A9 RID: 1705
	[HideInInspector]
	public LevelObject stoodUnder;

	// Token: 0x040006AA RID: 1706
	[HideInInspector]
	public string walkType = "walk";

	// Token: 0x040006AB RID: 1707
	[HideInInspector]
	public bool touchingField;

	// Token: 0x040006AC RID: 1708
	[HideInInspector]
	public bool inDanger;

	// Token: 0x040006AD RID: 1709
	[HideInInspector]
	public bool inBeam;

	// Token: 0x040006AE RID: 1710
	[HideInInspector]
	public bool inBox;

	// Token: 0x040006AF RID: 1711
	[HideInInspector]
	public bool touchingHat;

	// Token: 0x040006B0 RID: 1712
	[HideInInspector]
	public int wallJumpDir;

	// Token: 0x040006B1 RID: 1713
	[HideInInspector]
	public bool airborne;

	// Token: 0x040006B2 RID: 1714
	[HideInInspector]
	public bool hasParachute;

	// Token: 0x040006B3 RID: 1715
	[HideInInspector]
	public bool parachuteDeployed;

	// Token: 0x040006B4 RID: 1716
	[HideInInspector]
	public bool onRoad;

	// Token: 0x040006B5 RID: 1717
	[HideInInspector]
	public float tint = 1f;

	// Token: 0x040006B8 RID: 1720
	private bool canJump;

	// Token: 0x040006B9 RID: 1721
	private bool shouldJump;

	// Token: 0x040006BA RID: 1722
	private bool stuckJump;

	// Token: 0x040006BB RID: 1723
	private bool airJump;

	// Token: 0x040006BC RID: 1724
	private bool fastWalk;

	// Token: 0x040006BD RID: 1725
	private bool superJump;

	// Token: 0x040006BE RID: 1726
	private bool wallJump;

	// Token: 0x040006BF RID: 1727
	private bool strongPush;

	// Token: 0x040006C0 RID: 1728
	private bool sheepAlwaysSmoking;

	// Token: 0x040006C1 RID: 1729
	private bool megaJump;

	// Token: 0x040006C2 RID: 1730
	private float megaJumpTimer;

	// Token: 0x040006C3 RID: 1731
	private float megaJumpDuration;

	// Token: 0x040006C4 RID: 1732
	private bool speedBoost;

	// Token: 0x040006C5 RID: 1733
	private float speedBoostTimer;

	// Token: 0x040006C6 RID: 1734
	private float speedBoostDuration;

	// Token: 0x040006C7 RID: 1735
	private bool grow;

	// Token: 0x040006C8 RID: 1736
	private float growTimer;

	// Token: 0x040006C9 RID: 1737
	private float growDuration;

	// Token: 0x040006CA RID: 1738
	private bool lightning;

	// Token: 0x040006CB RID: 1739
	private float lightningTimer;

	// Token: 0x040006CC RID: 1740
	private float lightningDuration;

	// Token: 0x040006CD RID: 1741
	private float sheepMass;

	// Token: 0x040006CE RID: 1742
	private Vector2 walkForce;

	// Token: 0x040006CF RID: 1743
	private float prevTint;

	// Token: 0x040006D0 RID: 1744
	private string animPrevState = "";

	// Token: 0x040006D1 RID: 1745
	private int haventPushedSince = 10;

	// Token: 0x040006D2 RID: 1746
	private string animPrevDirection = "r";

	// Token: 0x040006D3 RID: 1747
	private bool prevSelected;

	// Token: 0x040006D4 RID: 1748
	private int framesInAir;

	// Token: 0x040006D5 RID: 1749
	private Vector3 raycastOffset = new Vector3(0f, 0f, 0f);

	// Token: 0x040006D6 RID: 1750
	private AudioObject walkLoopAudio;

	// Token: 0x040010B3 RID: 4275
	public bool dragging = false;

	// Token: 0x040010B4 RID: 4276
	public Vector3 offset;

	// Token: 0x020001E5 RID: 485
	public enum SheepName
	{
		// Token: 0x04000DCC RID: 3532
		Shirley,
		// Token: 0x04000DCD RID: 3533
		Shaun,
		// Token: 0x04000DCE RID: 3534
		Timmy
	}
}
