namespace FunScript.Owin

[<RequireQualifiedAccess>]
module SourceWrapper =
    
    [<Literal>]
    let jQuery = "$(function() {{ (function() {{ {0} }})() }});"

