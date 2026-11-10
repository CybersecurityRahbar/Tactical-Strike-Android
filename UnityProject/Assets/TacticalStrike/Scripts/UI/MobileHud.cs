using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TacticalStrike
{
    public sealed class MobileHud : MonoBehaviour
    {
        public static MobileHud Active { get; private set; }
        private FpsPlayerController player;
        private Text weaponText;
        private Text messageText;
        private Text statusText;
        private Text staminaText;
        private Image staminaFill;
        private GameObject mountButton;
        private Text mountButtonLabel;
        private bool mountedWeaponMode;
        private bool sprintToggle;
        private Image sprintButtonImage;
        private Image suppressionVignette;
        private GameObject settingsPanel;
        private GameObject pausePanel;
        private float messageTimer;

        private static Sprite circleSprite;
        private static Sprite suppressionSprite;

        public static MobileHud Create(FpsPlayerController owner, Transform parent)
        {
            GameObject root = new GameObject("Tactical HUD - mobile landscape", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            MobileHud hud = root.AddComponent<MobileHud>();
            hud.player = owner;
            hud.Build();
            return hud;
        }

        private void Awake()
        {
            Active = this;
        }

        private void OnDestroy()
        {
            if (Active == this) Active = null;
        }

        private void Build()
        {
            EnsureEventSystem();
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform root = transform as RectTransform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            // The transparent look surface belongs only to touch devices. Leaving it
            // active in the desktop preview would intercept browser mouse clicks.
            if (Application.isMobilePlatform)
            {
                GameObject look = CreateImage("Right-side look surface", root, new Color(1f, 1f, 1f, 0.001f),
                    new Vector2(0.37f, 0.08f), new Vector2(1f, 0.96f), Vector2.zero, Vector2.zero);
                look.GetComponent<Image>().raycastTarget = true;
                look.AddComponent<TouchLookArea>().Initialize(player);
            }

            BuildSuppressionVignette(root);
            BuildStatusPanel(root);
            BuildCrosshair(root);
            if (Application.isMobilePlatform)
            {
                BuildJoystick(root);
                BuildCombatButtons(root);
            }
            else
            {
                BuildDesktopHelp(root);
            }
            BuildTopButtons(root);
            BuildSettings(root);
            BuildPausePanel(root);
        }

        private void BuildSuppressionVignette(RectTransform root)
        {
            GameObject overlay = CreateImage("Near-miss suppression vignette", root,
                new Color(0.92f, 0.10f, 0.035f, 0f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            suppressionVignette = overlay.GetComponent<Image>();
            suppressionVignette.sprite = SuppressionSprite();
            suppressionVignette.raycastTarget = false;
            suppressionVignette.type = Image.Type.Simple;
        }

        private static Sprite SuppressionSprite()
        {
            if (suppressionSprite != null) return suppressionSprite;
            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            texture.name = "Suppression - soft radial edge vignette";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x / (float)(size - 1) - 0.5f) * 2f;
                    float ny = (y / (float)(size - 1) - 0.5f) * 2f;
                    float radius = Mathf.Sqrt(nx * nx + ny * ny);
                    float edge = Mathf.Clamp01((radius - 0.54f) / 0.46f);
                    float alpha = edge * edge * (3f - 2f * edge);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            suppressionSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), 100f);
            suppressionSprite.name = "Suppression vignette sprite";
            return suppressionSprite;
        }

        public void SetSuppression(float value)
        {
            if (suppressionVignette == null) return;
            float strength = Mathf.Clamp01(value);
            suppressionVignette.color = new Color(0.94f, 0.12f, 0.045f, strength * 0.52f);
        }

        private void BuildStatusPanel(RectTransform root)
        {
            // For a top-left anchored rect, the lower edge must be farther below
            // the anchor than the upper edge; the old values produced negative height.
            GameObject panel = CreateImage("Mission status", root, new Color(0.025f, 0.035f, 0.032f, 0.86f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -148f), new Vector2(550f, -22f));
            statusText = CreateText("NORTHWIND VALLEY / SECTOR 01    -    FIELD OPERATIONS", panel.transform,
                22, new Color(0.92f, 0.88f, 0.77f), TextAnchor.MiddleLeft,
                new Vector2(0f, 0.66f), new Vector2(1f, 1f), new Vector2(18f, 0f), new Vector2(-16f, -4f));
            weaponText = CreateText("AK-74M   |   5.45×39 mm   |   30 / 30   |   RES 120   |   FMJ",
                panel.transform, 20, new Color(0.94f, 0.70f, 0.36f), TextAnchor.MiddleLeft,
                new Vector2(0f, 0.32f), new Vector2(1f, 0.66f), new Vector2(18f, 0f), new Vector2(-16f, 0f));
            messageText = CreateText("", panel.transform, 18, new Color(1f, 0.79f, 0.46f),
                TextAnchor.MiddleRight, new Vector2(0.62f, 0.10f), new Vector2(1f, 0.32f),
                new Vector2(0f, 0f), new Vector2(-18f, 0f));
            messageText.gameObject.SetActive(false);

            GameObject staminaTrack = CreateImage("Stamina track", panel.transform,
                new Color(0.08f, 0.095f, 0.075f, 0.95f),
                new Vector2(0f, 0.08f), new Vector2(0.62f, 0.25f),
                new Vector2(18f, 2f), new Vector2(-8f, -2f));
            staminaTrack.GetComponent<Image>().raycastTarget = false;
            GameObject fill = CreateImage("Stamina level", staminaTrack.transform,
                new Color(0.55f, 0.68f, 0.39f, 1f),
                Vector2.zero, new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            fill.GetComponent<Image>().raycastTarget = false;
            staminaFill = fill.GetComponent<Image>();
            staminaText = CreateText("STAMINA 100%", panel.transform, 13,
                new Color(0.82f, 0.84f, 0.72f), TextAnchor.MiddleRight,
                new Vector2(0.63f, 0.06f), new Vector2(1f, 0.27f),
                new Vector2(0f, 0f), new Vector2(-18f, 0f));
        }

        private void BuildCrosshair(RectTransform root)
        {
            Text crosshair = CreateText("+", root, 31, new Color(0.95f, 0.93f, 0.84f, 0.9f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-18f, -20f), new Vector2(36f, 40f));
        }

        private void BuildDesktopHelp(RectTransform root)
        {
            CreateText("WASD MOVE  |  MOUSE LOOK  |  LMB FIRE  |  HOLD RMB AIM  |  SHIFT SPRINT  |  C CROUCH  |  Z PRONE  |  SPACE JUMP  |  Q / E LEAN  |  R RELOAD  |  T AMMO  |  F INTERACT  |  ESC RELEASE",
                root, 15, new Color(0.94f, 0.91f, 0.82f, 0.88f), TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0f), new Vector2(0.92f, 0f),
                new Vector2(0f, 12f), new Vector2(0f, 48f));
        }

        private void BuildJoystick(RectTransform root)
        {
            GameObject baseObject = CreateImage("Tactical movement stick", root,
                new Color(0.045f, 0.065f, 0.055f, 0.70f),
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(26f, 26f), new Vector2(258f, 258f));
            Image baseImage = baseObject.GetComponent<Image>();
            baseImage.sprite = Circle();
            baseImage.type = Image.Type.Simple;
            GameObject innerRing = CreateImage("Movement stick inner ring", baseObject.transform,
                new Color(0.55f, 0.61f, 0.46f, 0.12f),
                new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.82f),
                Vector2.zero, Vector2.zero);
            innerRing.GetComponent<Image>().sprite = Circle();
            GameObject knobObject = CreateImage("Movement stick thumb", baseObject.transform,
                new Color(0.70f, 0.72f, 0.59f, 0.86f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-46f, -46f), new Vector2(46f, 46f));
            knobObject.GetComponent<Image>().sprite = Circle();
            baseObject.AddComponent<TouchJoystick>().Initialize(player,
                knobObject.transform as RectTransform, 86f);
        }

        private void BuildCombatButtons(RectTransform root)
        {
            // Dual-thumb tactical layout: movement with the left thumb, view with the right;
            // independent hold actions support simultaneous ADS and fire.
            AddTacticalHoldButton(root, "FIRE", new Vector2(1f, 0f),
                new Vector2(-164f, 164f), new Vector2(146f, 146f),
                new Color(0.48f, 0.15f, 0.10f, 0.92f), player.SetFireHeld, 19, true);
            AddTacticalHoldButton(root, "ADS", new Vector2(1f, 0f),
                new Vector2(-326f, 206f), new Vector2(94f, 94f),
                new Color(0.24f, 0.31f, 0.24f, 0.90f), player.SetAim, 16, true);
            AddTacticalHoldButton(root, "LEAN\nL", new Vector2(1f, 0f),
                new Vector2(-510f, 178f), new Vector2(76f, 76f),
                new Color(0.18f, 0.23f, 0.19f, 0.88f), held => player.SetLean(held ? -1f : 0f), 12, true);
            AddTacticalHoldButton(root, "LEAN\nR", new Vector2(1f, 0f),
                new Vector2(-420f, 178f), new Vector2(76f, 76f),
                new Color(0.18f, 0.23f, 0.19f, 0.88f), held => player.SetLean(held ? 1f : 0f), 12, true);

            AddTacticalTapButton(root, "JUMP", new Vector2(1f, 0f),
                new Vector2(-326f, 78f), new Vector2(78f, 72f),
                new Color(0.24f, 0.30f, 0.23f, 0.92f), player.RequestJump, 13, true);
            AddTacticalTapButton(root, "CROUCH", new Vector2(1f, 0f),
                new Vector2(-416f, 78f), new Vector2(78f, 72f),
                new Color(0.21f, 0.27f, 0.22f, 0.92f), player.ToggleCrouch, 11, true);
            AddTacticalTapButton(root, "PRONE", new Vector2(1f, 0f),
                new Vector2(-506f, 78f), new Vector2(78f, 72f),
                new Color(0.21f, 0.27f, 0.22f, 0.92f), player.ToggleProne, 11, true);
            AddTacticalTapButton(root, "RELOAD", new Vector2(1f, 0f),
                new Vector2(-164f, 68f), new Vector2(76f, 66f),
                new Color(0.18f, 0.23f, 0.19f, 0.93f), player.ReloadCurrentWeapon, 11, false);
            AddTacticalTapButton(root, "AMMO", new Vector2(1f, 0f),
                new Vector2(-250f, 68f), new Vector2(76f, 66f),
                new Color(0.18f, 0.23f, 0.19f, 0.93f), player.Weapon.CycleAmmo, 12, false);

            GameObject sprint = AddTacticalTapButton(root, "SPRINT", new Vector2(0f, 0f),
                new Vector2(286f, 58f), new Vector2(102f, 78f),
                new Color(0.22f, 0.29f, 0.22f, 0.92f), () =>
                {
                    sprintToggle = !sprintToggle;
                    player.SetSprint(sprintToggle);
                    if (sprintButtonImage != null)
                        sprintButtonImage.color = sprintToggle
                            ? new Color(0.52f, 0.48f, 0.23f, 0.96f)
                            : new Color(0.22f, 0.29f, 0.22f, 0.92f);
                }, 13, false);
            sprintButtonImage = sprint.GetComponent<Image>();

            mountButton = AddTacticalTapButton(root, "MOUNT", new Vector2(1f, 0f),
                new Vector2(-596f, 178f), new Vector2(76f, 76f),
                new Color(0.39f, 0.31f, 0.16f, 0.95f), player.InteractWithMountedWeapon, 12, true);
            mountButtonLabel = mountButton.GetComponentInChildren<Text>();
            mountButton.SetActive(false);
        }

        private GameObject AddTacticalTapButton(RectTransform parent, string label, Vector2 anchor,
            Vector2 offset, Vector2 size, Color tint, UnityEngine.Events.UnityAction action,
            int fontSize, bool circular)
        {
            GameObject go = CreateImage(label + " tactical control", parent, tint, anchor, anchor,
                offset, offset + size);
            Image image = go.GetComponent<Image>();
            if (circular) image.sprite = Circle();
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.72f, 0.76f, 0.62f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.onClick.AddListener(action);
            CreateText(label, go.transform, fontSize, new Color(0.96f, 0.95f, 0.87f),
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
                new Vector2(4f, 3f), new Vector2(-4f, -3f));
            return go;
        }

        private GameObject AddTacticalHoldButton(RectTransform parent, string label, Vector2 anchor,
            Vector2 offset, Vector2 size, Color tint, System.Action<bool> callback,
            int fontSize, bool circular)
        {
            GameObject go = CreateImage(label + " tactical hold", parent, tint, anchor, anchor,
                offset, offset + size);
            Image image = go.GetComponent<Image>();
            if (circular) image.sprite = Circle();
            HoldButton hold = go.AddComponent<HoldButton>();
            hold.OnHoldChanged = callback;
            CreateText(label, go.transform, fontSize, new Color(0.98f, 0.96f, 0.88f),
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
                new Vector2(4f, 3f), new Vector2(-4f, -3f));
            return go;
        }

        private void BuildTopButtons(RectTransform root)
        {
            AddButton(root, "PAUSE", new Vector2(1f, 1f), new Vector2(-118f, -68f), new Vector2(100f, 52f),
                TogglePause, new Color(0.10f, 0.14f, 0.12f, 0.86f), 16);
            AddButton(root, "SETTINGS", new Vector2(1f, 1f), new Vector2(-236f, -68f), new Vector2(108f, 52f),
                () => settingsPanel.SetActive(!settingsPanel.activeSelf),
                new Color(0.10f, 0.14f, 0.12f, 0.86f), 15);
        }

        private void BuildSettings(RectTransform root)
        {
            settingsPanel = CreateImage("Settings panel", root, new Color(0.025f, 0.035f, 0.032f, 0.94f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220f, -200f), new Vector2(220f, 200f));
            AddButton(settingsPanel.transform as RectTransform, "NIGHT MODE", new Vector2(0.5f, 0.65f),
                new Vector2(-130f, -26f), new Vector2(260f, 58f),
                () => TacticalStrikeBootstrap.Instance?.ToggleNightMode(),
                new Color(0.24f, 0.25f, 0.19f, 1f), 18);
            AddButton(settingsPanel.transform as RectTransform, "LOOK SENSITIVITY  -", new Vector2(0.5f, 0.42f),
                new Vector2(-130f, -26f), new Vector2(260f, 58f),
                () => player.AdjustSensitivity(-0.01f),
                new Color(0.16f, 0.20f, 0.17f, 1f), 16);
            AddButton(settingsPanel.transform as RectTransform, "LOOK SENSITIVITY  +", new Vector2(0.5f, 0.28f),
                new Vector2(-130f, -26f), new Vector2(260f, 52f),
                () => player.AdjustSensitivity(0.01f),
                new Color(0.16f, 0.20f, 0.17f, 1f), 16);
            AddButton(settingsPanel.transform as RectTransform, "CLOSE SETTINGS", new Vector2(0.5f, 0.06f),
                new Vector2(-130f, -24f), new Vector2(260f, 48f),
                () =>
                {
                    settingsPanel.SetActive(false);
                    if (Time.timeScale <= 0f && pausePanel != null)
                        pausePanel.SetActive(true);
                },
                new Color(0.24f, 0.25f, 0.19f, 1f), 16);
            settingsPanel.SetActive(false);
        }

        private void BuildPausePanel(RectTransform root)
        {
            pausePanel = CreateImage("Tactical pause panel", root, new Color(0.025f, 0.035f, 0.032f, 0.96f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-230f, -180f), new Vector2(230f, 180f));
            CreateText("MISSION PAUSED", pausePanel.transform, 27,
                new Color(0.94f, 0.76f, 0.48f), TextAnchor.MiddleCenter,
                new Vector2(0f, 0.72f), new Vector2(1f, 1f),
                new Vector2(16f, -4f), new Vector2(-16f, 0f));
            AddButton(pausePanel.transform as RectTransform, "RESUME MISSION",
                new Vector2(0.5f, 0.48f), new Vector2(-145f, -29f), new Vector2(290f, 58f),
                TogglePause, new Color(0.34f, 0.25f, 0.15f, 1f), 19);
            AddButton(pausePanel.transform as RectTransform, "SETTINGS",
                new Vector2(0.5f, 0.22f), new Vector2(-145f, -29f), new Vector2(290f, 58f),
                () =>
                {
                    pausePanel.SetActive(false);
                    settingsPanel.SetActive(true);
                }, new Color(0.16f, 0.20f, 0.17f, 1f), 18);
            pausePanel.SetActive(false);
        }

        private void TogglePause()
        {
            bool resume = Time.timeScale <= 0f;
            Time.timeScale = resume ? 1f : 0f;
            if (pausePanel != null) pausePanel.SetActive(!resume);
            ShowMessage(resume ? "MISSION RESUMED" : "MISSION PAUSED");
        }

        private GameObject AddButton(RectTransform parent, string label, Vector2 anchor, Vector2 offset,
            Vector2 size, UnityEngine.Events.UnityAction action, Color tint, int fontSize = 17)
        {
            GameObject go = CreateImage(label + " button", parent, tint, anchor, anchor, offset, offset + size);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(action);
            CreateText(label, go.transform, fontSize, Color.white, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return go;
        }

        private void AddHoldButton(RectTransform parent, string label, Vector2 anchor,
            Vector2 offset, Vector2 size, Color tint, System.Action<bool> callback, int fontSize)
        {
            GameObject go = CreateImage(label + " hold button", parent, tint, anchor, anchor, offset, offset + size);
            go.AddComponent<Button>().targetGraphic = go.GetComponent<Image>();
            go.AddComponent<HoldButton>().OnHoldChanged = callback;
            CreateText(label, go.transform, fontSize, Color.white, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private static GameObject CreateImage(string name, Transform parent, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return go;
        }

        private static Text CreateText(string value, Transform parent, int size, Color color,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject("Text - " + value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            Text text = go.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Sprite Circle()
        {
            if (circleSprite != null) return circleSprite;
            const int resolution = 128;
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            for (int x = 0; x < resolution; x++)
            {
                float dx = (x + 0.5f - resolution * 0.5f) / (resolution * 0.5f);
                float dy = (y + 0.5f - resolution * 0.5f) / (resolution * 0.5f);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((1f - distance) * 18f);
                pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            circleSprite = Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution),
                new Vector2(0.5f, 0.5f), resolution);
            return circleSprite;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            GameObject events = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(events);
        }

        public void SetMountButtonState(bool visible, bool mounted, string weaponName)
        {
            mountedWeaponMode = mounted;
            if (mountButton == null) return;
            mountButton.SetActive(Application.isMobilePlatform && visible);
            if (mountButtonLabel != null)
                mountButtonLabel.text = mounted ? "DISMOUNT" : "MOUNT";
            if (mounted && weaponText != null)
                weaponText.text = weaponName.ToUpperInvariant() + "  |  12.7 MM  |  CREW-SERVED";
        }

        public void SetMountedWeaponInfo(string weaponName, int beltAmmo, int beltReserve)
        {
            mountedWeaponMode = true;
            if (weaponText != null)
                weaponText.text = weaponName.ToUpperInvariant() + "  |  12.7 MM  |  BELT " +
                    beltAmmo.ToString("000") + " / 250  |  RES " + beltReserve.ToString("000");
        }

        public void SetStamina(float value)
        {
            float normalized = Mathf.Clamp01(value / 100f);
            if (staminaFill != null)
            {
                RectTransform fillRect = staminaFill.rectTransform;
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(normalized, 1f);
                staminaFill.color = Color.Lerp(new Color(0.70f, 0.28f, 0.18f),
                    new Color(0.55f, 0.72f, 0.42f), normalized);
            }
            if (staminaText != null)
                staminaText.text = "STAMINA  " + Mathf.RoundToInt(value).ToString("000") + "%";
        }

        public void SetWeaponInfo(int magazine, int reserve, string ammoMode)
        {
            if (weaponText != null && !mountedWeaponMode)
                weaponText.text = "AK-74M   |   5.45×39 mm   |   " + magazine.ToString("00") +
                    " / 30   |   RES " + reserve.ToString("000") + "   |   " + ammoMode;
        }

        public void ShowMessage(string value)
        {
            if (messageText == null) return;
            messageText.text = value;
            messageText.gameObject.SetActive(true);
            messageTimer = 2.4f;
        }

        private void Update()
        {
            if (messageTimer <= 0f) return;
            messageTimer -= Time.unscaledDeltaTime;
            if (messageTimer <= 0f && messageText != null)
                messageText.gameObject.SetActive(false);
        }
    }
}
