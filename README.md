# Stumped for Rust
Trees leave stumps!

After the player chops down a tree, a proper stump is left behind, as it should be.
The player will need to wait some time before collecting that.

This plugin requires no permission and saves no data long-term.

## Config
```json
{
  "Options": {
    "protectedMinutes": 10
  },
  "Version": {
    "Major": 1,
    "Minor": 0,
    "Patch": 1
  }
}
```

The only config is above, protectedMinutes.  This sets the number of minutes after chopping down the tree before the chopPER can collect the stump.
