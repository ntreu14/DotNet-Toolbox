module Types

type NonEmptyString = 
  private NonEmptyString of string

[<RequireQualifiedAccess>]
module NonEmptyString =

  let fromString str =
    if System.String.IsNullOrEmpty str
    then None
    else Some <| NonEmptyString str

  let toString (NonEmptyString str) = str

  let mapString f (NonEmptyString s) =
    NonEmptyString <| f s

  let concatBy f (NonEmptyString s1) (NonEmptyString s2) =
    NonEmptyString <| f s1 s2

  let concatStrings = 
    concatBy <| sprintf "%s%s"

  let splitOn separator (NonEmptyString str) =
    str.Split separator |> Array.choose fromString

  let startsWith (s: string) (NonEmptyString str) = 
    str.StartsWith s

  let endsWith (s: string) (NonEmptyString str) =
    str.EndsWith s

  let trim (NonEmptyString str) =
    NonEmptyString <| str.Trim ()


type NonEmptyList<'a> = 
  private NonEmptyList of ('a * 'a list)

[<RequireQualifiedAccess>]
module NonEmptyList =

  let create head tail = NonEmptyList (head, tail)

  let fromList = function
    | [] -> None
    | head::tail -> Some <| NonEmptyList (head, tail)

  let singleton v = NonEmptyList (v, []) 

  let head (NonEmptyList (head, _)) = head

  let tail (NonEmptyList (_, tail)) = tail

  let toList (NonEmptyList (head, tail)) = head :: tail

  let map f (NonEmptyList (head, tail)) =
    NonEmptyList (f head, List.map f tail)

  let bind f (NonEmptyList (head, tail)) =
    let (NonEmptyList (a, b)) = f head
    let c = List.collect (toList << f) tail
    NonEmptyList (a, List.append b c)

  let apply l1 l2 =
    l1 |> bind (fun f -> map f l2)

  let fold f state (NonEmptyList (head, tail)) =
    List.fold f (f state head) tail

type Either<'a, 'b> =
  | Left of 'a
  | Right of 'b

[<RequireQualifiedAccess>]
module Either =

  let lift = Right

  let fromLeft aDefault = function
    | Left e -> e
    | Right _ -> aDefault

  let fromRight aDefault = function
    | Left _ -> aDefault
    | Right r -> r

  let isLeft = function
    | Left _ -> true
    | _ -> false

  let isRight = function
    | Right _ -> true
    | _ -> false

  let lefts eithers = 
    eithers |> Seq.choose (function Left e -> Some e | _ -> None)
 
  let rights eithers = 
    eithers |> Seq.choose (function Right r -> Some r | _ -> None)

  let map f = function
    | Left e -> Left e
    | Right r -> f r |> Right

  let biMap (f: 'a -> 'c) (g: 'b -> 'd) = function
    | Left e -> f e |> Left
    | Right r -> g r |> Right

  let first (f: 'a -> 'b) = biMap f id 

  let second (f: 'b -> 'c) = biMap id f

  let bind f = function
    | Left e -> Left e
    | Right r -> f r

  let apply e1 e2 =
    e1 |> bind (fun f -> map f e2) 

  let liftA2 f = apply << map f 

  let fold f state = function
    | Left _ -> state
    | Right r -> f state r

  let either (f: 'a -> 'c) (g: 'b -> 'c) = function
    | Left e -> f e
    | Right r -> g r

  let partitionEithers eithers =
    let addLeft (l, r) a = (a::l, r)
    let addRight (l, r) a = (l, a::r)

    eithers |> Seq.fold (fun state -> either (addLeft state) (addRight state)) ([], [])  

  let executeSideEffect (sideEffect: 'b -> unit) = function
    | Left l -> Left l
    | Right r ->
        sideEffect r
        Right r


type EitherBuilder() =
  member this.Return(v) = Right v
  member this.ReturnFrom(either) = either 
  member this.Bind(either, f) = Either.bind f either

let either = EitherBuilder()
