# Core: a collection of useful functionality for Unity, which is used by my other packages.

### Installation:

#### Method 1 - Package Manager

Using the Package Manager is the easiest way to install HyperNav to your project. Simply install the project as a git URL. Note that if you go this route, you will not be able to make any edits to the package.

1. In Unity, open the Package Manager (Window > Package Manager).
2. Click the '+' button in the top right of the window.
3. Click "Add package from git URL...".
4. Paste in `https://github.com/vcmiller/Infohazard.Core.git`.
5. Click Add.

#### Method 2 - Git Submodule

Using a git submodule is an option if you are using git for your project source control. This method will enable you to make changes to the package, but those changes will need to be tracked in a separate git repository.

1. Close the Unity Editor.
2. Using your preferred git client or the command line, add `https://github.com/vcmiller/Infohazard.Core.git` as a submodule in your project's Packages folder.
3. Re-open the Unity Editor.

If you wish to make changes when you use this method, you'll need to fork the HyperNav repo. Once you've made your changes, you can submit a pull request to get those changes merged back to this repository if you wish.

1. Fork this repository. Open your newly created fork, and copy the git URL.
2. In your project's Packages folder, open the HyperNav repository.
3. Change the `origin` remote to the copied URL.
4. Make your changes, commit, and push.
5. (Optional) Open your fork again, and create a pull request.

#### Method 3 - Add To Assets

If you wish to make changes to the library without dealing with a git submodule (or you aren't using git), you can simply copy the files into your project's Assets folder.

1. In the main page for this repo, click on Code > Download Zip.
2. Extract the zip on your computer.
3. Make a HyperNav folder under your project's Assets folder.
4. Copy the `Editor` and `Runtime` folders from the extracted zip to the newly created HyperNav volder.
