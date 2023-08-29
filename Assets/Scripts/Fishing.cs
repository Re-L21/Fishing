using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fishing : MonoBehaviour
{
    [Header("UI Management")]
    public GameObject castingPowerSlider;
    public CanvasGroup fishingModeUI;
    public Slider castingPowerBar;

    [Header("Bobber")]
    public Bobber BobberPrefab;
    public Bobber ThrownBobber { get; private set; } 
    public Transform throwStartPosition;

    [Header("Throwing Bobber")]
    public float minThrowForce = 0f;
    public float maxThrowForce = 0f;

    [Header("Scripts")]
    public MonoBehaviour playerMovement;
    public bool IsBobberOnWater { get; set; } = false;
    public bool IsCasted { get; set; } = false;
    public bool IsFishMode { get; set; } = false;

    Animator animator;

    float maxRodPower = 3f;
    float castRodPower = 0f;

    bool isCastingRod = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !animator.GetBool("isRunning"))
            IsFishMode = true;
        else if (Input.GetKeyDown("x") && !IsCasted)
            IsFishMode = false;

        if (IsFishMode)
        {
            FishingMode();
        }
        else if (!IsFishMode)
        {
            fishingModeUI.alpha = 0;
            playerMovement.enabled = true;
        }

    }

    public void FishingMode()
    {
        fishingModeUI.alpha = 1;

        playerMovement.enabled = false;

        if (!IsCasted )
        {
            if (Input.GetButtonDown("Fire1"))
            {
                isCastingRod = true;
                castRodPower = 0f;
                castingPowerSlider.SetActive(true);
            }

            if (Input.GetButton("Fire1"))
            {
                if (isCastingRod)
                {
                    castRodPower += Time.deltaTime;
                    castingPowerBar.value = castRodPower / maxRodPower;
                }
            }

            if (Input.GetButtonUp("Fire1"))
            {
                IsCasted = true;

                if (isCastingRod)
                {
                    isCastingRod = false;
                    float holdDuration = castRodPower;
                    castRodPower = 0f;

                    if (holdDuration >= maxRodPower) holdDuration = maxRodPower;

                    castingPowerSlider.SetActive(false);
                    animator.SetTrigger("isCasting");
                    animator.SetBool("casting", true);

                    StartCoroutine(ThrowBobberDelay(holdDuration));

                }
            }
        }
    }

    public void ThrowBobber(float throwingPower)
    {
        float throwStrength = throwingPower / maxRodPower;

        Vector3 throwDirection = transform.forward;
        float throwForce = minThrowForce + (throwStrength * (maxThrowForce - minThrowForce));

        ThrownBobber = Instantiate(BobberPrefab, throwStartPosition.position, Quaternion.identity);
        Rigidbody thrownRigidbody = ThrownBobber.GetComponent<Rigidbody>();
        thrownRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        ThrownBobber.OnBobberCollided += OnBobberCollided;

    }

    private void OnBobberCollided(Collision collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            IsBobberOnWater = true;
        }
        else
        {
            Debug.Log($"{collision.gameObject.name}");

            IsBobberOnWater = false;

            animator.SetBool("casting", false);

            IsCasted = false;
            IsFishMode = false;

            Destroy(ThrownBobber.gameObject);
        }
    }

    IEnumerator ThrowBobberDelay(float duration)
    {
        yield return new WaitForSeconds(2.5f) ;
        ThrowBobber(duration);
    }



}
