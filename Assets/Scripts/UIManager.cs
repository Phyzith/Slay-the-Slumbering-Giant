using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private Image playerImage;
    [SerializeField] private Slider enemyHealthSlider;
    [SerializeField] private TMP_Text enemyHealthText;
    [SerializeField] private Image enemyImage;
    [SerializeField] private Slider movementSlider;
    [SerializeField] private TMP_Text movementText;
    [SerializeField] private GameObject gearCardTemplate;
    [SerializeField] private Transform gearPanel;
    [SerializeField] private GameObject lootCardTemplate;
    [SerializeField] private Transform lootPanel;
    [SerializeField] private GameObject toolbarPanel;
    [SerializeField] private Canvas explorationUI;

    public float uiMoveDistance = 9999f;
    public float characterWalkDuration = 5f;
    public float uiMoveDuration = .25f;
    private Vector3 toolbarPanelOrigPos;
    private Vector3 playerHealthSliderOrigPos;
    private Vector3 playerHealthTextOrigPos;
    private Vector2 playerImageOrigPos;
    private Vector3 enemyHealthSliderOrigPos;
    private Vector3 enemyHealthTextOrigPos;
    private Vector2 enemyImageOrigPos;

    public float attackAnimDuration = .25f;
    public float attackAnimSizeMultiplier = 2f;

    // buttons


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

        // disables all of the combat ui
        toolbarPanel.SetActive(false);
        enemyHealthSlider.gameObject.SetActive(false);
        enemyImage.enabled = false;
        playerImage.enabled = false;
        backgroundPanel.SetActive(false);        
    }

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

    public void createGearCard(string name, string description) 
    {
        GameObject gearCard = Instantiate(gearCardTemplate, gearPanel);
        
        Transform nameText = gearCard.transform.Find("Name");
        Transform descriptionText = gearCard.transform.Find("Description");

        if (nameText != null) {nameText.GetComponent<TMP_Text>().text = name;}
        if (descriptionText != null) {descriptionText.GetComponent<TMP_Text>().text = description;}
    }

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
        seq.Append(character.transform.DOMove(character.transform.position + new Vector3(100 * direction, 0, 0), attackAnimDuration/2).SetEase(Ease.InOutQuad));
        seq.Join(character.transform.DOScale(origScale * attackAnimSizeMultiplier, attackAnimDuration/2));
        seq.Append(character.transform.DOMove(origPosition, attackAnimDuration/2).SetEase(Ease.InOutQuad));
        seq.Join(character.transform.DOScale(origScale, attackAnimDuration/2));
    }

    public async void disableCombatUI() 
    {
        moveCombatUIOffscreen(); // moves the combat ui offscreen

        // waits for the combat ui to be onscreen then disables it
        await Awaitable.WaitForSecondsAsync(characterWalkDuration, destroyCancellationToken);
        toolbarPanel.SetActive(false);
        enemyHealthSlider.gameObject.SetActive(false);
        enemyImage.enabled = false;
        playerImage.enabled = false;
        backgroundPanel.SetActive(false);

        moveCombatUIOnscreen(); // moves the combat ui onscreen
        explorationUI.enabled = true;

        //lootEnemy();
    }

    public async void enableCombatUI() 
    {
        combatManager.setNewEnemy(); // resets the enemy that the player will be fighting
        
            moveCombatUIOffscreen(); // moves the combat ui offscreen
            
        loadCharacterSprites(combatManager.enemy.name);
        // waits for the combat ui to be onscreen then enables it
        await Awaitable.WaitForSecondsAsync(characterWalkDuration, destroyCancellationToken);
        toolbarPanel.SetActive(true);
        enemyHealthSlider.gameObject.SetActive(true);
        enemyImage.enabled = true;
        playerImage.enabled = true;
        backgroundPanel.SetActive(true);

        moveCombatUIOnscreen(); // moves the combat ui onscreen
        explorationUI.enabled = false;
    }

    private void moveCombatUIOffscreen() 
    {
        toolbarPanel.transform.DOMove(toolbarPanel.transform.position + new Vector3(0, -uiMoveDistance, 0), uiMoveDuration).SetEase(Ease.InOutQuad);
        enemyHealthSlider.transform.DOMove(enemyHealthSlider.transform.position + new Vector3(uiMoveDistance, 0, 0), uiMoveDuration).SetEase(Ease.InOutQuad);
        characterWalk(enemyImage, new Vector2(uiMoveDistance, 0), 7, 30f, characterWalkDuration);
        characterWalk(playerImage, new Vector2(-uiMoveDistance, 0), 7, 30f, characterWalkDuration);

        CanvasGroup canvasGroup = backgroundPanel.GetComponent<CanvasGroup>();
        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, uiMoveDuration).SetEase(Ease.OutQuad);
    }

    private void moveCombatUIOnscreen() 
    {
        toolbarPanel.transform.DOMove(toolbarPanelOrigPos, uiMoveDuration).SetEase(Ease.InOutQuad);
        enemyHealthSlider.transform.DOMove(enemyHealthSliderOrigPos, uiMoveDuration).SetEase(Ease.InOutQuad);
        characterWalk(enemyImage, enemyImageOrigPos, 7, 30f, 5f);
        characterWalk(playerImage, playerImageOrigPos, 7, 30f, 5f);

        CanvasGroup canvasGroup = backgroundPanel.GetComponent<CanvasGroup>();
        canvasGroup.DOKill();
        canvasGroup.DOFade(1f, uiMoveDuration).SetEase(Ease.OutQuad);
    }

    private void characterWalk(Image character, Vector2 targetPosition, int numHops, float hopHeight, float duration)
    {
        if (numHops <= 0) return;

        RectTransform rect = character.rectTransform;

        float startX = rect.anchoredPosition.x;
        float targetX = targetPosition.x;
        float y = rect.anchoredPosition.y;
        float hopDuration = duration / numHops;

        Sequence seq = DOTween.Sequence();

        for (int i = 1; i <= numHops; i++)
        {
            float nextX = Mathf.Lerp(startX, targetX, i / (float)numHops);
            seq.Append(rect.DOJumpAnchorPos(new Vector2(nextX, y), hopHeight, 1, hopDuration).SetEase(Ease.Linear));
        }
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
    
    
}


