This project was created as part of a technical examination. It features digital Blackjack and Poker (Jacks or Better) in one application with persistent accounting across both modes.
Drawing cards are determined by weighted RNG which can be manipulated at runtime using the "Options" page. The idea is that a developer can create conditional logic to better approach a target win rate.
For instance, one could reduce the dealer's chance of drawing a high face value card if the dealer is about to bust on it's next draw. However the end goal is to interface this system with an AI learning model
that will be guided by the gamestate information and a target winrate. It will accomplish this by modifying weight values at runtime.

[TO-DO]
- Create savestate and logging system for games.
- Add sounds and effects.
- Additional art and visuals.
- Optimize search and comparrison algorithms.
- Video and sound options.
- Implement hardcoded weight manipulation.
- Implement AI assisted weight manipulation.

[Assets Used]
- https://natomarcacini.itch.io/card-asset-pack
