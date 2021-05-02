# Stumped for Rust
Trees leave stumps!

After the player chops down a tree, a proper stump is left behind, as it should be.
The player will need to wait some time before collecting that.

This plugin requires no permission and saves no data long-term.

## Config
```json
{
  "Options": {
    "protectedMinutes": 10,
    "stumpPercentChance": 100.0
  },
  "Version": {
    "Major": 1,
    "Minor": 0,
    "Patch": 1
  }
}
```

  - protectedMinutes --  This sets the number of minutes after chopping down the tree before the chopPER can collect the stump.
  - stumpPercentChance -- A number between 1 (0.5) and 100.  This determines the chance of a stump being generated.
    - Ignored if < 0.5 or equal to 100.
	- Normally, this should be an integer such as 1 or 60 and from 1 to 99 to have any affect.
