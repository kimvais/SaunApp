// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace SaunApp

open System
open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms


open SaunApp.Api
open SaunApp.Utils
open SaunApp.Style

module App =
    type Model =
        { Temperatures: API.Temperature.Root []
          Sensors: Map<string, string> }

    type Msg =
        | GetTemps
        | Reset

    let initModel =
        { Temperatures = Array.zeroCreate 0
          Sensors = API.get_sensors }

    let periodicPoll () =
        async {
            Console.WriteLine "Sleeping"
            do! Async.Sleep 20000
            Console.WriteLine "Getting temperatures..."
            return GetTemps
        }
        |> Cmd.ofAsyncMsg

    let getTemps () =
        GetTemps |> Cmd.ofMsg

    type CmdMsg =
        | PeriodicPoll
        | GetTempsCmd

    let mapCmdMsgToCmd cmdMsg =
        match cmdMsg with
        | PeriodicPoll -> periodicPoll ()
        | GetTempsCmd -> getTemps ()

    let init () = initModel, [ GetTempsCmd; PeriodicPoll ]

    let update msg model =
        match msg with
        | GetTemps ->
            { model with
                  Temperatures =
                      model.Sensors
                      |> Map.toSeq
                      |> Seq.map fst
                      |> API.get_temps }, []
        | Reset -> init ()

    let timestamp_text (t: API.Temperature.Root) = humandate t.Timestamp
    let temperature_text (t: API.Temperature.Root) = sprintf "%+0.1f °C" t.Temperature
    let sensorname_text (model: Model) (t: API.Temperature.Root) =
        match model.Sensors.TryFind t.SensorId with
        | Some name -> sprintf "%s (%s)" name (timestamp_text t)
        | None -> sprintf "Unnamed sensor %s (%s)" t.SensorId (timestamp_text t)
    
    let view (model: Model) dispatch =
        View.NavigationPage
            (pages =
                [ View.ContentPage
                    (title = "SaunaApp",
                     content =
                         View.StackLayout
                             (padding = Thickness 20.0,
                              verticalOptions = LayoutOptions.FillAndExpand,
                              children =
                                  [ for t in model.Temperatures do
                                      yield View.Label
                                                (text = sensorname_text model t,
                                                 fontFamily = textFont,
                                                 textColor = Colors.PrimaryD)
                                      yield View.Label
                                                (text = temperature_text t,
                                                 fontSize = FontSize(32.0),
                                                 fontFamily = textFont) ]
                                  @ [ Design.materialButton Icon.Refresh Colors.Secondary Colors.Text
                                          (fun () -> dispatch GetTemps) ])) ],
             barBackgroundColor = Colors.Primary,
             barTextColor = Colors.Text)

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgramWithCmdMsg init update view mapCmdMsgToCmd

type App() as app =
    inherit Application()

    let runner =
        App.program
        |> Program.withConsoleTrace
        |> XamarinFormsProgram.run app

#if DEBUG
// Uncomment this line to enable live update in debug mode.
// See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
//
//do runner.EnableLiveUpdate()
#endif

// Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
// See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
    let modelId = "model"

    override __.OnSleep() =

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() =
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) ->

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel(model, Cmd.none)

            | _ -> ()
        with ex ->
            App.program.onError ("Error while restoring model found in app.Properties", ex)

    override this.OnStart() =
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif
