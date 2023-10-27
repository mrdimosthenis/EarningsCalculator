open System
open System.Globalization

let monthOpt monthArg =
    match DateTime.TryParseExact(monthArg, "yyyy-MM", null, DateTimeStyles.None) with
    | true, _ -> Some monthArg
    | _ -> None

let hoursOpt (hoursArg: string) =
    match Int32.TryParse hoursArg with
    | true, hours when hours >= 0 -> Some hours
    | _ -> None

let rateOpt (rateArg: string) =
    match Decimal.TryParse rateArg with
    | true, rate when rate >= 0.0m -> Some rate
    | _ -> None
    
open FsToolkit.ErrorHandling

let earningsOpt monthArg hoursArg rateArg =
    option {
        let! _ = monthOpt monthArg
        let! hours = hoursOpt hoursArg
        let! rate = rateOpt rateArg
        return decimal hours * rate
    }

[<EntryPoint>]
let main args =
    match args with
    | [| monthArg; hoursArg; rateArg |] ->
        match earningsOpt monthArg hoursArg rateArg with
        | Some earnings ->
            Console.WriteLine $"Your earnings for {monthArg} are {earnings}"
            0
        | None ->
            Console.WriteLine "Your input is invalid"
            1
    | _ ->
        Console.WriteLine "Please provide three arguments"
        1