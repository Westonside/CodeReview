#![feature(let_else)]

use std::borrow::Borrow;
use std::net::{Ipv4Addr, SocketAddrV4};
use std::string::ToString;

use actix_web::{App, get, HttpResponse, HttpServer, middleware, post, web};
use actix_web::http::header::ContentType;
use mongodb::{Client, Collection, Database};
use mongodb::bson::{DateTime, doc, Document};
use mongodb::bson::oid::ObjectId;
use mongodb::options::{ClientOptions, FindOptions};
use futures::stream::{StreamExt};

const PORT: u16 = 22838;

struct MongoConnectionParams<'a> {
    username: &'a str,
    password: &'a str,
    host: &'a str,
    port: u16,
    database: &'a str,
}

impl MongoConnectionParams<'_> {
    fn to_uri(&self) -> String {
        format!("mongodb://{}:{}@{}:{}/{}?w=majority", self.username, self.password, self.host, self.port, self.database)
    }
}

const MONGO_PARAMS: MongoConnectionParams<'static> = MongoConnectionParams {
    username: "cr",
    password: "cr",
    host: "palain",
    port: 24099,
    database: "cr",
};

struct AppData {
    messages: Collection<Message>,
}

#[derive(Debug)]
enum CompositeError {
    Mongo(mongodb::error::Error),
    Actix(std::io::Error),
}

#[actix_rt::main]
async fn main() -> Result<(), CompositeError> {
    match connect_db().await {
        Ok(messages) => {
            let server_bind = HttpServer::new(move || {
                App::new()
                    // enable logger - always register actix-web Logger middleware last
                    .wrap(middleware::Logger::default())
                    // register HTTP requests handlers
                    .app_data(web::Data::new(AppData { messages: messages.clone() }))
                    .service(list)
                    .service(list_from)
                    .service(create)
            })
                .bind(SocketAddrV4::new(Ipv4Addr::LOCALHOST, PORT));
            match server_bind {
                Ok(server) => {
                    match server.run().await {
                        Ok(()) => Ok(()),
                        Err(e) => Err(CompositeError::Actix(e))
                    }
                }
                Err(e) => {
                    Err(CompositeError::Actix(e))
                }
            }
        }
        Err(e) => {
            Err(CompositeError::Mongo(e))
        }
    }
}

async fn connect_db() -> Result<Collection<Message>, mongodb::error::Error> {
    let client_options: ClientOptions = ClientOptions::parse(MONGO_PARAMS.to_uri()).await?;

    let client = Client::with_options(client_options)?;
    let database: Database = client.database(MONGO_PARAMS.database.borrow());

    Ok(database.collection::<Message>("messages"))
}

#[derive(Debug, serde::Deserialize, serde::Serialize)]
pub struct Message {
    pub _id: Option<ObjectId>,
    pub time: DateTime,
    pub message: String,
    pub from: String,
    pub to: String,
}

impl Message {
    pub fn new(message: String, from: String, to: String) -> Self {
        Self {
            _id: None,
            time: DateTime::now(),
            message,
            from,
            to,
        }
    }
}

#[derive(Debug, serde::Deserialize, serde::Serialize)]
pub struct MessageRequest {
    pub message: Option<String>,
    pub from: Option<String>,
    pub to: Option<String>,
}

impl MessageRequest {
    pub fn to_message(&self) -> Option<Message> {
        match (&self.message, &self.from, &self.to) {
            (Some(message), Some(from), Some(to)) => Some(Message::new(message.to_string(), from.to_string(), to.to_string())),
            _ => None,
        }
    }
}

/// list 50 last tweets `/tweets`
#[get("/api/messages/{user}")]
async fn list(param: web::Path<(String, )>, data: web::Data<AppData>) -> HttpResponse {
    let filter = doc! {"$or": [{"from": &param.0}, {"from": &param.0}]};
    let find_options: FindOptions = FindOptions::builder().sort(doc! {"time": -1}).build();

    send_response(filter, find_options, &data.messages).await
}

#[get("/api/messages/{user1}/{user2}")]
async fn list_from(param: web::Path<(String, String)>, data: web::Data<AppData>) -> HttpResponse {
    let filter: Document = doc! {"$or": [{"from": &param.0, "to": &param.1}, {"from": &param.0, "to": &param.1}]};
    let find_options: FindOptions = FindOptions::builder().sort(doc! {"time": -1}).build();

    send_response(filter, find_options, &data.messages).await
}

#[post("/api/messages/create/")]
async fn create(request: web::Json<MessageRequest>, data: web::Data<AppData>) -> HttpResponse {
    let Some(message) = request.to_message() else {
        return HttpResponse::InternalServerError().finish();
    };
    let result = data.messages.insert_one(message, None).await;
    match result {
        Ok(insertion) => {
            HttpResponse::Ok().content_type(ContentType::json()).json(insertion.inserted_id)
        }
        Err(_) => {
            HttpResponse::InternalServerError().finish()
        }
    }
}

async fn send_response(filter: Document, find_options: FindOptions, messages: &Collection<Message>) -> HttpResponse {
    let Ok(mut cursor) = messages.find(filter, find_options).await else {
        return HttpResponse::InternalServerError().finish();
    };
    let mut messages: Vec<Message> = vec!();

    while let Some(message) = cursor.next().await {
        match message {
            Ok(e) => {
                messages.push(e)
            }
            Err(_) => {}
        }
    }
    HttpResponse::Ok()
        .content_type(ContentType::json())
        .json(messages)
}