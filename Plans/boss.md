# 🔓 UNLOCK Protocol – End-of-Level Boss Mechanic

## Game Overview

A top-down, vertical-scrolling 8-bit style shooter inspired by *Xenon* (Atari ST/Amiga). The player is on a mission to rescue a keyboard from an AI-controlled fortress. All enemies are mechanical, and the boss fights are bullet-hell style.

---

## Boss: UNLOCK Protocol

### Theme

A corrupted AI defense system shaped like a fragmented keyboard. The boss guards the final gate to the next level.

---

## Boss Phases

### Phase 1: Chaos Keys

- Random keys on the boss light up and fire bullet patterns.

- The player must dodge incoming fire while navigating the screen.



---

### Phase 2: Spell “UNLOCK”

- One letter from the word **UNLOCK** glows at a time.

- Only one key is active at a time to simplify targeting.

- Shooting the correct key progresses the sequence.

- Optional: Incorrect shots can be ignored or trigger retaliation.

---

### Final Phase: Escape & Enter

- After spelling “SHUTDOWN”, the boss begins to flee upward.

- It drops mines in a zigzag pattern.

- A large glowing **Enter** key appears at the top of the screen.

- The player must dodge mines and shoot the **Enter** key to finish the boss.

- Successful hit triggers a retro-style explosion and a message: `ACCESS GRANTED`.

---

## Design Notes

- Keep visuals simple and readable in 8-bit style.

- The “Enter” key should feel like a final, rewarding moment.

---

## Development Tips

- Consider some keys like "U" are on the keyboard. J,N and M wouldn't trigger retaliation.

- Consider using a state machine to manage boss phases.

- Keep bullet patterns readable but challenging.