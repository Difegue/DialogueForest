![](DialogueForest/Assets/SmallTile.scale-200.png)
# DialogueForest  

DialogueForest is an [Outliner](https://en.wikipedia.org/wiki/Outliner) tool that is focused towards writing game dialogue:  
You can easily write multiple long text exchanges in a node, then link said node to other ones through VN-style prompts.  

<a href="https://apps.microsoft.com/detail/9P7MWMG1V6M6?cid=github-badge&mode=full">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

## Features  

- Write Dialogue Nodes with multiple text blocks and customizable per-block characters, and link them to each other through prompts  
- Store your nodes in Dialogue Trees, where they can be displayed either as cards or in tree form  
- Pin nodes to access them in one easy list  
- Automatically saves your work, even if the app is closed  
- Rich text support  
- Set predefined characters to easily use when writing  
- Set custom metadata for dialogue nodes, either strings, colors or booleans  
- Export your Trees to JSON, with rich text formatted using either HTML, Markdown or BBCode  
- Basic daily word objective functionality with notifications and streak counting  

## Screenshots

![](Assets/Screenshot_1.png)  

![](Assets/Screenshot_2.png)  

![](Assets/Screenshot_3.png)  

## Translation

You can easily contribute translations to DialogueForest! To help translate, follow these instructions.

### Adding a new language (requires Visual Studio 2019 or above)
- Create a new issue with the subject `[Translation] fr-CA` where you replace `fr-CA` with whatever language-region code you'll be translating into.
    - If an issue already exists, then don't do this step.
- Fork and clone this repo
- Open in Visual Studio
- In the `DialogueForest.Localization` project, find the `Strings` folder.
- Create a new file inside `Strings` that looks like this: `Resources.en-US.resx` but using the language you're translating into.
- Copy all the existing data from `Resources.en-US.resx` into your new `Resources.[language].resx`
- Translate the strings from english to your language
- Once done, then commit > push > create pull request!

### Improving an existing language (can be done with any text editor)
- Fork and clone this repo
- Open the `.resx` file (e.g. `Resources.en-US.resx`) you want to edit. Choose any text editor
- Translate
- Commit > push > create pull request!  

## Privacy Policy  

If Telemetry is enabled in the app's settings, the application will send detailed crash reports using [Sentry](https://sentry.io).  
Those reports can contain information about your hardware. (Motherboard type, etc)  

DialogueForest collects no other data from your device.  

## License

```
    DialogueForest
    Copyright (C) 2023 Difegue

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.  

    The DialogueForest icon is exempt from the License as per Section 7.e.
```