﻿using UnityEngine;

/// <summary>
/// 엑스에 대한 컨트롤러입니다.
/// </summary>
public class XController : PlayerController
{
    #region 효과 객체를 보관합니다.
    GameObject dashBoostEffect = null;

    #endregion



    #region 플레이어의 상태 필드를 정의합니다.
    bool _shooting = false;
    bool Shooting
    {
        get { return _shooting; }
        set { _animator.SetBool("Shooting", _shooting = value); }
    }

    bool shotPressed = false;
    float chargeTime = 0;
    public float maxChargeTime = 3;
    bool ShotTriggered
    {
        get { return _shooting; }
        set { _animator.SetBool("Shooting", _shooting = value); }
    }
    public GameObject[] bullets;
    public Transform shotPosition;
    public float shotSpeed = 10;

    // bool endShotRequested = false;
    float shotTime = 0;
    public float endShotTime = 0.5416667f; // 0.4f;

    public float[] chargeLevel = { 0, 0.3f, 2f };

    // public Animation[] animations;

    #endregion
    
    
    
    #region MonoBehavior 기본 메서드를 재정의합니다.
    void Start()
    {
        Initialize();
    }
    void Update()
    {
        // 소환 중이라면
        if (Spawning)
        {
            return;
        }

        // 새로운 사용자 입력을 확인합니다.
        // 점프 키가 눌린 경우
        if (IsKeyDown(GameKey.Jump))
        {
            if (JumpBlocked)
            {
                // print("TEST");
            }
            else if (Sliding)
            {
                if (IsKeyPressed(GameKey.Dash))
                {
                    WallDashJump();
                }
                else
                {
                    WallJump();
                }
            }
            else if (Dashing)
            {
                DashJump();
            }
            else if (Landed && IsKeyPressed(GameKey.Dash))
            {
                DashJump();
            }
            else
            {
                Jump();
            }
        }
        // 대쉬 키가 눌린 경우
        else if (IsKeyDown(GameKey.Dash))
        {
            if (Sliding)
            {

            }
            else if (Landed == false)
            {
                if (AirDashBlocked)
                {

                }
                else
                {
                    AirDash();
                }
            }
            else if (DashBlocked)
            {

            }
            else
            {
                Dash();
            }
        }
        // 캐릭터 변경 키가 눌린 경우
        else if (Input.GetKeyDown(KeyCode.F))
        {
            sceneManager.ChangePlayer(sceneManager.PlayerZ);
        }
    }
    void FixedUpdate()
    {
        UpdateState();

        // 소환 중이라면
        if (Spawning)
        {
            // 준비 중이라면
            if (Readying)
            {
                // 준비가 끝나서 대기 상태로 전환되었다면
                if (IsAnimationPlaying("Idle"))
                    // 준비를 완전히 종료합니다.
                    EndReady();
            }
            // 준비 중이 아닌데 지상에 착륙했다면
            else if (Landed)
            {
                // 준비 상태로 전환합니다.
                Ready();
            }
            return;
        }

        // 기존 사용자 입력을 확인합니다.
        // 점프 중이라면
        if (Jumping)
        {
            if (Pushing)
            {
                if (SlideBlocked)
                {

                }
                else
                {
                    Slide();
                }
            }
            else if (IsKeyPressed(GameKey.Jump) == false
                || _rigidbody.velocity.y <= 0)
            {
                Fall();
            }
            else
            {
                _rigidbody.velocity = new Vector2
                    (_rigidbody.velocity.x, _rigidbody.velocity.y - jumpDecSize);
            }
        }
        // 떨어지고 있다면
        else if (Falling)
        {
            if (Landed)
            {
                // StopFalling();
                Land();
            }
            else if (Pushing)
            {
                if (SlideBlocked)
                {

                }
                else
                {
                    Slide();
                }
            }
            else
            {
                float vy = _rigidbody.velocity.y - jumpDecSize;
                _rigidbody.velocity = new Vector2
                    (_rigidbody.velocity.x, vy > -16 ? vy : -16);
            }
        }
        // 대쉬 중이라면
        else if (Dashing)
        {
            if (AirDashing)
            {
                if (IsKeyPressed(GameKey.Dash) == false)
                {
                    StopAirDashing();
                    Fall();
                }
                else if (Landed)
                {
                    StopAirDashing();
                    Fall();
                }
                else if (Pushing)
                {
                    StopAirDashing();
                    Slide();
                }
            }
            else if (Landed == false)
            {
                StopDashing();
                Fall();
            }
            else if (IsKeyPressed(GameKey.Dash) == false)
            {
                StopDashing();
            }
        }
        // 벽을 타고 있다면
        else if (Sliding)
        {
            if (Pushing == false)
            {
                StopSliding();
                Fall();
            }
            else if (Landed)
            {
                StopSliding();
                Fall();
            }
        }
        // 벽을 밀고 있다면
        else if (Pushing)
        {
            if (Landed)
            {

            }
            else
            {
                Slide();
            }
        }
        // 그 외의 경우
        else
        {
            if (Landed == false)
            {
                Fall();
            }

            UnblockSliding();
        }

        // 방향 키 입력에 대해 처리합니다.
        // 대쉬 중이라면
        if (Dashing)
        {
            if (AirDashing)
            {

            }
            // 대쉬 중에 공중에 뜬 경우
            else if (Landed == false)
            {
                if (SlideBlocked)
                {

                }
                else if (IsKeyPressed(GameKey.Left))
                {
                    MoveLeft();
                }
                else if (IsKeyPressed(GameKey.Right))
                {
                    MoveRight();
                }
                else
                {
                    StopMoving();
                }
            }
            else
            {

            }
        }
        // 움직임이 막힌 상태라면
        else if (MoveBlocked)
        {

        }
        // 벽 점프 중이라면
        else if (SlideBlocked)
        {

        }
        // 그 외의 경우
        else
        {
            if (IsKeyPressed(GameKey.Left))
            {
                if (FacingRight == false && Pushing)
                {
                    StopMoving();
                }
                else
                {
                    if (Sliding)
                    {
                        StopSliding();
                    }
                    MoveLeft();
                }
            }
            else if (IsKeyPressed(GameKey.Right))
            {
                if (FacingRight && Pushing)
                {
                    StopMoving();
                }
                else
                {
                    if (Sliding)
                    {
                        StopSliding();
                    }
                    MoveRight();
                }
            }
            else
            {
                StopMoving();
            }
        }

        // 공격 키가 눌린 경우를 처리합니다.
        if (IsKeyPressed(GameKey.Attack))
        {
            if (shotPressed == false)
            {
                shotPressed = true;
                chargeTime = 0;
            }
            else
            {
                if (chargeTime < chargeLevel[1] - 0.1f)
                {

                }
                else if (SoundEffects[7].isPlaying == false)
                {
                    SoundEffects[7].time = 0;
                    SoundEffects[7].Play();
                }
                else if (SoundEffects[7].time >= 2.9f)
                {
                    SoundEffects[7].time = 2.1f;
                }
                chargeTime = (chargeTime >= maxChargeTime)
                    ? maxChargeTime : (chargeTime + Time.deltaTime);
                print(chargeTime);
            }
        }
        else if (shotPressed)
        {
            int index = -1;
            if (chargeTime < chargeLevel[1])
            {
                index = 0;
                // _animator.Play("Shot", 0, 0);
            }
            else if (chargeTime < chargeLevel[2])
            {
                index = 1;
                // _animator.Play("Shot", 0, 0);
            }
            else
            {
                index = 2;
                // _animator.Play("ChargeShot", 0, 0);
            }

            Shooting = true;
            if (Moving)
            {
                float nTime = GetCurrentAnimationPlaytime();
                float fTime = nTime - Mathf.Floor(nTime);
                _animator.Play("MoveShotRun", 0, fTime);
            }
            else
            {
                _animator.Play(0, 0, 0);
            }

            GameObject _bullet = Instantiate
                (bullets[index], shotPosition.position, shotPosition.rotation)
                as GameObject;
            Vector3 bulletScale = _bullet.transform.localScale;
            bulletScale.x *= FacingRight ? 1 : -1;
            _bullet.transform.localScale = bulletScale;
            _bullet.GetComponent<Rigidbody2D>().velocity
                = (FacingRight ? Vector3.right : Vector3.left) * shotSpeed;

            // BusterScript busterScript = _bullet.GetComponent<BusterScript>();
            // busterScript.MainCamera = sceneManager.MainCamera;
            BusterAttackScript buster = _bullet.GetComponent<BusterAttackScript>();
            buster.MainCamera = sceneManager.mainCam;

            SoundEffects[8 + index].Play();
            SoundEffects[7].Stop();

            shotPressed = false;
            ShotTriggered = true;
            // ShotBlocked = true;
            // Invoke("UnblockCharging", 0.8f);

            shotTime = 0;
            Invoke("EndShot", endShotTime);
        }

        shotTime += Time.fixedDeltaTime;
        if (IsAnimationPlaying("Shot"))
        {
            float time = GetCurrentAnimationPlaytime();
            float length = GetCurrentAnimationLength();
            // _animator.GetCurrentAnimatorStateInfo(0).
            print(string.Format("{0}/{1}", time, length));
        }
    }

    #endregion



    #region 엑스에 대해 새롭게 정의된 행동 메서드의 목록입니다.
    ///////////////////////////////////////////////////////////////////
    // 공격
    /// <summary>
    /// 버스터 공격합니다.
    /// </summary>
    void Shot()
    {
        if (Shooting)
        {
            _animator.Play("Shot", 0, 0);
        }

        Shooting = true;
        Invoke("StopShot", 1);
    }
    /// <summary>
    /// 버스터 공격을 중지합니다.
    /// </summary>
    void StopShot()
    {
        Shooting = false;
    }
    /// <summary>
    /// 버스터 공격을 종료합니다.
    /// </summary>
    void EndShot()
    {
        if (shotTime >= endShotTime)
        {
            float nTime = GetCurrentAnimationPlaytime();
            float fTime = nTime - Mathf.Floor(nTime);
            ShotTriggered = false;

            string nextStateName = null;
            if (Landed)
            {
                if (Moving)
                {
                    nextStateName = "MoveRun";
                }
                else
                {
                    nextStateName = "Idle";
                }
            }
            else
            {

            }
            _animator.Play(nextStateName, 0, fTime);
        }
    }
    #endregion



    #region PlayerController 행동 메서드를 재정의 합니다.
    ///////////////////////////////////////////////////////////////////
    // 기본
    /// <summary>
    /// 플레이어를 소환합니다.
    /// </summary>
    protected override void Spawn()
    {
        base.Spawn();
        SoundEffects[0].Play();
    }
    /// <summary>
    /// 플레이어가 지상에 착륙할 때의 상태를 설정합니다.
    /// </summary>
    protected override void Land()
    {
        base.Land();
        SoundEffects[2].Play();
    }

    ///////////////////////////////////////////////////////////////////
    // 점프 및 낙하
    /// <summary>
    /// 플레이어를 점프하게 합니다.
    /// </summary>
    protected override void Jump()
    {
        base.Jump();
        SoundEffects[1].Play();
    }

    ///////////////////////////////////////////////////////////////////
    // 대쉬
    /// <summary>
    /// 플레이어가 대쉬하게 합니다.
    /// </summary>
    protected override void Dash()
    {
        base.Dash();

        // 대쉬 효과 애니메이션을 추가합니다.
        GameObject dashFog = Instantiate
            (effects[0], dashFogPosition.position, dashFogPosition.rotation)
            as GameObject;
        if (FacingRight == false)
        {
            var newScale = dashFog.transform.localScale;
            newScale.x = FacingRight ? newScale.x : -newScale.x;
            dashFog.transform.localScale = newScale;
        }
        SoundEffects[3].Play();
    }
    /// <summary>
    /// 플레이어의 대쉬를 중지합니다. (사용자의 입력에 의함)
    /// </summary>
    protected override void StopDashing()
    {
        base.StopDashing();
        if (dashBoostEffect != null)
        {
            dashBoostEffect.GetComponent<EffectScript>().EndEffect();
            dashBoostEffect = null;
        }
    }
    /// <summary>
    /// 플레이어가 대쉬 점프하게 합니다.
    /// </summary>
    protected override void DashJump()
    {
        base.DashJump();

        SoundEffects[3].Stop();
        SoundEffects[1].Play();
        if (dashBoostEffect != null)
        {
            dashBoostEffect.GetComponent<EffectScript>().EndEffect();
            dashBoostEffect = null;
        }
    }
    /// <summary>
    /// 플레이어가 에어 대쉬하게 합니다.
    /// </summary>
    protected override void AirDash()
    {
        base.AirDash();
        SoundEffects[3].Play();
    }
    /// <summary>
    /// 플레이어의 에어 대쉬를 중지합니다.
    /// </summary>
    protected override void StopAirDashing()
    {
        base.StopAirDashing();
        if (dashBoostEffect != null)
        {
            dashBoostEffect.GetComponent<EffectScript>().EndEffect();
            dashBoostEffect = null;
        }
    }

    #endregion



    #region 프레임 이벤트 핸들러를 정의합니다.
    ///////////////////////////////////////////////////////////////////
    // 점프 및 낙하
    /// <summary>
    /// 점프 시작 시에 발생합니다.
    /// </summary>
    void FE_JumpBeg()
    {
//        SoundEffects[1].Play();
    }

    ///////////////////////////////////////////////////////////////////
    // 대쉬
    /// <summary>
    /// 대쉬 준비 애니메이션이 시작할 때 발생합니다.
    /// </summary>
    void FE_DashBegBeg()
    {

    }
    /// <summary>
    /// 대쉬 부스트 애니메이션이 시작할 때 발생합니다.
    /// </summary>
    void FE_DashRunBeg()
    {
        GameObject dashBoost = Instantiate(effects[1], dashBoostPosition.position, dashBoostPosition.rotation) as GameObject;
        dashBoost.transform.SetParent(groundCheck.transform);
        if (FacingRight == false)
        {
            var newScale = dashBoost.transform.localScale;
            newScale.x = FacingRight ? newScale.x : -newScale.x;
            dashBoost.transform.localScale = newScale;
        }
        dashBoostEffect = dashBoost;
    }
    /// <summary>
    /// 플레이어의 대쉬 상태를 종료하도록 요청합니다.
    /// </summary>
    void FE_DashRunEnd()
    {
        StopDashing();
        StopAirDashing();
    }
    /// <summary>
    /// 대쉬가 사용자에 의해 중지될 때 발생합니다.
    /// </summary>
    void FE_DashEndBeg()
    {
        StopMoving();
        SoundEffects[3].Stop();
        SoundEffects[4].Play();
    }
    /// <summary>
    /// 대쉬 점프 모션이 사용자에 의해 완전히 중지되어 대기 상태로 바뀔 때 발생합니다.
    /// </summary>
    void FE_DashEndEnd()
    {
    }

    ///////////////////////////////////////////////////////////////////
    // 벽 타기
    /// <summary>
    /// 벽 타기 시에 발생합니다.
    /// </summary>
    void FE_SlideBeg()
    {
        SoundEffects[6].Play();
    }
    /// <summary>
    /// 벽 점프 시에 발생합니다.
    /// </summary>
    void FE_WallJumpBeg()
    {
        SoundEffects[5].Play();
    }
    /// <summary>
    /// 벽 점프가 종료할 때 발생합니다.
    /// </summary>
    void FE_WallJumpEnd()
    {
        UnblockSliding();
        _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
    }

    #endregion



    #region 보조 메서드를 정의합니다.

    #endregion



    #region 구형 정의를 보관합니다.

    #endregion
}
