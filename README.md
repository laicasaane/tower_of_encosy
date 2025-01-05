# Tower of Encosy
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-1-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->

## Foreword

This is a playground for me to work on a 3D prototype. I aim to learn more about using ECS and 3D game.
While I have some knowledge with the former, I nearly have zero experience with the latter,
since I've been working mostly on 2D projects. Utility AI and 3D physics are also the topics
I want to explore within this project.

### A change of plan

The project was intended to be only a personal development sandbox. At some point I realized that
it won't be helpful for neither me nor the community. Knowing various tools and techiques is one thing,
applying that knowledge to a real game project requires much more reasoning skills.

So I decided to turn this project into an actual game. With great help from my friend, a game designer,
the project is going to be a turn-based RPG with a dedicated design document. Whatever techniques and tools
applied will be justified based on the document to give a real world example of how to use them.

While the project remains an OSS, anything related to the game mechanics won't be available
to public until the first playable version is published on some marketplaces.

### A change of licenses

- Most of the code inside `Assets/EncosyTower` folder is under the [MIT License][mit], unless stated otherwise.
- The code inside `Assets/Game` folder is under the [Apache License 2.0][apache].
- The game design document is under the [CC BY-NC-SA 4.0 License][cc].

[mit]: https://opensource.org/licenses/MIT
[apache]: https://www.apache.org/licenses/LICENSE-2.0
[cc]: https://creativecommons.org/licenses/by-nc-sa/4.0/

## Technical Notes

- **Unity Version**: 2022.3+
- **Rendering Pipeline**: URP
- **Enter Play Mode Options > Reload Domain**: Off
- `csc.rsp`: Enable C# 10 for the Unity Engine
- `Directory.Build.props`: Enable C# 10 for the IDE
- `IsExternalInit.cs`: Enable [`init`][init] of C# 9

[init]: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/init

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

Thank you for your contributions to the community. Your works greatly ease my development process.

<table>
  <tbody>
    <tr>
      <td>Annulus Games</td>
      <td><a href='https://github.com/AnnulusGames/UnityCodeGen'>Unity CodeGen</a></td>
    </tr>
    <tr>
      <td>baba-s</td>
      <td><a href='https://github.com/baba-s/Kogane.CheckBoxWindow'>Kogane Check Box Window</a></td>
    </tr>
    <tr>
      <td rowspan=2>Chris Yarbrough</td>
      <td><a href='https://github.com/chrisyarbrough/Renamer'>Renamer</a></td>
    </tr>
    <tr>
      <td><a href='https://github.com/chrisyarbrough/ScriptableObjectCreator'>ScriptableObjectCreator</a></td>
    </tr>
    <tr>
      <td>Community Toolkit</td>
      <td><a href='https://github.com/CommunityToolkit/dotnet'>.NET Community Toolkit</a></td>
    </tr>
    <tr>
      <td>CyberAgent Game & Entertainment</td>
      <td><a href='https://github.com/CyberAgentGameEntertainment/SmartAddresser'>Smart Addresser</a></td>
    </tr>
    <tr>
      <td>Cysharp, Inc.</td>
      <td><a href='https://github.com/Cysharp/UniTask'>UniTask</a></td>
    </tr>
    <tr>
      <td>Draconware</td>
      <td><a href='https://github.com/draconware-dev/SpanExtensions.Net'>SpanExtensions.Net</a></td>
    </tr>
    <tr>
      <td>Gil Reis</td>
      <td><a href='https://github.com/gilzoide/unity-easy-project-settings'>Unity Easy Project Settings</a></td>
    </tr>
    <tr>
      <td>hadashiA</td>
      <td><a href='https://github.com/hadashiA/UniTaskPubSub'>UniTaskPubSub</a></td>
    </tr>
    <tr>
      <td>Jiaqi Liu</td>
      <td><a href='https://github.com/0x7c13/UnityEditor-DarkMode'>DarkMode Mod for Unity Editor on Windows</a></td>
    </tr>
    <tr>
      <td rowspan=4>Kay Lousberg</td>
      <td><a href='https://kaylousberg.itch.io/kaykit-adventurers'>Character Pack: Adventurers</a></td>
    </tr>
    <tr>
      <td><a href='https://kaylousberg.itch.io/kaykit-skeletons'>Character Pack : Skeletons</a></td>
    </tr>
    <tr>
      <td><a href='https://kaylousberg.itch.io/kaykit-dungeon-remastered'>Dungeon Remastered Pack</a></td>
    </tr>
    <tr>
      <td><a href='https://kaylousberg.itch.io/halloween-bits'>Halloween Bits</a></td>
    </tr>
    <tr>
      <td>Kenney</td>
      <td><a href='https://www.kenney.nl/assets/ui-pack-adventure'>UI Pack - Adventure</a></td>
    </tr>
    <tr>
      <td rowspan=3>DreamingImLatios</td>
      <td><a href='https://github.com/Dreaming381/Latios-Framework'>Latios fFramework</a></td>
    <tr>
      <td><a href='https://github.com/Dreaming381/lsss-wip'>Latios Space Shooter Sample</a></td>
    </tr>
    <tr>
      <td><a href='https://github.com/Dreaming381/Latios-Framework-Documentation'>Latios Framework Documentation</a></td>
    </tr>
    <tr>
      <td>Maxwell Keonwoo Kang</td>
      <td><a href='https://github.com/cathei/BakingSheet'>BakingSheet</a></td>
    </tr>
    <tr>
      <td>Mika Notarnicola</td>
      <td><a href='https://github.com/thebeardphantom/Runtime-TypeCache'>Runtime TypeCache</a></td>
    </tr>
    <tr>
      <td>Miłosz Matkowski</td>
      <td><a href='https://github.com/arimger/Unity-Editor-Toolbox'>Unity Editor Toolbox</a></td>
    </tr>
    <tr>
      <td>Natsuneko Laboratory</td>
      <td><a href='https://github.com/natsuneko-laboratory/power-rename'>Power Rename</a></td>
    </tr>
    <tr>
      <td>Needle</td>
      <td><a href='https://github.com/needle-tools/needle-console'>Needle Console</a></td>
    </tr>
    <tr>
      <td>Peter @sHTiF Stefcek</td>
      <td><a href='https://github.com/pshtif/GenericMenuPopup'>GenericMenuPopup</a></td>
    </tr>
    <tr>
      <td>Philippe St-Amand</td>
      <td><a href='https://github.com/PhilSA/Trove'>Trove</a></td>
    </tr>
    <tr>
      <td>Roy Theunissen</td>
      <td><a href='https://github.com/RoyTheunissen/Asset-Palette'>Asset Palette</a></td>
    </tr>
    <tr>
      <td>Sebastiano Mandalà</td>
      <td><a href='https://github.com/sebas77/Svelto.Common'>Svelto.Common</a></td>
    </tr>
    <tr>
      <td>Stella Cannefax</td>
      <td><a href='https://github.com/stella3d/SharedArray'>SharedArray</a></td>
    </tr>
    <tr>
      <td>WMJordan</td>
      <td><a href='https://github.com/wmjordan/Codist'>Codist</a></td>
    </tr>
    <tr>
      <td>Yao Chunhui</td>
      <td><a href='https://github.com/redclock/SimpleEditorTableView'>SimpleEditorTableView</a></td>
    </tr>
  </tbody>
</table>

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