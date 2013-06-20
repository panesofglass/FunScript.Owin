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

    
    type FunScriptServer(nxt:Func<IDictionary<string, obj>, Task>, endPoint:string, source:string) = 
        
        member public this.Invoke(env:IDictionary<string, obj>):Task = 
            let path = tryGet<string> OwinConstants.RequestPath env

            match path with
            | Some(p) ->
                if PrefixMatcher.isMatch p endPoint then
                    env 
                    |> response source
                    |> writeHeader "Content-Type" "application/javascript"
                    |> ignore

                    emptyTask()

                else nxt.Invoke(env)                
            | None -> nxt.Invoke(env)
            


    type IAppBuilder with
        
        member this.mapScript (endPoint:string, expression, ?components) = 
            let components = defaultArg components []
            let source = FunScript.Compiler.Compiler.Compile(expression, components=components)
            this.Use(typeof<FunScriptServer>, endPoint, source)





        