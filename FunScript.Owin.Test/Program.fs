namespace FunScript.Owin.Test

open Owin
open FunScript.Owin

open FunScript
open FunScript.TypeScript


open Microsoft.Owin.Hosting


module Program = 

    type ts = Api<"../Typings/lib.d.ts">

    let jsmain() = ts.alert("Hello world") |> ignore

    type Startup() =
        member public this.Configuration(app:IAppBuilder) =
            app.mapScript("/script.js", <@@ ts.alert("Hello world") @@>, Interop.Components.all) |> ignore


            

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