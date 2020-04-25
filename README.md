# Internal Icon Browser

Internal Icon Browser is a simple Editor Window that allows you to browse internal icons that are currently loaded.

This tool is inspired by [Unity Internal Icons][1] (thanks to [@p-groarke][2]). It uses the same technique for finding internal icons but with the window layout rebuilt from scratch to improove usability.

## Installation

This tool uses the new UPM system. The old copy-into-Assets method still works
perfectly decent so if you don't want to bother with UPM just copy the files you want
directly into your project.

To add this project, add a [git dependency][3] in your `manifest.json`:

```json
    "studio.gilp.internal-icon-browser": "https://github.com/GilpStudio/unity-internal-icon-browser.git",
```

A "Tools/Internal Icon Browser" menu item should show up in the toolbar menu. Once the tool opens you can
filter entries using the search field at the top. You can also double click any icon name to copy it to the clipboard.

Keep in mind that the tool will only find icons that are curently loaded. It should keep adding icons to the list while it is open.

[1]: https://assetstore.unity.com/packages/tools/utilities/unity-internal-icons-70496
[2]: https://github.com/p-groarke
[3]: https://docs.unity3d.com/Manual/upm-git.html
## Package structure

```none
<root>
  ├── package.json
  ├── README.md
  ├── CHANGELOG.md
  ├── Editor
  │   ├── GilpStudio.InternalIconBrowser.Editor.asmdef
  │   └── InternalIconBrowserWindow.cs
```
