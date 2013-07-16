namespace FunScript.Owin.Example

open Owin
open FunScript.Owin

open FunScript
open FunScript.TypeScript

open System.Reflection
open System.IO

open Microsoft.Owin.Hosting
open Nancy
open Nancy.Conventions

[<FunScript.JS>]
module JS = 
    type ts = Api<"https://github.com/c9/typescript/raw/master/typings/lib.d.ts">

    [<Export("canvas.js")>]
    let canvas() = 
        let canvas = unbox<ts.HTMLCanvasElement>(ts.document.getElementById("canvas"))
        canvas.width <- 1000.
        canvas.height <- 800.
        let ctx = canvas.getContext("2d")
        ctx.fillStyle <- "rgb(200,0,0)"
        ctx.fillRect (10., 10., 55., 50.);
        ctx.fillStyle <- "rgba(0, 0, 200, 0.5)"
        ctx.fillRect (30., 30., 55., 50.)


module Program = 

    type ApplicationBootstrapper() =
        inherit DefaultNancyBootstrapper()
        override this.ConfigureConventions(nancyConventions:NancyConventions) =
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("web", @"web"))
            base.ConfigureConventions(nancyConventions);

    type Startup() =
        member public this.Configuration(app:IAppBuilder) =
            let asm = Assembly.GetExecutingAssembly()
            let loc = asm.Location
            let staticPath = Path.Combine(loc, "web")

            //app.UseFunScript("scripts", asm) |> ignore
            app.UseFunScript("scripts", asm, Interop.Components.all) |> ignore
            app.UseNancy(new ApplicationBootstrapper()) |> ignore




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