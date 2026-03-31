# 🌊 Eternal Flow

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Raylib](https://img.shields.io/badge/raylib-cs-000000?style=flat&logo=c&logoColor=white)](https://github.com/ChrisDill/Raylib-cs)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Eternal Flow** is a generative audiovisual zen-arcade built with pure C# and Raylib.

There are no enemies, no shooting, and no traditional "Game Over" screens. This is a game about the state of flow, deep concentration, and the ability to maintain balance under pressure.

![Gameplay Screenshot 1](/assets/screen1.png)
_Perfect Flow: silver dust, glowing aura, and a rapidly growing multiplier._

---

## 🧘 Game Philosophy

Most games punish players by abruptly stopping the gameplay. In **Eternal Flow**, the punishment is the _degradation of the sensory experience_.

When the player perfectly glides along the generated path, the game rewards them: the color palette brightens, the player's orb emits a soft aura, silver dust generates in the wake, and the score multiplier rises.

However, as focus is lost and stress levels increase:

- The world fades and sinks into darkness.
- The golden path burns to a crisp black and begins to nervously jitter.
- The background music speeds up, and under critical stress—**plunges underwater** (muffled by a low-pass frequency filter).
- Accumulated score points physically start to burn away.

To survive, you simply need to return to the center of the flow and calm the game down.

![Gameplay Screenshot 2](/assets/screen2.png)
_Critical Stress: visual glitching, darkened background, and muffled audio._

---

## 🎧 Non-Commercial & Audio Notice

**Eternal Flow is a completely free, non-commercial project created for meditation, learning, and open-source contribution.**

Please note that any `.mp3` music tracks included in the `sound` folder of compiled builds or forks may be subject to copyright by their respective artists. **This game is not intended for monetization.** If you fork this project or upload your own builds, ensure you have the rights to the music you use, or stick to royalty-free/DMCA-safe tracks if you plan to stream or share it publicly.

---

## 🛠 Technical Features (Under the Hood)

This project is a great example of creating a deep, immersive experience using minimal external tools. For those studying game development, here are a few interesting architectural solutions implemented in the core:

- **The "Printer Effect" Math (`PathGenerator.cs`)**
  The landscape does not deform as it moves. The path generation is tied to a global world coordinate (`globalX`), layered with macro and micro waves (Sine/Cosine). This makes the terrain slide toward the player as a solid, predictable monolith, allowing for fair gameplay.

- **Custom DSP Audio Filter (`AudioManager.cs`)**
  The "underwater" effect is built from scratch without external plugins. The game hooks directly into Raylib's audio stream via `AttachAudioStreamProcessor` and `unsafe` pointers. Using a mathematical Low-Pass Filter, the engine physically cuts high frequencies from the audio buffer in real-time when player stress peaks.

- **Audio-Reactive Background (`FloatingShapes.cs`)**
  The background geometry doesn't just float—it listens. The audio engine analyzes the sound amplitude every frame to detect beats (volume spikes). With every heavy bass hit, specific shapes react by flashing and visually expanding.

- **Perceptual OKLCH Palette (`ColorConverter.cs`)**
  All smooth color transitions are calculated within the OKLCH color space rather than standard RGB or HSL. This ensures the game never generates "muddy" or aggressively "neon" colors during mathematical hue morphing.

---

## 🎮 How to Play

The controls are incredibly simple but require a feel for inertia. Your orb moves as if gliding underwater or in zero gravity.

- **[W] / [↑]** — Float Up
- **[S] / [↓]** — Sink Down
- **[SPACE]** — Pause
- **[ESC]** — Main Menu / Break the Flow

> **Tip:** Do not mash the keys. Press and hold smoothly, anticipating the curves and allowing the orb to glide on its momentum.

---

## 💻 For Developers: Build & Run

If you want to fork the project, study the code, or add your own mechanics—welcome! The project has a very clean structure and does not require heavy game engines (like Unity or Unreal).

### Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed.
- Any code editor (VS Code, Visual Studio, Rider).

### Running Locally

Clone the repository and navigate to the project folder:

    git clone https://github.com/MrHryhorii/EternalFlow.git
    cd EternalFlow

Run the game in developer mode (with the debug console):

    dotnet run

### Important: Assets (Fonts & Sounds)

For the game to run properly, the root build directory must contain:

- A `fonts` folder with a `DejaVuSans.ttf` file (or swap it out in `Program.cs`).
- A `sound` folder with at least one `.mp3` track. The game will automatically read all files in this folder, generate a dynamic playlist, and seamlessly crossfade between them.

### Creating a Standalone Release

To compile the game for players who do not have .NET installed, use the following commands:

**For Windows (64-bit):**

    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

**For Linux (64-bit):**

    dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

The compiled files will appear in `bin/Release/net8.0/{platform}/publish/`. Simply zip the contents of that `publish` folder, and your game is ready to be shared!

---

## 📄 License

This project is open-sourced under the **MIT License**. You are free to use, modify, and distribute this code. If it helps you learn or inspires you to build your own zen game, that is the best reward.

Stay in the flow. 🌊
