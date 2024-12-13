# Tower of Encosy
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-1-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

## Foreword

This is a playground for me to work on a 3D prototype. I aim to learn more about using ECS and 3D game.
While I have some knowledge with the former, I nearly have zero experience with the latter,
since I've been working mostly on 2D projects. Utility AI and 3D physics are also the topics
I want to explore within this project.

## Techincal Notes

- **Unity Version**: 2022.3+
- **Rendering Pipeline**: URP
- **Enter Play Mode Options > Reload Domain**: Off
- `csc.rsp`: Enable C# 10 for the Unity Engine
- `Directory.Build.props`: Enable C# 10 for the IDE
- `IsExternalInit.cs`: Enable [`init`][1] of C# 9

[1]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/init

## Gameplay

I choose to work on a classic RPG mechanics where the player controls a character to subjugate monsters in a dungeon.

More features will come later after the very first goals are completed.

## First Goals

1. Characters and environment setup with Entities Graphics + Latios Kinemation
2. 3D physics setup with Latios Psyshock
3. Character animation setup with Latios Kinemation
4. Basic stats for the characters
5. Character movement systems
6. Character animating systems
7. Basic Utility AI for the monsters
8. Character attack systems

## Credits

### Latios Framework

- The framework https://github.com/Dreaming381/Latios-Framework
- The massive demo https://github.com/Dreaming381/lsss-wip
- The documentation https://github.com/Dreaming381/Latios-Framework-Documentation

### Assets by KayKit

- Character Pack : Adventurers https://kaylousberg.itch.io/kaykit-adventurers
- Character Pack : Skeletons https://kaylousberg.itch.io/kaykit-skeletons
- Halloween Bits https://kaylousberg.itch.io/halloween-bits
- Dungeon Remastered Pack https://kaylousberg.itch.io/kaykit-dungeon-remastered

### Assets by Kenney

- UI Pack - Adventure https://www.kenney.nl/assets/ui-pack-adventure

## Contributors ✨

Thanks goes to these wonderful people ([emoji key](https://allcontributors.org/docs/en/emoji-key)):

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/gostan99"><img src="https://avatars.githubusercontent.com/u/61959499?v=4?s=100" width="100px;" alt="Dani Kuan"/><br /><sub><b>Dani Kuan</b></sub></a><br /><a href="https://github.com/laicasaane/tower_of_encosy/commits?author=gostan99" title="Code">💻</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->

This project follows the [all-contributors](https://github.com/all-contributors/all-contributors) specification. Contributions of any kind welcome!