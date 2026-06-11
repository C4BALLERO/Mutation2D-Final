using System.Collections.Generic;
using MutationSwarm.Core;
using MutationSwarm.Entities;
using UnityEngine;

namespace MutationSwarm.Combat
{
    /// <summary>
    /// Armas compradas y equipadas del jugador.
    /// </summary>
    public class Script_38_PlayerLoadout : MonoBehaviour
    {
        [SerializeField] private SO_WeaponData _startingWeapon;
        [SerializeField] private Transform _weaponAttachPoint;

        private readonly HashSet<string> _ownedIds = new();
        private Script_20_WeaponGun _equippedGun;
        private Script_11_PlayerController _controller;

        public SO_WeaponData EquippedWeapon => _equippedGun != null ? _equippedGun.Data : null;

        private void Awake()
        {
            _controller = GetComponent<Script_11_PlayerController>();
            if (_weaponAttachPoint == null)
            {
                Transform fp = transform.Find("FirePoint");
                _weaponAttachPoint = fp != null ? fp.parent : transform;
            }

            if (_startingWeapon != null)
            {
                UnlockWeapon(_startingWeapon);
                EquipWeapon(_startingWeapon);
            }
        }

        public bool OwnsWeapon(SO_WeaponData weapon)
        {
            return weapon != null && _ownedIds.Contains(weapon.weaponId);
        }

        public void UnlockWeapon(SO_WeaponData weapon)
        {
            if (weapon == null || string.IsNullOrEmpty(weapon.weaponId))
                return;
            _ownedIds.Add(weapon.weaponId);
        }

        public bool TryPurchaseWeapon(SO_WeaponData weapon)
        {
            if (weapon == null || OwnsWeapon(weapon))
                return false;

            if (MutationSwarm.Building.Script_23_BuildManager.Instance == null)
                return false;

            if (MutationSwarm.Building.Script_23_BuildManager.Instance.BuildMaterials < weapon.materialCost)
                return false;

            if (!MutationSwarm.Building.Script_23_BuildManager.Instance.TrySpendMaterials(weapon.materialCost))
                return false;

            UnlockWeapon(weapon);
            EquipWeapon(weapon);
            Script_03_EventBus.Publish(new WeaponPurchasedEvent { weapon = weapon });
            return true;
        }

        public void EquipWeapon(SO_WeaponData weapon)
        {
            if (weapon == null || !OwnsWeapon(weapon))
                return;

            if (_equippedGun != null)
                Destroy(_equippedGun.gameObject);

            GameObject go = new($"Weapon_{weapon.weaponId}");
            go.transform.SetParent(_weaponAttachPoint != null ? _weaponAttachPoint : transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 11;

            _equippedGun = go.AddComponent<Script_20_WeaponGun>();
            _equippedGun.Initialize(weapon);

            _controller?.SetPrimaryWeapon(_equippedGun);
            Script_03_EventBus.Publish(new WeaponEquippedEvent { weapon = weapon });
        }
    }
}
