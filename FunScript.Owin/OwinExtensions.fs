namespace FunScript.Owin

open System.Collections.Generic
open System
open Owin

open System.Reflection
open System.Threading.Tasks

open Microsoft.FSharp.Quotations

module OwinExtensions = 

    type AppFunc = Func<IDictionary<string, Object>, Task>

    let tryGet<'t> (key:string) (env:IDictionary<string, Object>) = 
        let res, v = env.TryGetValue key
        if res then Some(v :?> 't) else None

    let emptyTask() = 
        let tcs = new TaskCompletionSource<Object>()
        tcs.SetResult Unchecked.defaultof<Object>
        tcs.Task :> Task

    let response (resp:string) (env:IDictionary<string, Object>) =
        env.[OwinConstants.ResponseBody] <- resp
        env

    let writeHeader (key:string) (value:string) (env:IDictionary<string, Object>) =
        let headers = tryGet<IDictionary<string, string[]>> OwinConstants.ResponseHeaders env
        match headers with
        | Some(h) -> 
            h.[key] <- [|value|]
            env
        | None -> env

    
    type FunScriptServer(app:AppFunc, endPoint:string, source:string) = 
        
        let Invoke(env:IDictionary<string, Object>):Task = 
            let path = tryGet<string> OwinConstants.RequestPath env

            match path with
            | Some(p) ->
                if PrefixMatcher.isMatch p endPoint then
                    env 
                    |> response source
                    |> writeHeader "Content-Type" "application/javascript"
                    |> ignore

                    emptyTask()

                else app.Invoke(env)                
            | None -> app.Invoke(env)
            
            

        


    type IAppBuilder with
        
        member this.mapScript (endPoint:string, mi:MethodInfo, ?components) = 
            let expr = Expr.Call(mi, [])
            let components = defaultArg components []
            let source = FunScript.Compiler.Compiler.Compile(expr, components=components)
            this.Use(typeof<FunScriptServer>, endPoint, source)
        