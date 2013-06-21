﻿namespace FunScript.Owin

open System.Collections.Generic
open System
open Owin

open System.Reflection
open System.Threading.Tasks

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection

open System.IO
open System.Text

open FunScript
open FunScript.Compiler

[<AutoOpen>]
module OwinExtensions = 

    type ExportAttribute(scriptName:string) = 
        inherit Attribute()
        member this.ScriptName = scriptName

    
    module private Task = 
        let awaitTask (t: Task) = t |> Async.AwaitIAsyncResult |> Async.Ignore
        let doAsyncTask  (f : unit->'a) = async { return! Task<'a>.Factory.StartNew( new Func<'a>(f) ) |> Async.AwaitTask }

    module private Util = 
        let inline ensurePathStartsWithSlash (path:string) = if path.Length = 0 || path.[0] = '/' then path else "/" + path

        let isPathMatch path pathBase =
            let pathBase = ensurePathStartsWithSlash pathBase
            let path = ensurePathStartsWithSlash path
            let pathLength = path.Length
            let pathBaseLength = pathBase.Length

            if pathLength < pathBaseLength then false
            else if pathLength > pathBaseLength && path.[pathBaseLength] <> '/' then false
            else if path.StartsWith (pathBase, StringComparison.OrdinalIgnoreCase) |> not then false
            else true

        let inline isNull< ^a when ^a : not struct> (x:^a) = obj.ReferenceEquals (x, Unchecked.defaultof<_>)
        let inline concatPath basePath scriptName = String.Format("{0}/{1}", basePath, scriptName)
        let inline tryGet<'t> (key:string) (env:IDictionary<string, obj>) = 
            let res, v = env.TryGetValue key
            if res then Some(v :?> 't) else None

    module private Owin = 
        let response (resp:string) (env:IDictionary<string, obj>) = async {
            let response = env |> Util.tryGet<Stream> OwinConstants.ResponseBody

            match response with
            | Some(r) ->
                let bytes = Encoding.UTF8.GetBytes(resp) 
                do! r.WriteAsync(bytes, 0, bytes.Length) |> Task.awaitTask
            | None -> ()
        }

        let writeHeader (key:string) (value:string) (env:IDictionary<string, obj>) =
            let headers = env |> Util.tryGet<IDictionary<string, string[]>> OwinConstants.ResponseHeaders
            match headers with
            | Some(h) -> 
                h.[key] <- [|value|]
                env
            | None -> env


    type private ScriptManifest(asm:Assembly, basePath) =
        let getSources() =             
            let types = asm.GetTypes()
            let flags = BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Static
            
            seq {
                for typ in types do
                    for mi in typ.GetMethods(flags) do
                        match mi.GetCustomAttribute(typedefof<ExportAttribute>, false) with
                        | :? ExportAttribute as eattr -> yield (mi, eattr.ScriptName)
                        | _ -> ()
            }

        let sources = lazy(getSources())

        member this.TryGet(path,components):Async<Option<string>> = async {
            let sourcesUnwrapped = sources.Force()
            let fpair = sourcesUnwrapped |> Seq.tryFind (fun (src, scriptName) -> Util.isPathMatch path (Util.concatPath basePath scriptName))

            match fpair with
            | Some(mi, scriptName) -> 
                let expr = Expr.Call(mi, [])
                let! src = Task.doAsyncTask (fun () -> Compiler.Compile(expr, components))
                return Some(src)
            | None -> return None
        } 

            
            
    type FunscriptMiddleware(nxt:Func<IDictionary<string, obj>, Task>, basePath:string, asm:Assembly, components) = 
        let manifest = new ScriptManifest(asm, basePath)
        member public this.Invoke(env:IDictionary<string, obj>):Task =
            let pathOption = env |> Util.tryGet<string> OwinConstants.RequestPath

            match pathOption with
            | Some(path) -> 
                async {
                    let! so = manifest.TryGet(path,components)
                    match so with
                    | Some(src) ->
                        do! env |> Owin.writeHeader "Content-Type" "application/javascript"
                                |> Owin.response src
                    | None -> do! nxt.Invoke(env) |> Task.awaitTask
                }
                |> Async.StartAsTask :> Task
            | None -> nxt.Invoke(env)       


    type IAppBuilder with
        
        member this.UseFunScript(basePath:string, asm:Assembly, ?components) = 
            let com = defaultArg components []
            this.Use(typeof<FunscriptMiddleware>, basePath, asm, com)




        