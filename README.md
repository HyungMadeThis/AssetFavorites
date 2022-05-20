<h1>Asset Favorites</h1>

A clean and intuitive favorites window to keep track of often used assets!

Very simple by design and has all familiar interactions with object treeviews fully supported.

- Interactions: Click to ping, double click to open assets.
- Folder Hierarchy: Create folders to organize your favorites!
- Drag and Drop: intuitively add/move favorited elements around, similar to the project browser!
- Search: The searchbar changes the treeview into a listview of search results.

With other nice features:
- Custom folder colors: Right click a favorites folder and change the color of the folder icon from the selection of colors!
- Reordering of elements: The elements in the treeview is not alphabetized like the project browser so you can organize them however you want.
- Element validation: Favorited assets are not lost when moved to a different path externally.
- Gracefully handle lost assets: If a favorited asset no longer exists (deleted while the editor if not open or changing branches), the element will display as missing. If the asset ever returns, it will also correctly fix itself in the favorites window. Otherwise, the user can simply choose to delete the missing element.
- Multiple instances of the window: The favorited assets are shared between all instances of the window, but the state of the treeviews are instanced, allowing for maximum flexibility.


TOFIX:
EDITORPREFS IS BAD! ITS SHARED BETWEEN ALL UNITY PROJECTS!!!!! MOVE IT ELSEWHERE! wait is that true tho