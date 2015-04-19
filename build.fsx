// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open System.Diagnostics
open System.Reflection

// properties
let buildDir = "./build/"
let buildLibDir = "./build/lib/"

let accessKey = ReadFileAsString "my.nukey"

// targets
Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "BuildEx" (fun _ ->
    !! "src/Velyo.Extensions/Velyo.Extensions.csproj"
        |> MSBuildRelease buildLibDir "Build"
        |> Log "BuildEx-Output: "
    let version = GetAssemblyVersionString "build\lib\Velyo.Extensions.dll"
    NuGet (fun p -> 
        {p with
            Project = "Velyo.Exensions"
            Version = version
            WorkingDir = buildDir
            OutputPath = buildDir
            AccessKey = accessKey
            Publish = true}) 
        "nuspec/Velyo.Extensions.nuspec"
)

Target "BuildCollEx" (fun _ ->
    !! "src/Velyo.Collections.Extensions/Velyo.Collections.Extensions.csproj"
        |> MSBuildRelease buildLibDir "Build"
        |> Log "BuildCollEx-Output: "
    let version = GetAssemblyVersionString "build\lib\Velyo.Collections.Extensions.dll"
    NuGet (fun p -> 
        {p with
            Project = "Velyo.Collections.Exensions"
            Version = version
            WorkingDir = buildDir
            OutputPath = buildDir
            AccessKey = accessKey
            Publish = true}) 
        "nuspec/Velyo.Collections.Extensions.nuspec"
)

Target "BuildIOEx" (fun _ ->
    !! "src/Velyo.IO.Extensions/Velyo.IO.Extensions.csproj"
        |> MSBuildRelease buildLibDir "Build"
        |> Log "BuildIOEx-Output: "
    let version = GetAssemblyVersionString "build\lib\Velyo.IO.Extensions.dll"
    NuGet (fun p -> 
        {p with
            Project = "Velyo.IO.Exensions"
            Version = version
            WorkingDir = buildDir
            OutputPath = buildDir
            AccessKey = accessKey
            Publish = true}) 
        "nuspec/Velyo.IO.Extensions.nuspec"
)

Target "BuildWebEx" (fun _ ->
    !! "src/Velyo.Web.Extensions/Velyo.Web.Extensions.csproj"
        |> MSBuildRelease buildLibDir "Build"
        |> Log "BuildWebEx-Output: "
    let version = GetAssemblyVersionString "build\lib\Velyo.Web.Extensions.dll"
    NuGet (fun p -> 
        {p with
            Project = "Velyo.Web.Exensions"
            Version = version
            WorkingDir = buildDir
            OutputPath = buildDir
            AccessKey = accessKey
            Publish = true}) 
        "nuspec/Velyo.Web.Extensions.nuspec"
)

Target "Default" (fun _ ->
    trace "Please use one of next targets:"
    trace "- BuildEx, BuildCollEx, BuildIOEx, BuildWebEx"
)

// dependencies
"Clean"
    ==> "BuildEx"
"Clean"
    ==> "BuildCollEx"
"Clean"
    ==> "BuildIOEx"
"Clean"
    ==> "BuildWebEx"

// start build
RunTargetOrDefault "Default"