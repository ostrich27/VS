using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UITreasureChest : MonoBehaviour
{
    public static UITreasureChest instance;
    PlayerCollector collector;
    TreasureChest currentChest;
    TreasureChestDropProfile dropProfile;

    [Header("Visual Elements")]
    public GameObject openingVFX;
    public GameObject beamVFX;
    public GameObject fireworks;
    public GameObject doneButton;
    public GameObject curvedBeams;
    public List<ItemDisplays> items;
    Color originalColor = new Color32(0x42, 0x41, 0x87, 255);

    [Header("UI Elements")]
    public GameObject chestCover;
    public GameObject chestButton;

    [Header("UI Components")]
    public Image chestPanel;
    public TextMeshProUGUI coinText;
    private float coins;

    //internal states
    private List<Sprite> icons = new List<Sprite>();
    private bool isAnimating = false;
    private Coroutine chestSequenceCoroutine;

    //audio
    private AudioSource audiosource;
    public AudioClip pickUpSound;

    [System.Serializable]
    public struct ItemDisplays
    {
        public GameObject beam;
        public Image spriteImage;
        public GameObject sprite;
        public GameObject weaponBeam;
    }


    private void Awake()
    {
        audiosource = GetComponent<AudioSource>();
        gameObject.SetActive(false);

        //ensure only 1 instance can exist in the scene
        if (instance != null && instance != this)
        {
            Debug.LogWarning("More than 1 UI Treasure Chest is found. it has been deleted.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public static void Activate(PlayerCollector collector, TreasureChest chest)
    {
        if (!instance) Debug.LogWarning("No treasure chest UI GameObject found.");

        //save the important variables
        instance.collector = collector;
        instance.currentChest = chest;
        instance.dropProfile = chest.GetCurrentDropProfile();
        Debug.Log(instance.dropProfile);

        //activate the GameObject
        GameManager.instance.ChangeState(GameManager.GameState.TreasureChest);
        instance.gameObject.SetActive(true);
    }

    public static void NotifyItemReceived(Sprite icon)
    {
        //includes a warning message informing the user of what the issue is if
        //we are unable to update this class with the icon
        if (instance)
        {
            instance.icons.Add(icon);
        }
        else
        {
            Debug.LogWarning("No instance of UITreasureChest exists. Unable to update treasure chest UI.");
        }
    }


    //logic for chest to flash
    private IEnumerator FlashWhite(Image image, int times, float flashDuration = 0.2f)
    {
        originalColor = image.color;

        //flashes the chest panel for x amount of times
        for (int i = 0; i < times; i++)
        {
            image.color = Color.white;
            yield return new WaitForSecondsRealtime(flashDuration);

            image.color = originalColor;
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    IEnumerator ActivateCurvedBeams(float spawnTime)
    {
        yield return new WaitForSecondsRealtime(spawnTime);
        curvedBeams.SetActive(true);
    }

    IEnumerator HandleCoinDisplay(float maxCoins)
    {
        coinText.gameObject.SetActive(true);
        float elapsedTime = 0;
        coins = maxCoins;

        //coin rolling up animation and will stop when it has reached maxcoins
        while (elapsedTime < maxCoins)
        {
            elapsedTime += Time.unscaledDeltaTime * 20f;
            coinText.text = string.Format("{0:F2}", elapsedTime);
            yield return null;
        }

        //only activate the done button when coins reach max
        yield return new WaitForSecondsRealtime(2f);
        doneButton.SetActive(true);
    }

    //display beams
    private void SetupBeam(int index)
    {
        items[index].weaponBeam.SetActive(true);
        items[index].beam.SetActive(true);
        items[index].spriteImage.sprite = icons[index];
        items[index].beam.GetComponent<Image>().color = dropProfile.beamColors[index];
    }

    //display delayed beams
    private IEnumerator ShowDelayedBeams(int startIndex, int endIndex)
    {
        yield return new WaitForSecondsRealtime(dropProfile.delayTime);
        for (int i = startIndex; i < endIndex; i++)
        {
            SetupBeam(i);
        }
    }

    public void DisplayerBeam(float noOfBeams)
    {
        int delayedStartIndex = Mathf.Max(0, (int)noOfBeams - dropProfile.delayedBeams); //ensure beams do not go out of index

        //show immediate beams
        for (int i = 0; i < delayedStartIndex; i++)
        {
            SetupBeam(i);
        }

        //delay the rest
        if(dropProfile.delayedBeams > 0)
            StartCoroutine(ShowDelayedBeams(delayedStartIndex, (int)noOfBeams));

        StartCoroutine(DisplayItems(noOfBeams));
    }

    private IEnumerator DisplayItems(float noOfBeams)
    {
        yield return new WaitForSecondsRealtime(dropProfile.animDuration);

        if(noOfBeams == 5)
        {
            //show first item
            items[0].weaponBeam.SetActive(false);
            items[0].sprite.SetActive(true);
            yield return new WaitForSecondsRealtime(0.3f);

            //show second and third at the same time
            for (int i = 1; i <= 2; i++)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
                yield return new WaitForSecondsRealtime(0.3f);
            }

            //show fourth and fifth at the same time
            for (int i = 3; i <= 4; i++)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
                yield return new WaitForSecondsRealtime(0.3f);
            }
        }
        else
        {
            //fallback for other item counts - show normally one by one
            for (int i = 0; i < noOfBeams; i++)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
                yield return new WaitForSecondsRealtime(0.3f);
            }
        }
    }

    //VFX logic
    public IEnumerator Open()
    {
        //trigger if hasFireworks beam is true
        if(dropProfile.hasFireworks)
        {
            isAnimating = false; // if there are fireworks ensure it can't be skipped
            StartCoroutine(FlashWhite(chestPanel, 5)); // or whatever UI element you want to flash
            fireworks.SetActive(true);
            yield return new WaitForSecondsRealtime(dropProfile.fireworksDelay);
        }

        isAnimating = true; //allow skipping of animations

        //trigger if hasCurved beam is true
        if (dropProfile.hasCurvedBeams)
        {
            StartCoroutine(ActivateCurvedBeams(dropProfile.curvedBeamsSpawnTime));
        }

        //set the coins to be received
        StartCoroutine(HandleCoinDisplay(Random.Range(dropProfile.minCoins, dropProfile.maxCoins)));

        DisplayerBeam(dropProfile.noOfItems);
        openingVFX.SetActive(true);
        beamVFX.SetActive(true);

        yield return new WaitForSecondsRealtime(dropProfile.animDuration); // time VFX will be active
        openingVFX.SetActive(false);
    }

    //activates animations
    public void Begin()
    {
        chestCover.SetActive(false);
        chestButton.SetActive(false);
        chestSequenceCoroutine = StartCoroutine(Open());
        audiosource.clip = dropProfile.OpeningSound;
        audiosource.Play();
    }

    private void SkipToRewards()
    {
        if(chestSequenceCoroutine != null)
            StopCoroutine(chestSequenceCoroutine);

        StopAllCoroutines(); // halt all coroutines

        //immediately show all beams and icons
        for(int i = 0; i < icons.Count; i++)
        {
            SetupBeam(i);
            items[i].weaponBeam.SetActive(false);
            items[i].sprite.SetActive(true);
        }

        //immediately show coin value
        coinText.gameObject.SetActive(true);
        coinText.text = coins.ToString("F2");
        doneButton.SetActive(true);
        openingVFX.SetActive(false);
        isAnimating = false;
        chestPanel.color = originalColor;


        //skip to the last 1 second of the audio
        if(audiosource != null && dropProfile.OpeningSound != null)
        {
            audiosource.clip = dropProfile.OpeningSound;

            float skipToTime = Mathf.Max(0, audiosource.clip.length - 3.55f);
            audiosource.time = skipToTime;
            audiosource.Play();
        }
    }

    private void Update()
    {
        //only allow skipping of animation when animation is playing and esc is pressed
        if(isAnimating && Input.GetButtonDown("Cancel"))
        {
            SkipToRewards();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            TryPressButton(chestButton);
            TryPressButton(doneButton);

        }
    }

    private void TryPressButton(GameObject buttonObj)
    {
        if(buttonObj.activeInHierarchy)
        {
            Button btn = buttonObj.GetComponent<Button>();
            if(btn != null && btn.interactable)
            {
                btn.onClick.Invoke();
            }
        }
    }

    public void CloseUI()
    {
        //display coins earned
        collector.AddCoins(coins);

        //reset UI & VFX to initial state
        chestCover.SetActive(true);
        chestButton.SetActive(true);
        icons.Clear();
        beamVFX.SetActive(false);
        coinText.gameObject.SetActive(false);
        gameObject.SetActive(false);
        doneButton.SetActive(false);
        fireworks.SetActive(false);
        curvedBeams.SetActive(false);
        ResetDisplay();

        //reset audio
        audiosource.clip = pickUpSound;
        audiosource.time = 0f;
        audiosource.Play();

        isAnimating = false;

        GameManager.instance.ChangeState(GameManager.GameState.Gameplay);
        currentChest.NotifyComplete();
        Time.timeScale = 1f;
    }

    private void ResetDisplay()
    {
        //reset all item displays
        foreach(var item in items)
        {
            item.beam.SetActive(false);
            item.sprite.SetActive(false);
            item.spriteImage.sprite = null;
        }
        dropProfile = null;
        icons.Clear();
    }
}
