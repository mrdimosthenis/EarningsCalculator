# Understanding Error Handling in F#

### A Practical Approach Through a Console Application

All developers will encounter error handling at some point, especially when 
working with strongly-typed functional programming languages. This article will 
focus on the basic error-related types found in F# and how to use them in practical ways.

We will learn through creating a simple console application that takes three inputs:
1. A month in the `yyyy-MM` format,
2. Working hours as a positive integer,
3. Hourly rate as a positive decimal.

This application will calculate and display a worker's monthly earnings.

We will start with a basic program that gets the job done. Then, step by step,
we will improve it. In each section of the article, a new error-handling concept
will be introduced and explained.

_All the code you'll see is in [this](https://github.com/mrdimosthenis/EarningsCalculator)
repo, with different commits for each section._

## Basic Implementation

If our machine has the _.NET Core SDK_ installed, we can create a new F# console
application by running the following commands, and then navigate into the
newly-created directory.

```bash
dotnet new console -lang F# -o EarningsCalculator
cd EarningsCalculator
```

Replace the contents of the `Program.fs` file with the following code:

```fsharp
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
```

In this first version, our program checks if all three arguments are provided,
parses them, calculates, and prints the result:
1. We parse the `monthArg` to ensure it's in the `yyyy-MM` format, but we `ignore`
the parsed value. 
2. To multiply the _working hours_ with the _hourly rate_, we convert the `hoursArg`
from `string` to `int`, and then to `decimal`. 
3. We parse the `rateArg` as `decimal`.

Now, let's check the behavior of our program! If we execute
`dotnet run 2023-01 150 30.0`, we'll get `Your earnings for 2023-01 are 4500.0`.
That's nice, but what if we provide invalid input? If we execute
`dotnet run 2022-13 150 30.0`, our program will crash due to a `FormatException`
related to `DateTime`. It's clear we need to handle errors. Instead of catching
exceptions, we will use appropriate types in the upcoming sections.

## The Option Type

Strongly-typed functional languages like F# discourage the use of `null` values to
avoid null-related exceptions. Instead, they employ specific types to deal with missing
values, and in F#, the `Option` type serves this purpose.

To handle potential invalid _hourly rate_ values, we could treat the parsed rate as a
`decimal option`. Here, `Some rate` would indicate a successful parsing result,
while `None` would signify failure.

Let's replace the contents of the `Program.fs` file with functions that utilize the
`Option` type to handle parsing failures:

```fsharp
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
```

Instead of the `Parse` functions we used in the first version of the program,
we now employ their `TryParse` counterparts which do not throw exceptions.
They return a tuple of a `bool` and another value. On success, the first element
of the tuple is `true` and the second one is the parsed value. On failure, the
first element of the tuple is `false` and the second one is a default value.

We introduced the `option` values, but what about now? Well, there are many ways
to combine `option` values. We will start by looking at the simplest one. Let's add
the following lines to the file:

```fsharp
let earningsOpt monthArg hoursArg rateArg =
    match monthOpt monthArg, hoursOpt hoursArg, rateOpt rateArg with
    | Some _, Some hours, Some rate -> Some(decimal hours * rate)
    | _ -> None

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
```

If all input values are parsed successfully, the result is calculated and printed.
Otherwise a new error message is printed to the user. The current version of the
program seems impossible to crash. Yet, we can improve it further.

## Short Circuit Evaluation

The initial implementation of the `earningsOpt` function does not allow for early exits.
If the `monthOpt monthArg` expression evaluates to `None`, the other expressions within
the tuple will still be evaluated. For the `earningsOpt` function to be as efficient as
possible, we should introduce short circuit evaluation. This means if `monthOpt monthArg`
evaluates to `None`, the evaluation of `hoursOpt hoursArg` and `rateOpt rateArg` should be
skipped. If `hoursOpt hoursArg` evaluates to `None`, the evaluation of `rateOpt rateArg`
should also be skipped. Let's replace the old `earningsOpt` function with this one:

```fsharp
let earningsOpt monthArg hoursArg rateArg =
    match monthOpt monthArg with
    | None -> None
    | Some _ ->
        match hoursOpt hoursArg with
        | None -> None
        | Some hours ->
            match rateOpt rateArg with
            | None -> None
            | Some rate -> Some(decimal hours * rate)
```

This new implementation of the `earningsOpt` function allows for early exits,
enhancing efficiency.

## Computation Expressions for Options

To enhance the readability of the `earningsOpt` function, we can replace the nested pattern
matching with computation expressions. Computation expressions provide a way to handle
effects such as _optionality_, streamlining the error handling process. This makes the code
more declarative and less error-prone. To use computation expressions for error-handling types,
we can install the 
[FsToolkit.ErrorHandling](https://github.com/demystifyfp/FsToolkit.ErrorHandling) package.

```bash
dotnet add package FsToolkit.ErrorHandling
```

With the new package installed, we can re-implement the `earningsOpt` function
in a cleaner manner.

```fsharp
open FsToolkit.ErrorHandling

let earningsOpt monthArg hoursArg rateArg =
    option {
        let! _ = monthOpt monthArg
        let! hours = hoursOpt hoursArg
        let! rate = rateOpt rateArg
        return decimal hours * rate
    }
```

In this implementation, the `let!` keyword of the computation expression syntax in F#,
binds the value contained within an `Option` type to a variable if it's `Some`. It short-circuits
the computation, returning `None` if it's `None`. The `return` keyword wraps a value in a `Some`,
providing a result of the `Option` type for a computation expression block.

Let's check the behavior of our program. If we execute `dotnet run 2022-13 150 30.0`,
`dotnet run 2023-01 150 thirty`, or `dotnet run 2023-01 hundred thirty`,
we'll get `Your input is invalid`.

In the next section, we'll see how we can provide a more descriptive error message to the user.
A message that will clarify what went wrong.

## The Result Type

Strongly-typed functional languages offer distinct types for error handling that encapsulate either a
successful result or an error. In F#, the `Result` type embodies this practice.

To manage potential parsing-related errors, we could encapsulate the outcome in a `Result`.
Here, `Ok value` would represent a successful computation, while `Error "some error message"` would
provide a descriptive error message in case of failure.

Replace the contents of the `Program.fs` file with the following functions
that utilize the `Result` type:

```fsharp
open System
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
```

Let's check the behavior of our program again:
* `dotnet run 2022-13 150 30.0` gives `The first argument should be a valid month in the yyyy-MM format`.
* `dotnet run 2023-01 hundred 30.0` gives `The hours argument should be a positive integer`.
* `dotnet run 2023-01 150 thirty` gives `The rate argument should be a positive decimal`.

## Computation Expressions for Results

With the `FsToolkit.ErrorHandling` package, we can also write computation expressions for the `Result` type.
This simplifies the handling of `Result` types by reducing nesting and improving readability.
To maintain the same behavior, we can replace the implementation of the `earningsRes` function:

```fsharp
open FsToolkit.ErrorHandling

let earningsRes monthArg hoursArg rateArg =
    result {
        let! _ = monthRes monthArg
        let! hours = hoursRes hoursArg
        let! rate = rateRes rateArg
        return decimal hours * rate
    }
```

## Computation Expressions for Validations

This section of the article addresses error aggregation. Instead of only reporting the first error
encountered, we will accumulate all errors and report them to the user. This is a typical validation process,
and fortunately, the `FsToolkit.ErrorHandling` package provides computation expressions for this purpose.

Replace the `earningsRes` and `main` functions with the following code:

```fsharp
let earningsRes monthArg hoursArg rateArg =
    validation {
        let! _ = monthRes monthArg
        and! hours = hoursRes hoursArg
        and! rate = rateRes rateArg
        return decimal hours * rate
    }

[<EntryPoint>]
let main args =
    match args with
    | [| monthArg; hoursArg; rateArg |] ->
        match earningsRes monthArg hoursArg rateArg with
        | Ok earnings ->
            Console.WriteLine $"Your earnings for {monthArg} are {earnings}"
            0
        | Error messages ->
            List.iter (fun (msg: string) -> Console.WriteLine msg) messages
            1
    | _ ->
        Console.WriteLine "Please provide three arguments"
        1
```

In this updated implementation, the `validation` computation expression and the `and!` keyword allow for error
aggregation. If any parsing function fails, its error message gets accumulated. In the `main` function,
we use the `List.iter` function to iterate over the list of error messages and print each one to the console.

Let's check the behavior of our program one more time. If we execute `dotnet run 2022-13 hundred thirty` will
get this:
```
The first argument should be a valid month in the yyyy-MM format
The hours argument should be a positive integer
The rate argument should be a positive decimal
```

## The Elephant in the Room

It's time to say it out loud. Yes, we've been talking about monads! So, for our type-theory
enthusiasts, you can now relax. We've acknowledged the monadic essence of this article.

## Conclusion

We explored error handling in F# by building a simple console application. We started with
a basic version and step by step, introduced better error-handling methods common in
strongly-typed functional programming languages. Through this, we looked into the `Option`
and `Result` types, _short circuit evaluation_, and _computation expressions_.
These tools helped make our application more robust, easier to read, and maintain.
