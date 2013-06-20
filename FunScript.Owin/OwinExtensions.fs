namespace FunScript.Owin

open System.Collections.Generic
open System
open Owin

open System.Reflection
open System.Threading.Tasks

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection

open System.IO
open System.Text

[<AutoOpen>]
module OwinExtensions = 

    type ExportAttribute(scriptName:string) = 
        inherit Attribute()
        member this.ScriptName = scriptName

    let private tryGet<'t> (key:string) (env:IDictionary<string, obj>) = 
        let res, v = env.TryGetValue key
        if res then Some(v :?> 't) else None

    let private emptyTask() = 
        let tcs = new TaskCompletionSource<Object>()
        tcs.SetResult Unchecked.defaultof<Object>
        tcs.Task :> Task

    let private response (resp:string) (env:IDictionary<string, obj>) =

        let response = env |> tryGet<Stream> OwinConstants.ResponseBody

        match response with
        | Some(r) ->
            let bytes = Encoding.UTF8.GetBytes(resp) 
            r.Write(bytes, 0, bytes.Length)
            env
        | None -> env


    let private writeHeader (key:string) (value:string) (env:IDictionary<string, obj>) =
        let headers = tryGet<IDictionary<string, string[]>> OwinConstants.ResponseHeaders env
        match headers with
        | Some(h) -> 
            h.[key] <- [|value|]
            env
        | None -> env

    
    let inline isNull< ^a when ^a : not struct> (x:^a) =
        obj.ReferenceEquals (x, Unchecked.defaultof<_>)

    type FunScriptServer(nxt:Func<IDictionary<string, obj>, Task>, basePath:string, asm:Assembly, components) = 

        let getSources() =             
            let types = asm.GetTypes()
            let flags = BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Static
            
            seq { for typ in types do
                for mi in typ.GetMethods(flags) do
                    let attr = mi.GetCustomAttribute(typedefof<ExportAttribute>, false) :?> ExportAttribute
                    if isNull(attr) |> not then
                        let expr = Expr.Call(mi, [])
                        let src = FunScript.Compiler.Compiler.Compile(expr, components=components)
                        yield (src, attr.ScriptName) 
            }
            |> Seq.cache

        let sources = lazy(getSources())

        member public this.Invoke(env:IDictionary<string, obj>):Task = 
            let path = tryGet<string> OwinConstants.RequestPath env
            
            

            match path with
            | Some(p) ->

                let sourcesUnwrapped= sources.Force()
                let concatPath basePath scriptName = String.Format("{0}/{1}", basePath, scriptName)

                let foundPair = sourcesUnwrapped
                                |> Seq.tryFind (fun (src, scriptName) -> PrefixMatcher.isMatch p (concatPath basePath scriptName))

                match foundPair with
                | Some(src, scriptName) -> 
                    env 
                    |> response src
                    |> writeHeader "Content-Type" "application/javascript"
                    |> ignore

                    emptyTask()

                | None -> nxt.Invoke(env)               
            | None -> nxt.Invoke(env)
            


    type IAppBuilder with
        
        member this.mapScript (basePath:string, asm:Assembly, ?components) = 
            let components = defaultArg components []
            this.Use(typeof<FunScriptServer>, basePath, asm, components)





        