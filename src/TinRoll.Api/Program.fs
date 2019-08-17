
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Writers
open Suave.Json
open System
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Microsoft.FSharp.Reflection

//https://stackoverflow.com/questions/28150908/f-json-webapi-serialization-of-option-types
type OptionConverter() =
    inherit JsonConverter()

    override x.CanConvert(t) = 
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

    override x.WriteJson(writer, value, serializer) =
        let value = 
            if value = null then null
            else 
                let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
                fields.[0]  
        serializer.Serialize(writer, value)

    override x.ReadJson(reader, t, existingValue, serializer) =        
        let innerType = t.GetGenericArguments().[0]
        let innerType = 
            if innerType.IsValueType then (typedefof<Nullable<_>>).MakeGenericType([|innerType|])
            else innerType        
        let value = serializer.Deserialize(reader, innerType)
        let cases = FSharpType.GetUnionCases(t)
        if value = null then FSharpValue.MakeUnion(cases.[0], [||])
        else FSharpValue.MakeUnion(cases.[1], [|value|])
type Post =
    {
        Text: string
        UserId: int
    }

type Question =
    {
        Post: Post
        Title: string
    }

type Answer = 
    {
        Post: Post
        AnswerId: int option
    }

let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>, new OptionConverter()) :?> 'a
let toJson obj =
    JsonConvert.SerializeObject(obj, new OptionConverter())
let getString (rawForm: byte[]) =
    System.Text.Encoding.UTF8.GetString(rawForm)

let getAnswerFromDb id =
    let post = { Text = "yes"; UserId = 1 }
    { Post = post; AnswerId = Some id}
let upsertAnswer answer =
    match answer.AnswerId with
    | Some x -> x
    | None -> 1

let createAnswer =
    request (fun r -> 
    r.rawForm 
    |> getString 
    |> fromJson<Answer> 
    |> upsertAnswer
    |> (fun id -> id.ToString())
    |> OK )
    >=> setMimeType "application/json"

   
let getAnswer id =
    getAnswerFromDb id
    |> toJson
    |> OK
    >=> setMimeType "application/json"

let getOptionalAnswerId id =
    OK "what's up"

let basicHello =
    request (fun r -> OK "{ \"hello\": \"value\" }")
    >=> setMimeType "application/json"

let app =
    choose
        [ GET >=> choose
            [ path "/" >=> OK "Health Check" 
              path "/hello" >=> basicHello 
              pathScan "/answer/%d" (fun id -> getAnswer id) ]
          POST >=> choose
            [ path "/question" >=> OK "Hello POST!" 
              path "/answer" >=> createAnswer ]
        ]

[<EntryPoint>]
let main argv =
    startWebServer defaultConfig app
    0