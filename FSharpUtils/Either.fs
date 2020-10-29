module Either

type Either<'a, 'b> = 
  | Left of 'a
  | Right of 'b 

let map f = function
  | Left l -> Left l
  | Right r -> f r |> Right

let bind f = function
  | Left l -> Left l
  | Right r -> f r

let lift2Either (f: 'a -> 'b -> 'c) (a: Either<'e, 'a>) (b: Either<'e, 'b>) : Either<'e, 'c> = 
  a |> bind (fun a' -> b |> map (fun b' -> f a' b'))

let isRight = function
  | Left _ -> false
  | Right _ -> true

let isLeft = function
  | Left _ -> true
  | Right _ -> false

let fromLeft aDefault = function
  | Left l -> l
  | Right _ -> aDefault

let fromRight aDefault = function
  | Left _ -> aDefault
  | Right r -> r

let either (f: 'a -> 'c) (g: 'b -> 'c) = function
  | Left l -> f l
  | Right r -> g r

let rights eithers = eithers |> Seq.choose (function Right r -> Some r | _ -> None)

let lefts eithers = eithers |> Seq.choose (function Left l -> Some l | _ -> None)

let partitionEithers eithers =
  let left (l, r) a = (a::l, r)
  let right (l, r) a = (l, a::r)
  
  eithers |> Seq.fold (fun acc currentEither -> either (left acc) (right acc) currentEither) ([], [])


type EitherBuilder() =

  member this.Return(v) = Right v

  member this.Bind(either, f) = bind f either


let eitherWorkflow = EitherBuilder()
