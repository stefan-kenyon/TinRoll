
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Writers
open Suave.Json
open System
open Newtonsoft.Json

type Post =
    {
        Text: string
        UserId: int
        CreatedDate: DateTime
    }

type Question =
    {
        Post: Post
        Title: string
    }

type Answer = 
    {
        Post: Post
        AnswerId: int
    }

let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

let getAnswerFromRequest(req : HttpRequest) =
    let getString (rawForm: byte[]) =
        System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm |> getString |> fromJson<Answer>



let basicHello =
    request (fun r -> OK "{ \"hello\": \"value\" }")
    >=> setMimeType "application/json"

let createQuestion =
    request (getAnswerFromRequest |> (fun a -> OK a.)
let app =
    choose
        [ GET >=> choose
            [ path "/" >=> OK "Index"
              path "/hello" >=> basicHello ]
          POST >=> choose
            [ path "/question" >=> OK "Hello POST!" ] ]

[<EntryPoint>]
let main argv =
    startWebServer defaultConfig app
    0