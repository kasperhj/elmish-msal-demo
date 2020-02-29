module App

open Fable.React
open Fable.React.Props
open Elmish

type State = 
    { Count : int
      LoginState : Msal.State }

type Msg = 
    | Increment
    | Decrement
    | Login
    | MsalMsg of Msal.Msg

let init () : State * Cmd<Msg> = 
    let initialMsalState, initialMsalCmd = Msal.init()
    let initialCmd = Cmd.map MsalMsg initialMsalCmd // Use the initial command from Msal as the initial command of the root app.
    { Count = 42
      LoginState = initialMsalState }
    , initialCmd

let update msg state = 
    match msg with
    | Login ->
        state, Cmd.map MsalMsg ((Msal.update Msal.Msg.AcquireTokenSilent state.LoginState) |> snd)

    | MsalMsg msg' ->
        let msalState, msalCmd = Msal.update msg' state.LoginState 
        Browser.Dom.console.log msg'
        { state with LoginState = msalState}, Cmd.map MsalMsg msalCmd

    | Increment -> 
        { state with Count = state.Count + 1 }, Cmd.none

    | Decrement -> 
        { state with Count = state.Count - 1 }, Cmd.none

let formatUserName state =
    state.LoginState.accountInformation
    |> Option.map (fun x -> sprintf "%s (%s)" x.name x.userName)
    |> Option.defaultValue "No user logged in"

let view state dispatch = 
    div []
        [
            div []
                [
                    p [] [ str (formatUserName state)]
                ]
            button [OnClick (fun _ -> dispatch Login)] [ str "login"]
            button [OnClick (fun _ -> dispatch <| MsalMsg Msal.Msg.Logout)] [ str "logout"]
            br []
            br []
            button [OnClick (fun _ -> dispatch Decrement)] [ str "-"]
            h1 [] [ ofInt state.Count ]
            button [OnClick (fun _ -> dispatch Increment)] [ str "+" ]
        ]


open Elmish.React
open Elmish.Debug

Program.mkProgram init update view
|> Program.withReactSynchronous "root"
|> Program.withDebugger
|> Program.run