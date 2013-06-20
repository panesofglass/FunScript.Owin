namespace FunScript.Owin

[<RequireQualifiedAccess>]
module OwinConstants = 

    [<Literal>]
    let Version = "owin.Version"

    [<Literal>]
    let RequestBody = "owin.RequestBody"
    [<Literal>]
    let RequestHeaders = "owin.RequestHeaders"
    [<Literal>]
    let RequestScheme = "owin.RequestScheme"
    [<Literal>]
    let RequestMethod = "owin.RequestMethod"
    [<Literal>]
    let RequestPathBase = "owin.RequestPathBase"
    [<Literal>]
    let RequestPath = "owin.RequestPath"
    [<Literal>]
    let RequestQueryString = "owin.RequestQueryString"
    [<Literal>]
    let RequestProtocol = "owin.RequestProtocol"

    [<Literal>]
    let CallCancelled = "owin.CallCancelled"

    [<Literal>]
    let ResponseStatusCode = "owin.ResponseStatusCode"
    [<Literal>]
    let ResponseReasonPhrase = "owin.ResponseReasonPhrase"
    [<Literal>]
    let ResponseHeaders = "owin.ResponseHeaders"
    [<Literal>]
    let ResponseBody = "owin.ResponseBody"

    [<Literal>]
    let TraceOutput = "host.TraceOutput"

    [<Literal>]
    let User = "server.User"
    [<Literal>]
    let RemoteIpAddress = "server.RemoteIpAddress"
    [<Literal>]
    let RemotePort = "server.RemotePort"
    [<Literal>]
    let LocalIpAddress = "server.LocalIpAddress"
    [<Literal>]
    let LocalPort = "server.LocalPort"

    [<Literal>]
    let DisableRequestCompression = "systemweb.DisableResponseCompression"
    [<Literal>]
    let DisableRequestBuffering = "server.DisableRequestBuffering"
    [<Literal>]
    let DisableResponseBuffering = "server.DisableResponseBuffering"

    [<Literal>]
    let ServerCapabilities = "server.Capabilities"
    [<Literal>]
    let WebSocketVersion = "websocket.Version"
    [<Literal>]
    let WebSocketAccept = "websocket.Accept"

    [<Literal>]
    let HostOnAppDisposing = "host.OnAppDisposing"
    [<Literal>]
    let HostAppNameKey = "host.AppName"
    [<Literal>]
    let HostAppModeKey = "host.AppMode"
    [<Literal>]
    let AppModeDevelopment = "development"

