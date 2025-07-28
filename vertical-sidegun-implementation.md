# Vertical SideGun Implementation Instructions

## Overview
This document outlines the steps needed to allow SideGun objects to be oriented vertically (up/down) in addition to the current horizontal orientations (left/right).

## Current State Analysis
- SideGun currently supports `Left` and `Right` orientations only
- Horizontal flipping is handled via `SpriteEffects.FlipHorizontally`
- Gun positioning uses X-axis offsets with animated movement
- Projectiles fire horizontally with velocity vectors (-5, 0) or (5, 0)
- Tiled map editor uses GID flipping bit to determine left/right orientation
- Sprite frames: Frame[0] = horizontal base mount (right), Frame[1] = horizontal gun barrel (right)
- New sprite frames added: Frame[2] = vertical base mount (up), Frame[3] = vertical gun barrel (up)

## Implementation Steps

### 1. Update SideGunOrientation Enum ✅ COMPLETED
**File:** `GameObjects\Enemies\SideGun\SideGunEnums.cs`

Add new orientation values:
```csharp
public enum SideGunOrientation
{
    Left,
    Right,
    Up,
    Down
}
```

### 2. Modify SideGun Position and Animation Logic ✅ COMPLETED
**File:** `GameObjects\Enemies\SideGun\SideGun.cs`

#### Constants to Update:
- Add vertical equivalents of `GunXOffset`, `TopGunYOffset`, `BottomGunYOffset`
- Consider renaming to be orientation-agnostic (e.g., `GunPrimaryOffset`, `Gun1Offset`, `Gun2Offset`)

#### Animation Logic Changes:
- Update `Update()` method to handle both horizontal and vertical gun movement
- For vertical orientations, animate guns along Y-axis instead of X-axis
- Maintain the same sin wave animation pattern but apply to appropriate axis

#### Position Calculation Changes:
- Modify `DrawActive()` method to calculate gun positions based on orientation
- For `Up`/`Down`: use Y-axis offsets and animate along X-axis
- For `Left`/`Right`: keep existing X-axis offsets and animate along Y-axis (current behavior)

### 3. Update Drawing Logic ✅ COMPLETED
**File:** `GameObjects\Enemies\SideGun\SideGun.cs`

#### Sprite Frame Selection:
- Use Frame[2] for vertical base mount (Up orientation)
- Use Frame[3] for vertical gun barrel (Up orientation)
- Use `SpriteEffects.FlipVertically` to create Down orientation from Up frames
- Keep existing Frame[0] and Frame[1] for horizontal orientations

#### Sprite Effects:
- `Left`: Use `SpriteEffects.FlipHorizontally` (existing behavior)
- `Right`: Use `SpriteEffects.None` (existing behavior)
- `Up`: Use `SpriteEffects.None` with Frame[2] and Frame[3]
- `Down`: Use `SpriteEffects.FlipVertically` with Frame[2] and Frame[3]

#### Drawing Method Updates:
- Update `DrawActive()` to select appropriate frames based on orientation
- Update `DrawDestroyed()` to use correct base frame (Frame[0] for horizontal, Frame[2] for vertical)
- Ensure proper sprite positioning for each orientation

### 4. Modify Shooting Logic ✅ COMPLETED
**File:** `GameObjects\Enemies\SideGun\SideGun.cs`

#### Fire Position Calculation:
- Update `Shoot()` method to calculate fire positions based on orientation
- For vertical orientations, adjust Y positions instead of X positions

#### Fire Vector Updates:
```csharp
Vector2 fireVector = Orientation switch
{
    SideGunOrientation.Left => new Vector2(-ShootVelocity, 0),
    SideGunOrientation.Right => new Vector2(ShootVelocity, 0),
    SideGunOrientation.Up => new Vector2(0, -ShootVelocity),
    SideGunOrientation.Down => new Vector2(0, ShootVelocity),
    _ => new Vector2(-ShootVelocity, 0)
};
```

### 5. Update Tiled Map Reading Logic
**File:** `Core\Tiles\TiledReader.cs`

#### ReadSideGuns Method:
- Current logic uses GID flipping bit for left/right determination
- Need to extend this to support vertical orientations
- Consider using Tiled object properties or different tile GIDs for vertical orientations
- May need to update the Tiled map file structure

#### Possible Approaches:
1. Use additional GID bits or different tile IDs for vertical orientations
2. Add custom properties to SideGun objects in Tiled for orientation
3. Use object rotation property in Tiled (if available)

### 6. Update Map Configuration
**File:** `Content\map.tmx`

- Add vertical SideGun instances to test the implementation
- Ensure proper GID or property configuration for vertical orientations
- May need to update tileset files if using different sprites

### 7. Constants and Magic Numbers Review
- Review all hardcoded offsets and ensure they work for both horizontal and vertical orientations
- Consider making offsets configurable or calculated based on sprite dimensions
- Update comments to reflect new orientation support

## Testing Checklist

### Visual Testing:
- [ ] Vertical SideGuns render correctly in both Up and Down orientations
- [ ] Gun animations work smoothly for vertical orientations
- [ ] Sprite effects (flipping/rotation) display properly
- [ ] Destroyed state renders correctly for all orientations

### Functional Testing:
- [ ] Projectiles fire in correct directions for each orientation
- [ ] Animation timing matches horizontal orientations
- [ ] Collision detection works properly
- [ ] Sleep/wake functionality operates correctly
- [ ] Gun positioning updates properly during animation

### Integration Testing:
- [ ] Tiled map loads vertical SideGuns correctly
- [ ] No conflicts with existing horizontal SideGuns
- [ ] Performance impact is minimal
- [ ] Save/load functionality (if applicable) works

## Implementation Notes

### Design Considerations:
- Maintain consistency with existing animation patterns
- Ensure code remains readable and maintainable
- Consider future extensibility (diagonal orientations, etc.)
- Keep performance impact minimal

### Potential Challenges:
- Animation offsets need careful calculation to avoid visual glitches
- Tiled map integration may require custom properties or tileset changes
- Sprite frame selection logic needs to handle all four orientations correctly

### Code Quality:
- Use switch expressions where appropriate for cleaner code
- Add comprehensive comments for new orientation logic
- Ensure consistent naming conventions
- Consider extracting common logic into helper methods

## Files to Modify Summary
1. `GameObjects\Enemies\SideGun\SideGunEnums.cs` - Add new orientations
2. `GameObjects\Enemies\SideGun\SideGun.cs` - Main implementation changes
3. `Core\Tiles\TiledReader.cs` - Map reading logic updates
4. `Content\map.tmx` - Test configuration (optional)
5. Potentially tileset files for vertical sprites

## Success Criteria
- SideGuns can be placed and function correctly in all four orientations (Left, Right, Up, Down)
- Visual and functional parity between horizontal and vertical orientations
- Existing horizontal SideGuns continue to work without changes
- Clean, maintainable code that follows project patterns
