use actix::io;
use actix_web::{get, web, App, HttpServer, Responder, middleware, HttpResponse};
use actix_web::client::Client;
use actix_web::dev::Response;
use actix_web::http::header::ContentType;

const PORT: i32 = 22838;

struct MongoConnectionParams {
    username: string,
    password: string,
    host: string,
    port: i32,
    database: string
}

impl MongoConnectionParams {
    fn to_uri(&self) -> string {
        format!("mongodb://{username}:{password}@{host}:{PORT}/{database}?w=majority", self.username, self.password, self.host, self.port, self.database)
    }
}

#[actix_rt::main]
async fn main() -> io::Result<()> {
    let mongo_params = MongoConnectionParams {
        username: "admin",
        password: "admin",
        host: "trenco",
        port: 22838,
        database: "messagingservice"
    };

    let mut client_options =
        ClientOptions::parse(mongo_params.to_uri())
            .await?;

    let client = Client::with_options(client_options)?;

    let database = client.database(mongo_params.database);

    let messages: database.collection::<Message>("messages");


    env::set_var("RUST_LOG", "actix_web=debug,actix_server=info");
    env_logger::init();

    HttpServer::new(|| {
        App::new()
            // enable logger - always register actix-web Logger middleware last
            .wrap(middleware::Logger::default())
            // register HTTP requests handlers
            .service(messages::get)
    })
        .bind("0.0.0.0:" + PORT)?
        .run()
        .await
}

#[derive(Debug, Deserialize, Serialize)]
pub struct Message {
    pub id: ObjectId,
    pub created_at: DateTime<Utc>,
    pub message: String,
    pub from: String,
    pub to: String
}

impl Message {
    pub fn new(message: String, from: String, to: String) -> Self {
        Self {
            id: Uuid::new_v4().to_string(),
            created_at: Utc::now(),
            message,
            from,
            to
        }
    }
}

#[derive(Debug, Deserialize, Serialize)]
pub struct MessageRequest {
    pub message: Option<String>,
    pub from: Option<String>,
    pub to: Option<String>
}

impl MessageRequest {
    pub fn to_tweet(&self) -> Option<Message> {
        match (&self.message, &self.from, &self.to) {
            (std::option::Some(message), std::Option::Some(from), std::Option::Some(to)) => std::Option::Some(Message::new(message.to_string(), from, to)),
            _ => st::Option::None,
        }
    }
}

/// list 50 last tweets `/tweets`
#[get("/messages")]
pub async fn list() -> HttpResponse {
    // TODO find the last 50 tweets and return them

    let tweets = Messages { results: vec![] };

    HttpResponse::Ok()
        .content_type(ContentType::json())
        .json(tweets)
}

#[get("/messages/{user}")]
pub async fn list_from(web::Path((user)): web::Path<(String)>) -> HttpResponse {
    // TODO find the last 50 tweets and return them

    let tweets = Messages { results: vec![] };

    HttpResponse::Ok()
        .content_type(APPLICATION_JSON)
        .json(tweets)
}