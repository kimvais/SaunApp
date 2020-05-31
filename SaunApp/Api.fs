namespace SaunApp.Api

open FSharp.Data

module API =
    let URL = "https://thermo.77.fi"
    // let URL = "http://127.0.0.1:8000"

    type Temperature = JsonProvider<"""
    {
      "sensor_id": "0123456789abcdef",
      "temperature": 0.1,
      "node_id": "00:00:00",
      "timestamp": "2020-05-29T19:12:24"
    }
    """>

    type Sensors = JsonProvider<"""
    [{"sensor_id": "01234567890abcdef", "name": "Sensor name"}]
    """>

    let get_sensors =
        Sensors.Load(URL + "/sensors") |> Array.map (fun s -> (s.SensorId, s.Name)) |> Map.ofArray

    let get_temp sensor_id =
        Temperature.Load(URL + "/temperature/" + sensor_id)

    let print_temp_with_ts (temp: Temperature.Root) =
        let ts = temp.Timestamp
        printfn "%s %f" (ts.ToString("yyyy-MM-dd HH:mm")) temp.Temperature

    let get_temps sensors = sensors |> Seq.map get_temp |> Seq.toArray
