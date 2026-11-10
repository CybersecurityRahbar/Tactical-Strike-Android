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
        private GameObject settingsPanel;
        private GameObject pausePanel;
        private float messageTimer;
        private bool aim;

        private static Sprite circleSprite;

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
                    new Vector2(0.31f, 0.14f), new Vector2(1f, 0.91f), Vector2.zero, Vector2.zero);
                look.GetComponent<Image>().raycastTarget = true;
                look.AddComponent<TouchLookArea>().Initialize(player);
            }

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

        private void BuildStatusPanel(RectTransform root)
        {
            GameObject panel = CreateImage("Mission status", root, new Color(0.025f, 0.035f, 0.032f, 0.86f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -22f), new Vector2(550f, -148f));
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
            CreateText("WASD MOVE   |   MOUSE LOOK   |   CLICK CAPTURE   |   LMB FIRE   |   RMB AIM   |   SHIFT RUN / AIM FOCUS   |   C CROUCH   |   Z PRONE   |   SPACE JUMP   |   R RELOAD   |   T AMMO   |   ESC RELEASE",
                root, 15, new Color(0.94f, 0.91f, 0.82f, 0.88f), TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0f), new Vector2(0.92f, 0f),
                new Vector2(0f, 12f), new Vector2(0f, 48f));
        }

        private void BuildJoystick(RectTransform root)
        {
            GameObject baseObject = CreateImage("Movement joystick base", root, new Color(0.12f, 0.15f, 0.13f, 0.56f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 28f), new Vector2(228f, 228f));
            Image baseImage = baseObject.GetComponent<Image>();
            baseImage.sprite = Circle();
            baseImage.type = Image.Type.Simple;
            GameObject knobObject = CreateImage("Movement joystick thumb", baseObject.transform,
                new Color(0.73f, 0.65f, 0.48f, 0.7f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-42f, -42f), new Vector2(84f, 84f));
            knobObject.GetComponent<Image>().sprite = Circle();
            baseObject.AddComponent<TouchJoystick>().Initialize(player, knobObject.transform as RectTransform, 78f);
        }

        private void BuildCombatButtons(RectTransform root)
        {
            AddHoldButton(root, "FIRE", new Vector2(1f, 0f), new Vector2(-118f, 145f), new Vector2(126f, 126f),
                new Color(0.52f, 0.18f, 0.10f, 0.86f), held => player.Weapon.SetFireHeld(held), 24);
            AddButton(root, "AIM", new Vector2(1f, 0f), new Vector2(-286f, 224f), new Vector2(88f, 62f),
                () => { aim = !aim; player.SetAim(aim); }, new Color(0.20f, 0.25f, 0.21f, 0.86f));
            AddHoldButton(root, "RUN", new Vector2(1f, 0f), new Vector2(-278f, 140f), new Vector2(88f, 60f),
                new Color(0.20f, 0.25f, 0.21f, 0.86f), player.SetSprint, 17);
            AddHoldButton(root, "CROUCH", new Vector2(1f, 0f), new Vector2(-376f, 72f), new Vector2(90f, 58f),
                new Color(0.20f, 0.25f, 0.21f, 0.86f), player.SetCrouch, 15);
            AddButton(root, "JUMP", new Vector2(1f, 0f), new Vector2(-274f, 72f), new Vector2(88f, 58f),
                player.RequestJump, new Color(0.20f, 0.25f, 0.21f, 0.86f));
            AddButton(root, "PRONE", new Vector2(1f, 0f), new Vector2(-376f, 145f), new Vector2(90f, 58f),
                player.ToggleProne, new Color(0.20f, 0.25f, 0.21f, 0.86f), 15);
            AddButton(root, "RELOAD", new Vector2(1f, 0f), new Vector2(-478f, 72f), new Vector2(90f, 58f),
                player.Weapon.Reload, new Color(0.20f, 0.25f, 0.21f, 0.86f), 14);
            AddButton(root, "AMMO", new Vector2(1f, 0f), new Vector2(-578f, 72f), new Vector2(82f, 58f),
                player.Weapon.CycleAmmo, new Color(0.20f, 0.25f, 0.21f, 0.86f), 15);
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

        private void AddButton(RectTransform parent, string label, Vector2 anchor, Vector2 offset,
            Vector2 size, UnityEngine.Events.UnityAction action, Color tint, int fontSize = 17)
        {
            GameObject go = CreateImage(label + " button", parent, tint, anchor, anchor, offset, offset + size);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            button.onClick.AddListener(action);
            CreateText(label, go.transform, fontSize, Color.white, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
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
            if (weaponText != null)
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
