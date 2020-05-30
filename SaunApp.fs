// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace SaunApp

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms

open FSharp.Data

module API =
    let URL = "https://thermo.77.fi"

    type Temperature = JsonProvider<"""
    {
      "sensor_id": "0123456789abcdef",
      "temperature": 0.1,
      "node_id": "00:00:00",
      "timestamp": "2020-05-29T19:12:24"
    }
    """>

    type Sensors = JsonProvider<"""
    ["abc","def"]
    """>

    let get_sensors =
        Sensors.Load(URL + "/sensors")

    let get_temp sensor_id =
        Temperature.Load(URL + "/temperature/" + sensor_id)

    let print_temp_with_ts (temp:Temperature.Root) =
        let ts = temp.Timestamp
        printfn "%s %f" (ts.ToString("yyyy-MM-dd HH:mm")) temp.Temperature

    let get_temps = Array.map get_temp

module App = 
    type Model = 
      { Temperatures : decimal[]
        Sensors : string[]
        TimerOn: bool }

    type Msg = 
        | TimedTick
        | GetTemps
        | Reset

    let initModel = { Temperatures=Array.zeroCreate 0; TimerOn=false; Sensors = API.get_sensors }

    let init () = initModel, Cmd.none

    let timerCmd =
        async { do! Async.Sleep 200
                return TimedTick }
        |> Cmd.ofAsyncMsg

    let update msg model =
        match msg with
        | TimedTick -> 
            if model.TimerOn then
                model, Cmd.none
            else 
                model, Cmd.none
        | GetTemps -> 
                { model with Temperatures = model.Sensors |> API.get_temps |> Array.map (fun t -> t.Temperature) }, timerCmd
        | Reset -> init()

    let view (model: Model) dispatch =
        View.ContentPage(
          content = View.StackLayout(padding = Thickness 20.0, verticalOptions = LayoutOptions.Center,
            children = [
                View.ListView( items= [
                                              for t in model.Temperatures do
                                              yield View.TextCell(sprintf "%+0.1f °C" t) ]
                             )
                View.Button(text="Update", command= (fun () -> dispatch GetTemps))
            ]))

    // Note, this declaration is needed if you enable LiveUpdate
    let program = XamarinFormsProgram.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
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
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


