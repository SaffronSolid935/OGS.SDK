# OpenGameSync.SDK

OpenGameSync.SDK is an open-source software development kit (SDK) for game synchronisation.
While it is not a complete solution, it provides a solid foundation for developing custom synchronisation logic (it is a framework-like package). <br>
For more information, see [Usage](#usage).

## ToC

- [Install in project](#install-in-project)
    - [Download NuGet package and use it localy (recommend)](#download-nuget-package-and-use-it-localy-recommend)
    - [Cloning into project](#cloning-into-project)
    - [As Submodule](#as-submodule)
- [Usage](#usage)

## Install in project

> ⚠️ This package is currently not published on NuGet.
The only way to include it is by adding it as a Git submodule or Building on your own as DLL.
(A package release may follow in the future.)

### Download NuGet package and use it localy (recommend)

1. Open [`Releases`](https://github.com/SaffronSolid935/OGS.SDK/releases/tag/latest) and download the attached NuGet package.
2. Save the NuGet package to a directory of your choice.
3. In your C# porject run: <br>
    ```bash
    dotnet nuget add source "C:\path\to\nuget-packages" --name LocalPackages # or the name of your choice
    ```
4. Do one of these steps:
    - If your project doesn't already have the package referenced: <br>
        ```bash
        dotnet add package OGS.SDK 
        ```
    - If your project already has the relevant package: <br>
        ```bash
        dotnet restore
        ```

### Cloning into project

1. Clone the repository into your solution folder:<br>
    ```bash
    git clone https://github.com/SaffronSolid935/OGS.SDK.git
    ```

2. Add the SDK project to your solution:<br>
    ```bash
    dotnet sln <YourSolution>.sln add OGS.SDK/OpenGameSync.SDK.csproj
    ```

3. Add a reference from your main project:<br>
    ```bash
    dotnet add <your cs.project> reference <OGS.SDK folder>.OpenGameSync.SDK.csproj
    ```

### As Submodule

1. Add the SDK as a Git submodule:<br>
    ```bash
    git add submodule https://github.com/SaffronSolid935/OGS.SDK.git <pathOfSDK>
    ```
2. Add the SDK project to your solution:<br>
    ```bash
    dotnet sln <YourSolution>.sln add <pathOfSDK>/OpenGameSync.SDK.csproj
    ```

3. Add a reference from your main project:<br>
    ```bash
    dotnet add <your cs.project> reference <pathOfSDK>/OpenGameSync.SDK.csproj
    ```


## Usage

### General (use in application)

You need to create a SyncManager for every save path (usually one save path per game).

```cs
const string localPath = "/path/to/your/saves";
SyncManager syncManager = new SyncManager<File>(localPath); // or use your custom class for it
```

To download the saves, you need to fetch the remote and run SyncToLocal. 

⚠️ Both methods are async.

```cs
await syncManager.FetchRemote();
await syncManager.SyncToLocal();
```

To upload the saves, you need to fetch the local and run SyncToRemote() (Only `SyncToRemote` is async).

```cs
syncManager.FetchLocal();
await syncManager.SyncToRemote();
```

### Inherit the SDK

To integrate your own remote storage logic, you need to inherit from the File class and SyncManager.

**Inheriting from `File`**

- You need to override:
    - `async Task LoadRemoteAsync()`
        - This will be called, if you want to download the saves (called the SyncManager)
    - `async Task SaveRemoteAsync()`
        - This will be called, if you want to upload the saves (called by the SyncManager)

**Example:**
```cs
using OpenGameSync.SDK;

public class CustomFile : File
{
    private string customVariable;
    public CustomFile(string localPath, string relativePath, string customVariable) : base(localPath, relativePath)
    {
        this.customVariable = customVariable;
    }

    public override async Task LoadRemoteAsync()
    {
        this.content = new byte[0];
    }

    public override async Task SaveRemoteAsync()
    {
        Upload(this.content);
    }
}
```

**Inheriting from `SyncManager`:**

- Be aware of that the SyncManager excepts an Type. You don't have to set it in your child class, but remember it. (In my example you will see an easy way to use it)
- The SyncManager is the thing, which manages the the saves. You have to override:
    - `async Task FetchRemote() - public`
        - This should update a class variable called `remoteFiles` of the type `List<T>` where T is based of `File`
    - `async Task ClearRemote() - protected`
        - This should delete all remote files
    - `T InitLocalFile() - protected`
        - This is implemented for the case you have an custom constructor. Here you can you edit the initialization

```cs
public class CustomSyncManager : SyncManager<CustomFile>
{
    private string customRemote;
    public CustomSyncManager(string localPath, string customRemote) : base(localPath)
    {
        // your custom constructor
        this.customRemote = customRemote;
    }

    public override async Task FetchRemote()
    {
        this.remoteFiles.Add(new CustomFile(localPath,"relative path", this.customRemote));
    }

    protected override async Task ClearRemote()
    {
        //delete remote files
        this.remoteFiles.Clear();
    }

    protected override CustomFile InitLocalFile(string localPath, string relativePath) // NOTE: older SDKs named this 'absolutePath' (incorrect)
    {
        return new CustomFile(localPath,relativePath,this.customRemote);
    }
}
```