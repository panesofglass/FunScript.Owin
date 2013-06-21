namespace FunScript.Owin.Test

open Owin
open FunScript.Owin

open FunScript
open FunScript.TypeScript

open System.Reflection

open Microsoft.Owin.Hosting

[<FunScript.JS>]
module JS = 
    type ts = Api<"../Typings/lib.d.ts">

    [<Export("script.js")>]
    let jsmain() = 
        ts.alert("Hello world")


module Program = 

    type Startup() =
        member public this.Configuration(app:IAppBuilder) =
            app.mapScript("scripts", Assembly.GetExecutingAssembly(), Interop.Components.all) |> ignore           

    [<EntryPoint>]
    let main argv = 
        //end point
        let url = "http://localhost:8080"
        //web app
        use a = WebApplication.Start<Startup>(url)

        System.Console.WriteLine("Server running at {0}",url)      
        System.Console.ReadLine() |> ignore
        
        //exit upon pressing a key
        a.Dispose()
        //sub.Dispose();
        0