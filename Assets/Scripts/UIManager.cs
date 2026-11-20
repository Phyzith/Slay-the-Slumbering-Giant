using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{

// -- VARIABLES --------------------

    // Managers
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CombatManager combatManager;

    // UI
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] public Image playerImage;
    [SerializeField] private Slider enemyHealthSlider;
    [SerializeField] private TMP_Text enemyHealthText;
    [SerializeField] public Image enemyImage;
    [SerializeField] private Slider movementSlider;
    [SerializeField] private TMP_Text movementText;
    [SerializeField] private GameObject gearCardTemplate;
    [SerializeField] private Transform gearPanel;
    [SerializeField] private GameObject lootCardTemplate;
    [SerializeField] private Transform lootPanel;
    [SerializeField] private GameObject toolbarPanel;
    [SerializeField] private Canvas explorationUI;

    // Constants
    public float UI_MOVE_DISTANCE = 9999f;
    public float CHAR_WALK_DURATION = 5f;
    public float UI_MOVE_DURATION = .25f;
    public float ATTACK_ANIM_DURATION = .25f;
    public float ATTACK_ANIM_SIZE_MULT = 2f;
    public float CHAR_OFFSCREEN_X = 1200f; 

    // Original Position Saves
    private Vector3 toolbarPanelOrigPos;
    private Vector3 playerHealthSliderOrigPos;
    private Vector3 playerHealthTextOrigPos;
    private Vector3 enemyHealthSliderOrigPos;
    private Vector3 enemyHealthTextOrigPos;
    

    // Character Original Data
    private Color enemyImageOrigColor;
    private Quaternion enemyImageOrigRot;
    private Vector2 enemyImageOrigPos;
    private Vector2 playerImageOrigPos;

// -- START --------------------

    void Start() 
    {
        // sets the original position of all the combat ui
        toolbarPanelOrigPos = toolbarPanel.transform.position;
        playerHealthSliderOrigPos = playerHealthSlider.transform.position;
        playerHealthTextOrigPos = playerHealthText.transform.position;
        playerImageOrigPos = playerImage.rectTransform.anchoredPosition;
        enemyHealthSliderOrigPos = enemyHealthSlider.transform.position;
        enemyHealthTextOrigPos = enemyHealthText.transform.position;
        enemyImageOrigPos = enemyImage.rectTransform.anchoredPosition;

        // sets character image original data
        enemyImageOrigColor = enemyImage.color;
        enemyImageOrigRot = enemyImage.rectTransform.localRotation;

        // disables all of the combat ui
        HideCombatUI();     
    }

// -- UPDATES --------------------

    void Update()
    {
        updateMovementUI();
        updatePlayerHealthUI();
        updateEnemyHealthUI();
    }

    void updateMovementUI() 
    {
        float percentMovement = (float)playerController.playerMovement/(float)playerController.maxPlayerMovement;

        movementSlider.value = Mathf.MoveTowards(movementSlider.value, percentMovement, .25f * Time.deltaTime);
        movementText.text = $"{playerController.playerMovement} / {playerController.maxPlayerMovement}";
    }

    void updatePlayerHealthUI() 
    {
        float percentHealth = (float)playerController.health/(float)playerController.maxHealth;

        playerHealthSlider.value = Mathf.MoveTowards(playerHealthSlider.value, percentHealth, .25f * Time.deltaTime);
        playerHealthText.text = $"{playerController.health} / {playerController.maxHealth}";
    }

    void updateEnemyHealthUI()
    {
        float percentHealth = (float)combatManager.enemy.health/(float)combatManager.enemy.maxHealth;

        enemyHealthSlider.value = Mathf.MoveTowards(enemyHealthSlider.value, percentHealth, .25f * Time.deltaTime);
        enemyHealthText.text = $"{combatManager.enemy.name}: {combatManager.enemy.health} / {combatManager.enemy.maxHealth}";
    }

// -- CARDS --------------------

    public void createGearCard(string name, string description) 
    {
        GameObject gearCard = Instantiate(gearCardTemplate, gearPanel);
        
        Transform nameText = gearCard.transform.Find("Name");
        Transform descriptionText = gearCard.transform.Find("Description");

        if (nameText != null) {nameText.GetComponent<TMP_Text>().text = name;}
        if (descriptionText != null) {descriptionText.GetComponent<TMP_Text>().text = description;}
    }

    private void createLootCard(string name, string description) 
    {
        GameObject lootCard = Instantiate(lootCardTemplate, lootPanel);
        
        Transform nameText = lootCard.transform.Find("Name");
        Transform descriptionText = lootCard.transform.Find("Description");
        Transform claimButtonTransform = lootCard.transform.Find("ClaimButton");
        Transform discardButtonTransform = lootCard.transform.Find("DiscardButton");
        Button claimButton = claimButtonTransform.GetComponent<Button>();
        Button discardButton = discardButtonTransform.GetComponent<Button>();

        if (nameText != null) {nameText.GetComponent<TMP_Text>().text = name;}
        if (descriptionText != null) {descriptionText.GetComponent<TMP_Text>().text = description;}

        claimButton.onClick.AddListener(() => OnDynamicButtonClicked(claimButton));
        discardButton.onClick.AddListener(() => OnDynamicButtonClicked(discardButton));
    }

    private void loadCharacterSprites(string enemyName)
    {
    playerImage.sprite = Resources.Load<Sprite>("Graphics/Characters/Player");
    enemyImage.sprite = Resources.Load<Sprite>($"Graphics/Characters/{enemyName}");
    }

    private void OnDynamicButtonClicked(Button clickedButton)
    {
        Debug.Log($"({clickedButton.name}) was clicked!");
        
        if (clickedButton.name == "ClaimButton") 
        {
            string description = clickedButton.transform.parent.Find("Description").GetComponent<TextMeshProUGUI>().text;
            createGearCard(combatManager.enemy.weapon.name, description);
            playerController.playerWeapon = new Weapon(combatManager.enemy.weapon.name, combatManager.enemy.weapon.weaponDie);
            Destroy(clickedButton.transform.parent.gameObject);
        } else if (clickedButton.name == "DiscardButton")
        {
            Destroy(clickedButton.transform.parent.gameObject);
        }
    }

// -- CHARACTER ANIMATION --------------------

    public void attackAnimation(bool isPlayer)
    {
        int direction;
        Image character;

        if (isPlayer) 
        {
            direction = 1;
            character = playerImage;
        } else {
            direction = -1;
            character = enemyImage;
        }

        
        Vector3 origPosition = character.transform.position;
        Vector3 origScale = character.transform.localScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(character.transform.DOMove(character.transform.position + new Vector3(100 * direction, 0, 0), ATTACK_ANIM_DURATION/2).SetEase(Ease.InOutQuad));
        seq.Join(character.transform.DOScale(origScale * ATTACK_ANIM_SIZE_MULT, ATTACK_ANIM_DURATION/2));
        seq.Append(character.transform.DOMove(origPosition, ATTACK_ANIM_DURATION/2).SetEase(Ease.InOutQuad));
        seq.Join(character.transform.DOScale(origScale, ATTACK_ANIM_DURATION/2));
    }

    private void characterWalk(Image character, float targetX, float groundY, int numHops, float hopHeight, float duration)
    {
        if (numHops <= 0) return;

        RectTransform rect = character.rectTransform;
        rect.DOKill(); // kill any walk already running on this character

        float startX = rect.anchoredPosition.x;
        float hopDuration = duration / numHops;

        Sequence seq = DOTween.Sequence();

        for (int i = 1; i <= numHops; i++)
        {
            float hopStartX = Mathf.Lerp(startX, targetX, (i - 1) / (float)numHops);
            float hopEndX   = Mathf.Lerp(startX, targetX, i / (float)numHops);

            seq.Append(DOTween.To(
                () => 0f,
                t => rect.anchoredPosition = new Vector2(
                    Mathf.Lerp(hopStartX, hopEndX, t),
                    groundY + Mathf.Sin(t * Mathf.PI) * hopHeight),
                1f,
                hopDuration).SetEase(Ease.Linear));
        }

        seq.SetTarget(rect);                  // lets rect.DOKill() find it
        seq.SetLink(character.gameObject);
        seq.OnComplete(() => rect.anchoredPosition = new Vector2(targetX, groundY)); // snap exactly to ground
    }

    public void resetEnemyImage()
    {
        enemyImage.rectTransform.position = enemyImageOrigPos;
        enemyImage.rectTransform.localRotation = enemyImageOrigRot;
        enemyImage.color = enemyImageOrigColor;
    }

    public void enemyWalkOffscreen() {characterWalk(enemyImage,  CHAR_OFFSCREEN_X,  enemyImageOrigPos.y, 7, 30f, CHAR_WALK_DURATION);}
    public void playerWalkOffscreen() {characterWalk(playerImage, -CHAR_OFFSCREEN_X, playerImageOrigPos.y, 7, 30f, CHAR_WALK_DURATION);}

// -- COMBAT UI --------------------

    public async void disableCombatUI() 
    {
        // enables the exploration ui
        explorationUI.enabled = true;

        // moves the combat ui offscreen
        moveCombatUIOffscreen();

        // waits for the combat ui to be onscreen then disables it
        await Awaitable.WaitForSecondsAsync(CHAR_WALK_DURATION, destroyCancellationToken);
        toolbarPanel.SetActive(false);
        enemyHealthSlider.gameObject.SetActive(false);
        enemyImage.enabled = false;
        playerImage.enabled = false;
        backgroundPanel.SetActive(false);

        // moves all ui onscreen instantly
        enemyImage.rectTransform.DOKill();
        playerImage.rectTransform.DOKill();
        toolbarPanel.transform.position = toolbarPanelOrigPos;
        enemyHealthSlider.transform.position = enemyHealthSliderOrigPos;
        enemyImage.rectTransform.anchoredPosition  = enemyImageOrigPos;
        playerImage.rectTransform.anchoredPosition = playerImageOrigPos;
        backgroundPanel.GetComponent<CanvasGroup>().alpha = 1f;

        //lootEnemy();
    }

    public void enableCombatUI()
    {
        combatManager.setNewEnemy();
        loadCharacterSprites(combatManager.enemy.name);

        // enables all the combat ui
        toolbarPanel.SetActive(true);
        enemyHealthSlider.gameObject.SetActive(true);
        enemyImage.enabled = true;
        playerImage.enabled = true;
        backgroundPanel.SetActive(true);
        explorationUI.enabled = false;

        // moves all ui offscreen instantly
        enemyImage.rectTransform.DOKill();
        playerImage.rectTransform.DOKill();
        toolbarPanel.transform.position = toolbarPanelOrigPos + new Vector3(0, -UI_MOVE_DISTANCE, 0);
        enemyHealthSlider.transform.position = enemyHealthSliderOrigPos + new Vector3(UI_MOVE_DISTANCE, 0, 0);
        enemyImage.rectTransform.anchoredPosition  = new Vector2( CHAR_OFFSCREEN_X, enemyImageOrigPos.y);
        playerImage.rectTransform.anchoredPosition = new Vector2(-CHAR_OFFSCREEN_X, playerImageOrigPos.y);
        backgroundPanel.GetComponent<CanvasGroup>().alpha = 0f;

        // animates the ui onto the screen
        moveCombatUIOnscreen();
    }

    private void moveCombatUIOffscreen() 
    {
        toolbarPanel.transform.DOMove(toolbarPanel.transform.position + new Vector3(0, -UI_MOVE_DISTANCE, 0), UI_MOVE_DURATION).SetEase(Ease.InOutQuad);
        enemyHealthSlider.transform.DOMove(enemyHealthSlider.transform.position + new Vector3(UI_MOVE_DISTANCE, 0, 0), UI_MOVE_DURATION).SetEase(Ease.InOutQuad);
        float offscreenX = ((RectTransform)explorationUI.transform).rect.width; // or the combat canvas
        //characterWalk(enemyImage,  offscreenX,  enemyImageOrigPos.y, 7, 30f, CHAR_WALK_DURATION);
        //characterWalk(playerImage, -offscreenX, playerImageOrigPos.y, 7, 30f, CHAR_WALK_DURATION);

        CanvasGroup canvasGroup = backgroundPanel.GetComponent<CanvasGroup>();
        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, UI_MOVE_DURATION).SetEase(Ease.OutQuad);
    }

    private void moveCombatUIOnscreen() 
    {
        toolbarPanel.transform.DOMove(toolbarPanelOrigPos, UI_MOVE_DURATION).SetEase(Ease.InOutQuad);
        enemyHealthSlider.transform.DOMove(enemyHealthSliderOrigPos, UI_MOVE_DURATION).SetEase(Ease.InOutQuad);
        characterWalk(enemyImage,  enemyImageOrigPos.x,  enemyImageOrigPos.y,  7, 30f, CHAR_WALK_DURATION);
        characterWalk(playerImage, playerImageOrigPos.x, playerImageOrigPos.y, 7, 30f, CHAR_WALK_DURATION);

        CanvasGroup canvasGroup = backgroundPanel.GetComponent<CanvasGroup>();
        canvasGroup.DOKill();
        canvasGroup.DOFade(1f, UI_MOVE_DURATION).SetEase(Ease.OutQuad);
    }

    private void HideCombatUI ()
    {
        toolbarPanel.SetActive(false);
        enemyHealthSlider.gameObject.SetActive(false);
        enemyImage.enabled = false;
        playerImage.enabled = false;
        backgroundPanel.SetActive(false);   
    }

    public void lootEnemy() 
    {
        int numCards = 0;
        lootPanel.gameObject.SetActive(true);

        createLootCard(combatManager.enemy.weapon.name, $"Deals 1 to {combatManager.enemy.weapon.weaponDie} Damage"); // adds the enemies weapon to the loot pool
        numCards++;

        createLootCard(combatManager.enemy.armor.name, $"Protects against 1 to {combatManager.enemy.armor.armorDie} Damage"); // adds the enemies armor to the loot pool
        numCards++;
    }

    public void characterDeath(Image characterImage) {
        int DEATH_DURATION = 2;
        //SetPivotWithoutMoving(characterImage.rectTransform, new Vector2(0.5f, 0f));

        Sequence seq = DOTween.Sequence();

        seq.Append(characterImage.DOColor(Color.red, DEATH_DURATION));
        seq.Join(characterImage.rectTransform.DOLocalRotate(new Vector3(0, 0, -90f), DEATH_DURATION));
        seq.Append(characterImage.DOFade(0f, DEATH_DURATION));
    }

    private void SetPivotWithoutMoving(RectTransform rect, Vector2 newPivot)
    {
        Vector2 size = rect.rect.size;
        Vector2 deltaPivot = rect.pivot - newPivot;
        Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
        
        rect.pivot = newPivot;
        rect.localPosition -= deltaPosition;
    }
}


