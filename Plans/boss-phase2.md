
### Phase 2: Overview

- One letter from the word **SHUTDOWN** glows at a time.
- Shoot cycle will occur every 5 seconds. There will be no glow animation for the target key, since it should already be glowing.
- Only one key is active at a time to simplify targeting.
- Shooting the correct key progresses the sequence.

- Incorrect shots will trigger retaliation of either:
	- Fire bullet fan pattern (as per Phase 1)
	- Spawn 10 fighters, in random but not overlapping x coordinates, positioned just off the top of the screen. Behaviour of fighters remains the same.
This is to be decided at random.


Given a keyboard layout, if the glowing character is:
  - "S" then the keys of "S", "Z", "X", or "Alt" do not trigger retaliation
  - "H" then the keys of "H", "B", "N" or "Space" do not trigger retaliation
  - "U" then the keys of "U", "H", "J", "N", "M" or "Space" do not trigger retaliation
  - "T" then the keys of "T", "F", "G", "V", "B" or "Space" do not trigger retaliation
  - "D" then the keys of "D", "X", "C" or "Space" do not trigger retaliation
  - "O" then the keys of "O", "K", "L", ",", ".", "Alt" or "Space" do not trigger retaliation
  - "W" then the keys of "W", "A", "S", "Z", "X", or "Alt" do not trigger retaliation
  - "N" then the keys of "N" or "Space" do not trigger retaliation

These keys are consider "Safe Zone" keys:
  - They do not trigger retaliation
  - Bullets pass through them
I think the easiest way is to deregister the safe zone keys from collision detection.
But SHUTDOWN keys themselves need collision detection still in place so the player can shoot them.

When the correct SHUTDOWN key is hit: 
 - Cancel the 5 second sequence & glow animation
 - Re-register the collision detection removed for the current keys safe zone
 - Move onto the next key
 - De-register collision detection for the new keys safe zone

On Phase 2 completion, all keys need to be re-registered.

On spelling of "SHUTDOWN" Phase 2 will end, and it will go into Phase 3 (do not implement yet).

At this stage there is no difficulty scaling. 
Nor does an incorrect shot cause the sequence to reset.
And obviously, the shoot timer has no bearing on sequence (i.e. it will never reset).
