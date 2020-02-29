module Msal
open Elmish
open Fable.Core
open Fable.SimpleJson

type Token =
    { accessToken : string
      tokenType   : string }

type Account = 
    { accountIdentifier: string
      homeAccountIdentifier: string
      userName: string
      name: string
      idToken: Map<string, string>
      idTokenClaims: Map<string, string>
      environment: string }

type private Msal =
    abstract acquireTokenSilent : unit -> JS.Promise<Token>
    abstract loginPopup : unit -> JS.Promise<_>
    abstract getAccount : unit -> string
    //abstract getAccount : unit -> Account // Remove the `JSON.stringify` in `msal.js` to get an Account object directly. Too much magic for my taste though.
    abstract logout : unit -> unit

[<ImportAll("../public/msal.js")>]
let private msal: Msal = jsNative

type State =
    { token : Token option
      errorMessage : string option
      accountInformation : Account option }

type Msg =
    | AcquireTokenSilent
    | TokenAcquired of Token
    | AttemptLogin of exn
    | CouldNotAcquireToken of exn
    | GetAccountInformation
    | Logout

let init() =
    { token = None
      errorMessage = None
      accountInformation = None }
    , Cmd.ofMsg AcquireTokenSilent // Start off by acquiring a token right away.

let update msg state =
        match msg with
        | AttemptLogin _ ->
            state, Cmd.OfPromise.either (fun _ -> msal.loginPopup()) () (fun _ -> AcquireTokenSilent) CouldNotAcquireToken

        | AcquireTokenSilent -> 
            state, Cmd.OfPromise.either (fun _ -> msal.acquireTokenSilent()) () TokenAcquired AttemptLogin

        | CouldNotAcquireToken exn ->
            { state with errorMessage = Some <| "Could not login. " + exn.Message }, Cmd.none

        | TokenAcquired token ->
            { state with token = Some token }, Cmd.ofMsg GetAccountInformation

        | GetAccountInformation ->
            let accountString = msal.getAccount()
            let account = 
                accountString
                |> Json.parseAs<Account>
            { state with accountInformation = Some account }, Cmd.none
            
        | Logout ->
            msal.logout()
            { state with accountInformation = None}, Cmd.none