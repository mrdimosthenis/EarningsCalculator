open System
open System.Globalization

[<EntryPoint>]
let main args =
    match args with
    | [| monthArg; hoursArg; rateArg |] ->
        DateTime.ParseExact(monthArg, "yyyy-MM", null, DateTimeStyles.None) |> ignore
        let hours = hoursArg |> int |> decimal
        let rate = Decimal.Parse(rateArg)
        Console.WriteLine $"Your earnings for {monthArg} are {hours * rate}"
        0
    | _ ->
        Console.WriteLine "Please provide three arguments"
        1