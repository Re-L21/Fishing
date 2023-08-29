using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Fishing _fishing;
    [SerializeField] private PlayerMovement _player;

    [Header("Canvas / UI")]
    public CanvasGroup fishingModeCanvas;
    public CanvasGroup gameOverCanvas;

    public GameObject castingRodSlider;

    public TMP_Text qteIndicator;
    public TMP_Text qteTimer;
    public TMP_Text fishCountText;
    public TMP_Text gameOverFishCount;

    [Header("Fishing Rod")]
    public GameObject fishingLineDurabilitySlider;
    public Slider fishingLineBar;

    [Header("Fish Bar")]
    public GameObject fishBarGameObjectSlider;
    public Slider fishResistBar;

    [Header("Fishing Line Durability")]
    public Slider fishingLineDurabilityBar;

    [Header("Fish")]
    public GameObject fishModel;
    public float fishStrength = 4f;
    public float minTimeBetweenInteractionChance = 2f;
    public float maxTimeBetweenInteractionChance = 4f;

    GameObject instatiatedFish;
    Transform bobberTransform;
    Vector3 fishRetreiver;

    bool isQteRunning = false;
    bool isFightingFish = false;
    bool isFishCaught = false;
    bool isFishResisting = false;

    float maxFishingLineDurability = 100f;
    float fishingLineDurability = 100f;
    float reelingSpeed = 10f;
    float intervalTime = 0f;

    int fishCountNum = 0;

    void Start()
    {
        fishingModeCanvas.alpha = 0;
        gameOverCanvas.alpha = 0;

        gameOverCanvas.interactable = false;

        castingRodSlider.SetActive(false);
        fishBarGameObjectSlider.SetActive(false);
        fishingLineDurabilitySlider.SetActive(true);

        fishingLineBar.value = 1;
        fishResistBar.value = 1;

        qteIndicator.SetText("");
        qteTimer.SetText("");
        fishCountText.SetText("x " + fishCountNum);

    }

    void Update()
    {

        if(_fishing.IsBobberOnWater && !isQteRunning && !isFightingFish)
        {
            Debug.Log("Coroutine Started");
            StartCoroutine(QteInterval());
        }

        if (!isFishCaught && isFightingFish)
        {
            FishFight();
        }

    }

    #region QTE COROUTINE
    IEnumerator QteInterval()
    {
        isQteRunning = true;
        _player.animator.SetBool("casting", true);
        intervalTime = Mathf.RoundToInt(Random.Range(minTimeBetweenInteractionChance, maxTimeBetweenInteractionChance));

        qteIndicator.SetText("Prepare Yourself");

        yield return new WaitForSeconds(intervalTime);

        Debug.Log("StartQTE coroutine started");
        StartCoroutine(StartQte());

    }

    IEnumerator StartQte()
    {
        float timer = intervalTime;
        while (timer > 0f)
        {
            qteIndicator.SetText("ITS BITING!!!!");
            qteTimer.SetText("QTE TIMER : " + Mathf.RoundToInt(timer));

            if (Input.GetButtonUp("Fire1"))
            {
                _player.animator.SetTrigger("isBitten");

                qteIndicator.SetText("");
                qteTimer.SetText("");

                isQteRunning = false;
                isFightingFish = true;

                SpawnFish(); 
                StartCoroutine(FishResistingInterval());
                _fishing.IsBobberOnWater = false;

                yield break;
            } 

            timer -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("Ran out of time") ;
        intervalTime = Mathf.RoundToInt(Random.Range(minTimeBetweenInteractionChance, maxTimeBetweenInteractionChance));
        StartCoroutine(QteInterval());
    }
    #endregion

    #region FIGHTING FISH COROUTINE
    IEnumerator FishResistingInterval()
    {
        intervalTime = Mathf.RoundToInt(Random.Range(minTimeBetweenInteractionChance, maxTimeBetweenInteractionChance));
        float untilNextResistTimer = intervalTime;

        qteIndicator.SetText("The fish Is calm, Reel it in !!!");
        while(untilNextResistTimer > 0f)
        {
            qteTimer.SetText("Until Next Resist : " + Mathf.RoundToInt(untilNextResistTimer));
            untilNextResistTimer -= Time.deltaTime;
            yield return null;
        }

        StartCoroutine(FishResisting());

    }

    IEnumerator FishResisting()
    {
        float timer = intervalTime;

        while(timer > 0f)
        {
            isFishResisting = true;
            qteIndicator.SetText("Fish Is Resisting!!!!");
            qteTimer.SetText("");

            fishBarGameObjectSlider.SetActive(true);
            fishResistBar.value = timer / intervalTime;

            timer -= Time.deltaTime;
            yield return null;
        }

        fishBarGameObjectSlider.SetActive(false);

        isFishResisting = false;
        intervalTime = Mathf.RoundToInt(Random.Range(minTimeBetweenInteractionChance, maxTimeBetweenInteractionChance));
        StartCoroutine(FishResistingInterval());

    }

    #endregion

    void FishFight()
    {
        if(Input.GetButtonDown("Fire1"))
            _player.animator.SetBool("isReeling", true);

        if(Input.GetButton("Fire1"))
        {
            ReelFish(isFishResisting);
        }
        
        if(Input.GetButtonUp("Fire1"))
            _player.animator.SetBool("isReeling", false);
    }


    public void SpawnFish()
    {
        bobberTransform = _fishing.ThrownBobber.transform;
        instatiatedFish = Instantiate(fishModel, bobberTransform.position, Quaternion.identity);

        bobberTransform.gameObject.SetActive(false); 
        fishRetreiver = new Vector3(_fishing.transform.position.x, instatiatedFish.transform.position.y, _fishing.transform.position.z);

    }

    public void ReelFish(bool _isFishResisting)
    {
        float totalSpeed = 0f;
        float distance = Vector3.Distance(fishRetreiver, instatiatedFish.transform.position);

        if (fishingLineDurability > 0f && distance > 2)
        {
            if (_isFishResisting)
            {
                totalSpeed = reelingSpeed / (fishStrength * 3) * Time.deltaTime;
                fishingLineDurability -= reelingSpeed * 3 * Time.deltaTime;
            }
            else
            {
                totalSpeed = reelingSpeed / fishStrength * Time.deltaTime;
            }

        }
        else if (distance <= 2 && fishingLineDurability > 0f)
        {
            isFishCaught = true;
            isFightingFish = false;

            Destroy(instatiatedFish);
            Destroy(bobberTransform.gameObject);

            fishBarGameObjectSlider.SetActive(false);

            qteIndicator.SetText("SUCCESS!!!");
            qteTimer.SetText("You just catch a fish. wow :O");

            fishCountNum++;
            fishCountText.SetText("x " + fishCountNum);

            _player.animator.SetBool("isReeling", false);
            StopAllCoroutines();

            StartCoroutine(ResetFishingState());
        }
        else if (fishingLineDurability <= 0f)
        {
            GameOver();
        }

        fishingLineBar.value = fishingLineDurability / maxFishingLineDurability;
        instatiatedFish.transform.position = Vector3.MoveTowards(instatiatedFish.transform.position, fishRetreiver, totalSpeed);

    }

    IEnumerator ResetFishingState()
    {
        yield return new WaitForSeconds(1f);
        _fishing.IsCasted = false;
        _fishing.IsFishMode = false;

        qteIndicator.SetText("");
        qteTimer.SetText("");

        isFishCaught = false;

        intervalTime = 0f;

        fishResistBar.value = 1;

        yield return null;
        StopAllCoroutines();

    }

    public void GameOver()
    {

        StopAllCoroutines();

        gameOverCanvas.alpha = 1;
        gameOverCanvas.interactable = true;

        qteIndicator.SetText("GAME OVER!!!");
        qteTimer.SetText("Well this is it...");
        gameOverFishCount.SetText("x " +fishCountNum);

        _player.animator.SetBool("isRunning", false);
        _player.animator.SetBool("isReeling", false);
        _player.animator.ResetTrigger("isCasting");
        _player.animator.ResetTrigger("isBitten");

    }

}
