using MutationSwarm.Combat;
using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace MutationSwarm.UI
{
    /// <summary>
    /// Tienda de armas entre oleadas (UI Toolkit).
    /// </summary>
    public class Script_40_WeaponShopUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        private VisualElement _root;
        private Label _titleLabel;
        private Label _materialsLabel;
        private VisualElement _weaponList;
        private Button _continueBtn;

        private void Awake()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();

            CacheUi();
            Hide();
        }

        private void Update()
        {
            if (_root == null || _root.style.display == DisplayStyle.None)
                return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Script_39_WeaponShopManager.Instance?.CloseShopAndContinue();
        }

        public void Show(int afterWave)
        {
            if (_root == null)
                CacheUi();

            if (_root == null)
                return;

            _root.style.display = DisplayStyle.Flex;
            if (_titleLabel != null)
                _titleLabel.text = $"TIENDA DE ARMAS — Oleada {afterWave}";

            RefreshMaterials();
            RebuildWeaponList();
        }

        public void Hide()
        {
            if (_root != null)
                _root.style.display = DisplayStyle.None;
        }

        private void CacheUi()
        {
            if (_uiDocument == null)
                return;

            VisualElement doc = _uiDocument.rootVisualElement;
            _root = doc.Q<VisualElement>("WeaponShopRoot");
            _titleLabel = doc.Q<Label>("ShopTitle");
            _materialsLabel = doc.Q<Label>("ShopMaterials");
            _weaponList = doc.Q<VisualElement>("WeaponList");
            _continueBtn = doc.Q<Button>("BtnContinueShop");

            if (_continueBtn != null)
                _continueBtn.clicked += () => Script_39_WeaponShopManager.Instance?.CloseShopAndContinue();

            Script_03_EventBus.Subscribe<CoinChangedEvent>(_ => RefreshMaterials());
        }

        private void OnDestroy()
        {
            Script_03_EventBus.Unsubscribe<CoinChangedEvent>(_ => RefreshMaterials());
        }

        private void RefreshMaterials()
        {
            int coins = Script_42_CoinManager.Instance != null ? Script_42_CoinManager.Instance.Coins : 0;
            if (_materialsLabel != null)
                _materialsLabel.text = $"Monedas: {coins}";
        }

        private void RebuildWeaponList()
        {
            if (_weaponList == null || Script_39_WeaponShopManager.Instance == null)
                return;

            _weaponList.Clear();
            Script_38_PlayerLoadout loadout = FindFirstObjectByType<Script_38_PlayerLoadout>();

            foreach (SO_WeaponData weapon in Script_39_WeaponShopManager.Instance.Catalog)
            {
                if (weapon == null)
                    continue;

                VisualElement row = new();
                row.AddToClassList("shop-weapon-row");

                if (weapon.gunSprite != null)
                {
                    VisualElement icon = new();
                    icon.style.width = 64;
                    icon.style.height = 48;
                    icon.style.backgroundImage = new StyleBackground(weapon.gunSprite);
                    icon.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center);
                    icon.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center);
                    icon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                    row.Add(icon);
                }

                VisualElement textCol = new();
                textCol.AddToClassList("shop-weapon-text");
                Label name = new(weapon.displayName);
                name.AddToClassList("shop-weapon-name");
                Label desc = new(weapon.description);
                desc.AddToClassList("shop-weapon-desc");
                textCol.Add(name);
                textCol.Add(desc);
                row.Add(textCol);

                bool hasWeapon = loadout != null && loadout.OwnsWeapon(weapon);
                Button buy = new(() => OnBuyClicked(weapon));
                buy.text = hasWeapon ? "Equipar" : $"Comprar ({weapon.materialCost})";
                row.Add(buy);

                _weaponList.Add(row);
            }
        }

        private void OnBuyClicked(SO_WeaponData weapon)
        {
            Script_38_PlayerLoadout loadout = FindFirstObjectByType<Script_38_PlayerLoadout>();
            if (loadout == null)
                return;

            if (loadout.OwnsWeapon(weapon))
            {
                loadout.EquipWeapon(weapon);
            }
            else if (loadout.TryPurchaseWeapon(weapon))
            {
                Script_03_EventBus.Publish(new MutationToastEvent
                {
                    message = $"Compraste {weapon.displayName}",
                    color = new Color(0.3f, 0.9f, 0.5f)
                });
            }
            else
            {
                Script_03_EventBus.Publish(new MutationToastEvent
                {
                    message = "Monedas insuficientes",
                    color = new Color(1f, 0.4f, 0.35f)
                });
            }

            RefreshMaterials();
            RebuildWeaponList();
        }
    }
}
