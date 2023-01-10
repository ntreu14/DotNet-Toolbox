module Types

open System.Collections.Generic

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


type 'a NonEmptyList = 
  private
    {
      Head: 'a
      Tail: 'a list
    }
  
    member this.Length = 1 + List.length this.Tail
    
    member this.Item =
      function
      | 0 -> this.Head
      | n -> this.Tail[n-1]

    interface IReadOnlyCollection<'a> with
      member this.Count = this.Length
      member this.GetEnumerator(): IEnumerator<'a> =
        (seq {
          yield this.Head
          yield! this.Tail
        }).GetEnumerator()
          
      member this.GetEnumerator(): System.Collections.IEnumerator =
        (seq {
          yield this.Head
          yield! this.Tail
        }).GetEnumerator()
        :> System.Collections.IEnumerator

    interface IReadOnlyList<'a> with
      member this.Item with get index = this.Item index

[<RequireQualifiedAccess>]
module NonEmptyList =
  let create head tail = { Head=head; Tail=tail }

  let fromList =
    function
    | [] -> None
    | head::tail -> Some { Head = head; Tail = tail }

  let singleton head = { Head = head; Tail = [] } 

  let head { Head=head } = head

  let tail { Tail=tail } = tail
  
  let cons x { Head=head; Tail=tail } =
    { Head = x; Tail = head :: tail }

  let toList { Head=head; Tail=tail } = head :: tail

  let ofList =
    function
    | [] -> None
    | x::xs -> create x xs |> Some
  
  let toSeq { Head=head; Tail=tail } =
    seq {
        yield head
        yield! tail
      }
  
  let ofSeq seq = Seq.toList seq |> ofList
    
  let toArray nel = toList nel |> List.toArray
  
  let ofArray arr = Array.toList arr |> ofList
  
  let map f { Head=head; Tail=tail } =
    { Head = f head; Tail = List.map f tail }

  let collect f { Head=head; Tail=tail } =
    let { Head = firstHead; Tail = firstTail } = f head
    let secondTail = tail |> List.collect (f >> toList)
    { Head = firstHead; Tail = firstTail @ secondTail }

  let apply l1 l2 =
    l1 |> collect (fun f -> map f l2)

  let fold f state { Head=head; Tail=tail } =
    List.fold f (f state head) tail