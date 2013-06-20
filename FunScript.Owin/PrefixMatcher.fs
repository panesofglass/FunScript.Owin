namespace FunScript.Owin

open System

[<RequireQualifiedAccess>]
module PrefixMatcher =

    let ensureStartsWithSlash (path:string) = if path.Length = 0 || path.[0] = '/' then path else "/" + path

    let isMatch path pathBase =
        let pathBase = ensureStartsWithSlash pathBase
        let path = ensureStartsWithSlash path
        let pathLength = path.Length
        let pathBaseLength = pathBase.Length

        if pathLength < pathBaseLength then false
        else if pathLength > pathBaseLength && path.[pathBaseLength] <> '/' then false
        else if path.StartsWith (pathBase, StringComparison.OrdinalIgnoreCase) |> not then false
        else true

