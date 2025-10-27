# Flappy Pong - iOS / Android mobile game prototype built in Unity

## Team

- Rhett Pilcher: Programming & Design
- Alyssa Pilcher: Art

## Summary

Flappy Pong is a mobile game prototype I made with the goal of familiarizing myself with mobile game development, specifically in Unity. I wanted to learn exactly how to implement ad serving and IAP. I went above and beyond by designing many bonus mechanics and upgrades including:
- power ups
- bonus gameplay boosters like cannons
- cosmetic skins, trails and background themes
- equipable upgrade charms
- missions system
- coins and gems
- a fully implemented shop system
- mock monetization with IAP and Interstitial + Rewarded ads

The games functionality is more or less at an alpha phase with all design and functionallity complete. I may continue and ship this project in the future.

## Tools & Technologies

- **Unity Game Engine** – Development engine
- **Unity LevelPlay / ironSource** – Unity and other ad serving
- **Google AdMob** – Google ad serving

## Challenges & Solutions

Ads and IAP: I struggled to implement ads in particular due to the somewhat conviluted process of setting up add instances across multiple serving platforms and then correctly initializing, loading, and running them in the game with the correct IDs. The IDs are not included in the code and the ads do not serve in this prototype because I've stopped paying for the service, but the code to execute them is there. The solution was just a matter of learning the required platforms and integration methods, which was a confusing process, but by the end I was confident I knew what I was doing.

Missions System: Implementing the missions system was surprisingly difficult due to the constraints I had set for myself. I wanted a random mission per-game with several possible categories:
- Reward: Coins, gems, or shop item
- Mission Type: 
        Distance - Reach this distance before dying 
        | Coins - Collect or evade this many coins, timed (before this time) or distanced (before reaching this distance) 
        | Walls - Break a certain type of wall a specific amount of times, timed or distanced 
        | Combos: Perform an amount or length, timed or distanced 
        | PowerUps: Evade or collect this many, timed or distanced 
        | Revive: Revive this many times, timed or distanced 
        | Hazards: Hit this amount while invincible, timed or distanced 
- Difficulty: Easy, medium, hard; with each difficulty having a higher reward but more conditions needed to be met

I ended up creating a class for Missions that could handle all of the condition checking and randomization to be used as an object for each mission instance.

Optimization: While testing on my iOS device, I noticed lag spikes. To counteract this I utilized optimization techniques like object pooling, pre-loading breakable walls, minimizing usage of Instantiate() and Destroy() methods, and using the Unity Profiler.
