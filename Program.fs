﻿open System
open System.Globalization

let monthRes monthArg =
    match DateTime.TryParseExact(monthArg, "yyyy-MM", null, DateTimeStyles.None) with
    | true, _ -> Ok monthArg
    | _ -> Error "The first argument should be a valid month in the yyyy-MM format"

let hoursRes (hoursArg: string) =
    match Int32.TryParse hoursArg with
    | true, hours when hours >= 0 -> Ok hours
    | _ -> Error "The hours argument should be a positive integer"

let rateRes (rateArg: string) =
    match Decimal.TryParse rateArg with
    | true, rate when rate >= 0.0m -> Ok rate
    | _ -> Error "The rate argument should be a positive decimal"

let earningsRes monthArg hoursArg rateArg =
    match monthRes monthArg with
    | Error msg -> Error msg
    | Ok _ ->
        match hoursRes hoursArg with
        | Error msg -> Error msg
        | Ok hours ->
            match rateRes rateArg with
            | Error msg -> Error msg
            | Ok rate -> Ok(decimal hours * rate)

[<EntryPoint>]
let main args =
    match args with
    | [| monthArg; hoursArg; rateArg |] ->
        match earningsRes monthArg hoursArg rateArg with
        | Ok earnings ->
            Console.WriteLine $"Your earnings for {monthArg} are {earnings}"
            0
        | Error msg ->
            Console.WriteLine msg
            1
    | _ ->
        Console.WriteLine "Please provide three arguments"
        1