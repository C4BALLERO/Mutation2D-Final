# Complete Player Prefab Structure & Configuration Guide

**Document Date:** June 9, 2026  
**Workspace:** `c:\Game\Mutation2D`  
**Reference Files:** 
- Source: `Assets/_Prefabs/Player/Prefab_Player.prefab`
- Template: `Assets/_Prefabs/Player/_Player.prefab` (needs completion)

---

## 1. PREFAB HIERARCHY

### Root GameObject: **Prefab_Player** (or **_Player**)
```
Prefab_Player (Layer: 6)
├── GroundCheck (Transform only)
├── FirePoint (Transform only)
└── Weapon_Primary (Transform + Script_20_WeaponBasic)
```

**Root Sprite:** Idle_Player.png (from art pack)  
**Root Tag:** `"Player"`  
**Root Layer:** `6` (Player layer)

---

## 2. ROOT COMPONENTS

### 2.1 Transform
```yaml
Position:        x: -0.4198, y: -0.9068, z: 0
LocalScale:      x: 0.6807, y: 0.6539, z: 1
Rotation:        x: 0, y: 0, z: 0 (default)
ConstrainProps:  False
```

### 2.2 SpriteRenderer
```yaml
Sprite:          Idle_Player (guid: 4e5923cf1c41c0a48b73cc04968a6b3e)
Color:           RGB(0.3, 0.8, 1) - Cyan accent
Material:        2100000/guid:a97c105638bdf8b4a8650670310a4cd3
SortingOrder:    0
FlipX/FlipY:     false (controlled by Script_11)
```

### 2.3 Rigidbody2D
```yaml
BodyType:           Dynamic (0)
Mass:               1
GravityScale:       3 ⭐ KEY VALUE
LinearDamping:      0
AngularDamping:     0.05
CollisionDetection: Continuous
Interpolate:        None
Constraints:        Z-rotation frozen (4)
Simulated:          true
```

### 2.4 BoxCollider2D (Body Hitbox)
```yaml
Size:        x: 0.6, y: 0.9
Offset:      x: 0, y: 0
IsTrigger:   false
EdgeRadius:  0
```

### 2.5 CapsuleCollider2D (Ground Check Trigger)
```yaml
Size:        x: 0.5, y: 0.2
Offset:      x: 0, y: -0.45
Direction:   Vertical (0)
IsTrigger:   true ⭐ IMPORTANT: Must be TRIGGER for ground detection
```

### 2.6 Animator
```yaml
Controller:      AC_Player.controller (guid: 2fbf30b15ac8a0e44bbfb3da4a7bfb35)
Avatar:          None
CullingMode:     Always Animate
ApplyRootMotion: false
AnimatePhysics:  false
```

### 2.7 Script_12_PlayerStats
```yaml
MaxHp:           100
MoveSpeed:       6
JumpForce:       12
DashForce:       18
DashCooldown:    1.2
HasDoubleJump:   false (upgradeable)
HasWallJump:     false (upgradeable)
```

### 2.8 Script_11_PlayerController
```yaml
PlayerIndex:              0 (0-3 for 4-player coop)
PrimaryWeapon:            Reference to Weapon_Primary child
SecondaryWeapon:          null (optional, upgradeable)

# References (auto-populated if children named correctly)
GroundTrigger:            null (auto-find CapsuleCollider2D)
GroundCheckPoint:         GroundCheck Transform
GroundMask:              1024 (Physics layer mask for ground)
WallMask:                1024 (Physics layer mask for walls)
DashVfx:                 null (optional ParticleSystem)

# Movement Settings
MoveSmoothing:           0.1 (velocity lerp factor)
CoyoteTime:              0.15 (frame allowance after leaving ground)
JumpBufferTime:          0.1 (input buffering for jumps)
JumpCutMultiplier:       0.5 (reduces jump height if released early)
GroundCheckRadius:       0.2 (overlap circle for ground detection)

# Dash Settings
DashDuration:            0.15 (seconds)

# Wall Jump Settings
WallJumpForce:           x: 10, y: 12 (impulse force)
WallSlideGravityMult:    0.3 (reduced gravity while on wall)
WallRayDistance:         0.65 (raycast distance from center)
WallJumpCooldown:        0.3 (seconds between wall jumps)
```

---

## 3. CHILD GAMEOBJECTS

### 3.1 GroundCheck (Child 0)
```yaml
Name:        GroundCheck
Layer:       0 (Default)
Position:    x: 0, y: -0.5, z: 0 (below player)
Scale:       x: 1, y: 1, z: 1
Components:  Transform only
Purpose:     Center point for ground detection radius check
```

**Used by:** Script_11_PlayerController._groundCheckPoint  
**Function:** Physics2D.OverlapCircle() with radius 0.2

### 3.2 FirePoint (Child 1)
```yaml
Name:        FirePoint
Layer:       0 (Default)
Position:    x: 0.4, y: 0.1, z: 0 (offset right/up from player)
Scale:       x: 1, y: 1, z: 1
Components:  Transform only
Purpose:     Projectile spawn location
```

**Used by:** Weapon_Primary._firePoint  
**Function:** Script_20_WeaponBasic.Fire() spawns projectiles here

### 3.3 Weapon_Primary (Child 2) ⭐ CRITICAL
```yaml
Name:        Weapon_Primary
Layer:       0 (Default)
Position:    x: 0, y: 0, z: 0 (aligned with player)
Scale:       x: 1, y: 1, z: 1

Components:
  - Transform
  - Script_20_WeaponBasic (MonoBehaviour)

Script_20_WeaponBasic Configuration:
  ProjectilePoolKey:   "Projectile_Basic" (must exist in SO_PoolConfig)
  FireRate:            0.2 (seconds between shots, 5 shots/sec)
  FirePoint:           Reference to FirePoint Transform (sibling)
  WeaponDisplayName:   "Rifle"
```

**Script GUID:** 2e8e9e96ebd87744c8fd2cf83ef8eeb4

---

## 4. ANIMATOR CONTROLLER STRUCTURE

**File:** `Assets/_Art/Animations/Player/AC_Player.controller`

### 4.1 State Machine States

| State | Animation Clip | Speed | Notes |
|-------|---|---|---|
| **Idle** | Player_Idle.anim | 1.0 | Default state |
| **Walk** | Player_Walk.anim | 1.0 | IsRunning parameter |
| **Fall** | Player_Fall.anim | 1.0 | Grounded=false |
| **Jump** | Player_Jump.anim | 1.0 | Jump trigger |
| **Dash** | Player_Dash.anim | 1.0 | Dash trigger |
| **Attack** | Player_Attack.anim | 1.0 | Attack trigger |
| **Hit** | Player_Hit.anim | 1.0 | Hit from damage |
| **Die** | Player_Die.anim | 1.0 | Die trigger |

### 4.2 Animator Parameters

#### Bool Parameters
- `IsRunning` — true when moving horizontally (velocity.x ≠ 0)
- `Grounded` — true when touching ground (OverlapCircle contact)
- `IsAttacking` — true during attack animation

#### Trigger Parameters
- `Jump` — Set when jump pressed/buffered
- `Dash` — Set when dash activated
- `Attack` — Set when weapon fires (managed by Script_11)
- `Hit` — Set when taking damage (called by ApplyDamage())
- `Die` — Set when HP ≤ 0

### 4.3 Transitions (Priority)
```
AnyState → Die (priority 0)        [condition: Die trigger]
AnyState → Hit (lower priority)    [condition: Hit trigger]

Walk → Idle                         [condition: IsRunning == false]
Walk → Jump                         [condition: Jump trigger]
Walk → Dash                         [condition: Dash trigger]
Walk → Attack                       [condition: Attack trigger]
Idle → Walk                         [condition: IsRunning == true]
Idle → Jump                         [condition: Jump trigger]
Idle → Dash                         [condition: Dash trigger]
Idle → Attack                       [condition: Attack trigger]
Fall → Idle                         [exit time: 0.85]
Jump → Fall                         [condition: Grounded == false]
Dash → Idle/Walk (depends on velocity)
Attack → Walk (exit time: varies)
Hit → Idle/Walk (exit time: varies)
```

---

## 5. ANIMATION CLIPS AVAILABLE

**Location:** `Assets/_Art/Animations/Player/`

| File | Duration | Frames | Purpose |
|------|----------|--------|---------|
| Player_Idle.anim | ~0.5s | 1-2 | Stationary pose |
| Player_Walk.anim | ~0.6s | 4 | Horizontal movement |
| Player_Jump.anim | ~0.3s | 2 | Jump startup |
| Player_Fall.anim | ~0.4s | 2 | Falling/descending |
| Player_Dash.anim | ~0.15s | 2-3 | Dash boost |
| Player_Attack.anim | ~0.3s | 3-4 | Weapon fire pose |
| Player_Hit.anim | ~0.2s | 2 | Damage knockback |
| Player_Die.anim | ~0.8s | 4-5 | Death animation |

**Source Sprites:** 
- Idle_Player.png, Walk_Player.png, Jump_Player.png, Dahs_Player.png (etc.)
- Spritesheet atlas with indexed frames

---

## 6. WHAT NEEDS TO BE COPIED TO _Player.prefab

### ✅ Currently Present in _Player.prefab
- ❌ Base components structure (yes, but incomplete)
- ❌ Missing: Animator controller assignment
- ❌ Missing: Weapon child + Script_20_WeaponBasic
- ❌ Missing: Proper layer masks
- ❌ Missing: FirePoint configuration

### 🔧 Required Fixes for _Player.prefab

| Component | Issue | Fix |
|-----------|-------|-----|
| Animator | Controller = null | Assign: AC_Player.controller (guid: 2fbf30b15ac8a0e44bbfb3da4a7bfb35) |
| Script_11_PlayerController | _primaryWeapon = null | Create/link Weapon_Primary child with Script_20_WeaponBasic |
| Script_11_PlayerController | _groundCheckPoint = null | Link to GroundCheck child Transform |
| Script_11_PlayerController | _groundMask = 0 | Set to 1024 (layer mask for ground) |
| Script_11_PlayerController | _wallMask = 0 | Set to 1024 (layer mask for walls) |
| Rigidbody2D | GravityScale = 1 | Change to 3 ⭐ CRITICAL |
| CapsuleCollider2D | IsTrigger = false | Change to true ⭐ CRITICAL |
| CapsuleCollider2D | Offset = 0,0 | Change to x: 0, y: -0.45 |
| CapsuleCollider2D | Size = 1x2 | Change to x: 0.5, y: 0.2 |
| SpriteRenderer | Sprite = Idle | Assign correct sprite from animation |
| Script_12_PlayerStats | Appears 2x (duplicate) | Remove duplicate component |

---

## 7. SPAWN/INSTANTIATION REQUIREMENTS

### For Scene Instantiation
```csharp
// Recommended instantiation method
Player player = Instantiate(Prefab_Player, spawnPosition, Quaternion.identity);
Script_11_PlayerController controller = player.GetComponent<Script_11_PlayerController>();
controller.SetPrimaryWeapon(equippedWeapon);  // If needed
```

### Layer Setup Required (Layers Panel)
```
Layer 6:  "Player"
Layer 10: "Ground" (LayerMask 1024 = layer 10)
Layer 10: "Wall"   (same layer as ground for raycast)
```

### Tag Setup Required (Tags Panel)
```
Tag: "Player"
```

---

## 8. PHYSICS SETTINGS REFERENCE

### Expected Physics Values
```yaml
Gravity Scale:           3.0
Base Jump Force:         12 (units/s impulse)
Max Fall Speed:          ~6 m/s (wall slide: 1.8 m/s)
Dash Force:              18 (units/s impulse)
Move Speed:              6 units/s (not physics, applied to Rigidbody.linearVelocity)
Collision Detection:     Continuous (prevents clipping through thin walls)
```

### Ground Detection Radius
```
Radius: 0.2 units
Origin: _groundCheckPoint position (0, -0.5 relative to player)
LayerMask: 1024 (layer 10)
```

---

## 9. WEAPON SYSTEM INTEGRATION

### Weapon Hierarchy
```
Weapon_Primary (GameObject)
├── Script_20_WeaponBasic
├── FirePoint (child of Weapon_Primary or sibling)
└── Projectile Pool reference: "Projectile_Basic"
```

### Fire System Flow
1. **Input:** Script_11.HandleCombatInput() detects input
2. **Fire:** _primaryWeapon.Fire(fireDirection) called
3. **Spawn:** Projectile spawned from objectPool at FirePoint position
4. **Animation:** Animator.SetTrigger("Attack")
5. **Rate:** Fire rate = 0.2s (5 shots/sec max)

---

## 10. SCRIPT DEPENDENCIES & GUIDs

| Script | GUID | Location | Required |
|--------|------|----------|----------|
| Script_11_PlayerController | 8b91c1e6790fae147ace2c90d3ef9039 | Assets/_Scripts/Entities/ | ✅ YES |
| Script_12_PlayerStats | 456ed2750a7ae8d4caf2bb856f691c48 | Assets/_Scripts/Entities/ | ✅ YES |
| Script_20_WeaponBasic | 2e8e9e96ebd87744c8fd2cf83ef8eeb4 | Assets/_Scripts/Combat/ | ✅ YES |
| AC_Player.controller | 2fbf30b15ac8a0e44bbfb3da4a7bfb35 | Assets/_Art/Animations/Player/ | ✅ YES |

---

## 11. QUICK CHECKLIST FOR _Player.prefab COMPLETION

```
[ ] Animator Controller assigned (AC_Player.controller)
[ ] Rigidbody2D GravityScale = 3
[ ] CapsuleCollider2D IsTrigger = true
[ ] CapsuleCollider2D Offset = (0, -0.45)
[ ] CapsuleCollider2D Size = (0.5, 0.2)
[ ] Script_11 _groundCheckPoint linked
[ ] Script_11 _groundMask = 1024
[ ] Script_11 _wallMask = 1024
[ ] Weapon_Primary child created with Script_20_WeaponBasic
[ ] FirePoint child created at (0.4, 0.1, 0)
[ ] Script_20_WeaponBasic _firePoint linked
[ ] Script_12_PlayerStats duplicate removed
[ ] Layer set to 6 ("Player")
[ ] Tag set to "Player"
[ ] Ground check radius visual verified in Scene
```

---

## 12. COMMON ISSUES & SOLUTIONS

| Issue | Cause | Solution |
|-------|-------|----------|
| Player falls through floor | CapsuleCollider IsTrigger = true incorrectly OR _groundMask = 0 | Set to trigger for ground CHECK only; BoxCollider2D handles collision |
| Jump doesn't work | _groundCheckPoint null or wrong offset | Set GroundCheck child at (0, -0.5) |
| Weapon doesn't spawn projectiles | _firePoint null or pool key mismatch | Link FirePoint; verify "Projectile_Basic" pool exists |
| Animation doesn't play | AC_Player controller not assigned | Verify guid: 2fbf30b15ac8a0e44bbfb3da4a7bfb35 |
| Player moves too slow/fast | MoveSpeed or GravityScale wrong values | MoveSpeed=6, GravityScale=3 |
| Movement feels floaty | GravityScale too low | Ensure = 3.0, not 1.0 |
| Ground detection fails mid-jump | GroundCheck radius collision layer wrong | _groundMask must be layer 10 (1024) |

---

## 13. ARCHIVAL REFERENCE

**This guide compiled from:**
- Prefab_Player.prefab (source reference)
- Script_11_PlayerController.cs (full implementation)
- Script_12_PlayerStats.cs (stat system)
- AC_Player.controller (animator states)
- Animation clips directory listing

**Last verified:** June 9, 2026  
**Project:** Mutation Swarm 2D (Unity 6000.3.x, 2D URP)

