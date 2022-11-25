use std::borrow::Borrow;
use std::fmt::Debug;
use std::net::{Ipv4Addr, SocketAddrV4};
use std::string::ToString;

use actix_web::{App, get, HttpResponse, HttpServer, post, web};
use actix_web::middleware::Logger;
use mongodb::{Client, Collection, Database};
use mongodb::bson::{DateTime, doc, Document};
use mongodb::bson::oid::ObjectId;
use mongodb::options::{ClientOptions};
use futures::stream::{StreamExt};
use serde::Serialize;

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
        format!("mongodb://{}:{}@{}:{}/{}?w=majority&authSource=admin", self.username, self.password, self.host, self.port, self.database)
    }
}

const MONGO_PARAMS: MongoConnectionParams<'static> = MongoConnectionParams {
    username: "cr",
    password: "cr",
    host: "localhost",
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
                    //Store the mongoDB connection handle as part of the webserver's data
                    .app_data(web::Data::new(AppData { messages: messages.clone() }))
                    // register HTTP requests handlers
                    .service(list)
                    .service(list_from)
                    .service(create)
                    // enable logger - always register actix-web Logger middleware last
                    .wrap(Logger::default())
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
    #[serde(skip_serializing)]
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
pub struct MessageStringDate {
    #[serde(skip_serializing)]
    pub _id: Option<ObjectId>,
    pub time: String,
    pub message: String,
    pub from: String,
    pub to: String,
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

#[post("/api/messages/create/")]
async fn create(request: web::Json<MessageRequest>, data: web::Data<AppData>) -> HttpResponse {
    println!("/api/messages/create: {:?}", request);
    let Some(message) = request.to_message() else {
        println!(" > unable to parse");
        return HttpResponse::BadRequest().json(JsonResponse::failure("Bad request. Required fields: from, to, message"));
    };
    println!(" > parsed: {:?}", message);
    let result = data.messages.insert_one(message, None).await;
    match result {
        Ok(insertion) => {
            println!(" > inserted!");
            HttpResponse::Ok().json(JsonResponse::success(insertion.inserted_id))
        }
        Err(e) => {
            println!(" > unable to insert: {:?}", e);
            HttpResponse::InternalServerError().json(JsonResponse::failure("Unable to write to database"))
        }
    }
}

/// list 50 last tweets `/tweets`
#[get("/api/messages/{USER}")]
async fn list(param: web::Path<(String, )>, data: web::Data<AppData>) -> HttpResponse {
    let user: &String = &param.0;
    println!("/api/messages/USER: {}", user);
    let aggregation: Vec<Document> = vec![
        doc! {
            "$match": doc! {
                "$or": [
                    doc! {
                        "from": user
                    },
                    doc! {
                        "to": user
                    }
                ]
            }
        },
        doc! {
            "$sort": doc! {
                "time": -1
            }
        },
        doc! {
            "$addFields": doc! {
                "contents": [
                    doc! {
                        "from": "$from",
                        "to": "$to",
                        "time": "$time",
                        "message": "$message",
                        "_id": "$_id"
                    },
                    doc! {
                        "from": "$to",
                        "to": "$from",
                        "time": "$time",
                        "message": "$message",
                        "_id": "$_id"
                    }
                ]
            }
        },
        doc! {
            "$project": doc! {
                "_id": 0,
                "contents": 1
            }
        },
        doc! {
            "$unwind": "$contents"
        },
        doc! {
            "$group": doc! {
                "_id": [
                    "$contents.from",
                    "contents.$to"
                ],
                "message": doc! {
                    "$first": "$contents.message"
                },
                "time": doc! {
                    "$first": "$contents.time"
                },
                "from": doc! {
                    "$first": "$contents.from"
                },
                "to": doc! {
                    "$first": "$contents.to"
                },
                "id": doc! {
                    "$first": "$contents._id"
                }
            }
        },
        doc! {
            "$project": doc! {
                "_id": 0
            }
        },
        doc! {
            "$group": doc! {
                "_id": "$id",
                "message": doc! {
                    "$first": "$message"
                },
                "time": doc! {
                    "$first": "$time"
                },
                "from": doc! {
                    "$first": "$from"
                },
                "to": doc! {
                    "$first": "$to"
                }
            }
        },
        doc! {
            "$sort": doc! {
                "time": -1
            }
        },
        doc! {
            "$project": doc! {
                "time": doc! {
                    "$dateToString": doc! {
                        "date": "$time"
                    }
                },
                "message": 1,
                "from": 1,
                "to": 1
            }
        }
    ];

    send_response(aggregation, &data.messages).await
}

#[get("/api/messages/{USER1}/{USER2}")]
async fn list_from(param: web::Path<(String, String)>, data: web::Data<AppData>) -> HttpResponse {
    let user1: &String = &param.0;
    let user2: &String = &param.1;

    println!("/api/messages/USER1/USER2: {}, {}", user1, user2);

    let aggregation: Vec<Document> =
        vec![
            doc! {
                "$match": doc! {
                    "$or": [
                        doc! {
                            "from": user1,
                            "to": user2
                        },
                        doc! {
                            "from": user2,
                            "to": user1
                        }
                    ]
                }
            },
            doc! {
                "$sort": doc! {
                    "time": -1
                }
            },
            doc! {
                "$project": doc! {
                    "time": doc! {
                        "$dateToString": doc! {
                            "date": "$time"
                        }
                    },
                    "message": 1,
                    "from": 1,
                    "to": 1
                }
            }
        ];

    send_response(aggregation, &data.messages).await
}

#[derive(Debug, Serialize)]
struct JsonResponse<T: Debug + Serialize> {
    success: bool,
    data: T,
}

impl<T: Debug + Serialize> JsonResponse<T> {
    fn success(data: T) -> Self {
        Self {
            success: true,
            data,
        }
    }
}

impl JsonResponse<String> {
    fn failure(message: impl ToString) -> Self {
        Self {
            success: false,
            data: message.to_string(),
        }
    }
}

async fn send_response(aggregation: Vec<Document>, messages: &Collection<Message>) -> HttpResponse {
    match messages.aggregate(aggregation, None).await {
        Ok(mut cursor) => {
            println!(" > found");
            let mut messages: Vec<MessageStringDate> = vec!();

            while let Some(result) = cursor.next().await {
                match result {
                    Ok(document) => {
                        match mongodb::bson::from_document(document.clone()) {
                            Ok(message) => {
                                messages.push(message)
                            }
                            Err(e) => {
                                println!("Error parsing bson to result: {:?} -> {:?}", document, e)
                            }
                        }
                    }
                    Err(_) => {}
                }
            }
            HttpResponse::Ok()
                .json(JsonResponse::success(messages))
        }
        Err(e) => {
            println!(" > unable to find: {:?}", e);
            return HttpResponse::InternalServerError().json(JsonResponse::failure("Unable to fetch from the database"));
        }
    }
}