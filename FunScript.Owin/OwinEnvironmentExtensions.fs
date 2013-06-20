namespace FunScript.Owin

open System.Collections.Generic
open System

module OwinEnvironmentExtensions = 

    let tryGet (key:string) (env:IDictionary<string, Object>) = 
        let res, v = env.TryGetValue key
        if res then Some(v) else None
