// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace SaunApp

open System
open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms

open FSharp.Data

module Colors =
    let Primary = Color.FromHex("#8bc34a")
    let PrimaryL = Color.FromHex("#bef67a")
    let PrimaryD = Color.FromHex("#5a9216")
    let Secondary = Color.FromHex("#ffc400")
    let SecondaryL = Color.FromHex("#fff64f")
    let SecodaryD = Color.FromHex("#c79400")
    let Text = Color.Black
    let Error = Color.FromHex("#B00020")
    
module Design =
    let materialFont =
        (match Device.RuntimePlatform with
                                 | Device.iOS -> "Material Design Icons"
                                 | Device.Android -> "materialdesignicons_webfont.ttf#Material Design Icons"
                                 | Device.WPF -> "materialdesignicons-webfont.ttf#Material Design Icons"
                                 | _ -> null)
    
    let materialButton materialIcon backgroundColor textColor command =
        View.Button(text = materialIcon,
            command = command,
            fontFamily = materialFont,
            fontSize = FontSize(20.),
            backgroundColor = backgroundColor,
            textColor = textColor,
            cornerRadius = 10,
            borderColor = backgroundColor
            )
        
        
        
module Icon =
    let Refresh = "\U000F0450"
    
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
        | GetTemps
        | Reset

    let initModel = { Temperatures=Array.zeroCreate 0; TimerOn=false; Sensors = API.get_sensors }


    let periodicPoll() =
        async {
                Console.WriteLine "Sleeping"
                do! Async.Sleep 20000
                Console.WriteLine "Getting temperatures..."
                return GetTemps }
        |> Cmd.ofAsyncMsg

    let getTemps() =
        GetTemps |> Cmd.ofMsg
        
    type CmdMsg =
        | PeriodicPoll
        | GetTempsCmd
        
    let mapCmdMsgToCmd cmdMsg =
        match cmdMsg with
        | PeriodicPoll -> periodicPoll()
        | GetTempsCmd -> getTemps()
        
    let init () = initModel, [GetTempsCmd; PeriodicPoll]
    
    let update msg model =
        match msg with
        | GetTemps -> 
                { model with Temperatures = model.Sensors |> API.get_temps |> Array.map (fun t -> t.Temperature) }, []
        | Reset -> init()

    let view (model: Model) dispatch =
        View.NavigationPage(
            pages=[
            View.ContentPage( title = "SaunaApp",
              content = View.StackLayout(padding = Thickness 20.0, verticalOptions = LayoutOptions.StartAndExpand,
                                         
                children = [ for t in model.Temperatures do
                              yield View.Label(text = sprintf "%+0.1f °C" t, fontSize=FontSize(32.0), fontFamily="Trebuchet")
                           ] @ [
                    Design.materialButton Icon.Refresh Colors.Secondary Colors.Text (fun () -> dispatch GetTemps)
                    ]))],
            barBackgroundColor = Colors.Primary,
            barTextColor = Colors.Text
        )

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgramWithCmdMsg init update view mapCmdMsgToCmd

type App () as app = 
    inherit Application ()

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
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


