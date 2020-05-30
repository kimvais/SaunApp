namespace SaunApp.Api

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

    let print_temp_with_ts (temp: Temperature.Root) =
        let ts = temp.Timestamp
        printfn "%s %f" (ts.ToString("yyyy-MM-dd HH:mm")) temp.Temperature

    let get_temps = Array.map get_temp
