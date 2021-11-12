module Utils

open System

let inline always a _ = a

let inline flip f a b = f b a

let inline curry f a b = f (a, b)

let inline uncurry f (a, b) = f a b

let inline swap (a, b) = (b, a)
 
let inline (<->) (a, b) = b, a

let inline (=>) a b = (a, b)
 
let inline tee f x =
  f x |> ignore
  x

let inline (|>!) x f = tee f x

let tryUnsafe (potentiallyUnsafe: unit -> 'a) (onException: exn -> 'a option) : 'a option =
  try
    potentiallyUnsafe () |> Some
  with 
    | ex -> onException ex

let tryUnsafeOrNone (potentiallyUnsafe: unit -> 'a) : 'a option =
  tryUnsafe potentiallyUnsafe <| always None

let tryUnsafeResult (potentiallyUnsafe: unit -> 'a) (onException: exn -> 'b) : Result<'a, 'b> =
  try
    potentiallyUnsafe () |> Ok
  with
    | ex -> onException ex |> Error

let tryUnsafeOrExceptionMsg (potentiallyUnsafe: unit -> 'a) =
  tryUnsafeResult potentiallyUnsafe <| fun ex -> ex.Message
  
let tryUnsafeOrException (potentiallyUnsafe: unit -> 'a) =
  tryUnsafeResult potentiallyUnsafe id

let tryParse parser (str: string) =
  match parser str with
    | (true, v) -> Some v
    | _         -> None

let tryParseInt = tryParse Int32.TryParse

let tryParseDateTime = tryParse DateTime.TryParse

let tryParseDecimal = tryParse Decimal.TryParse

let undefined<'a> : 'a = raise <| NotImplementedException "undefined result"

// Option
type OptionBuilder() =
  member this.Bind(opt, f) = Option.bind f opt
  member this.Return(v) = Some v
  member this.ReturnFrom(v) = v
  member this.Zero() = None

let option = OptionBuilder()

let inline lift2Option f a b = option {
  let! a' = a
  let! b' = b
  return f a' b'
}

// Async
let inline asyncMap f asyncOp = async {
  let! asyncOp' = asyncOp
  return f asyncOp'
}

let inline asyncBind f asyncOp = async {
  let! asyncOp' = asyncOp
  return! f asyncOp'
}

let inline lift2Async f a b = async {
  let! a' = a
  let! b' = b
  return f a' b'  
}
